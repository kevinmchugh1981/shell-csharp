using System.Diagnostics;

public class Pipe(List<IInstruction> instructions, IFileSystem fileSystem, IBuiltins builtins) : IPipe
{
    public void ExecutePipe()
    {
        switch (instructions.Count)
        {
            case 0:
                return;
            case 1:
                ExecuteSingle(instructions[0]);
                return;
        }

        var producer = instructions[0];
        var consumer = instructions[1];

        // We assume the consumer is an external process (like wc, head, grep)
        if (fileSystem.IsExecutable(consumer.CommandName, out var consumerPath))
        {


            var startInfoB = consumer.ToStartInfo(consumerPath);
            startInfoB.RedirectStandardInput = true;
            startInfoB.RedirectStandardOutput = true;

            using var procB = Process.Start(startInfoB);
            if (procB == null) return;

            // Bridge: Producer writes to B's input
            producer.OutputSink = procB.StandardInput;

            // 1. Start Producer
            var producerTask = Task.Run(() => RunCommand(producer));

            // 2. The Pumper: Read and write in real-time
            var consumerReadTask = Task.Run(async () =>
            {
                SetupStreams(consumer);
                while (!procB.StandardOutput.EndOfStream)
                {
                    var line = await procB.StandardOutput.ReadLineAsync();
                    if (line == null)
                    {
                        continue;
                    }

                    consumer.WriteLine(line);
                    consumer.OutputSink.Flush(); // Push to terminal/tester immediately
                }
            });

            try
            {
                // 3. The Race: Wait for any to finish
                Task.WaitAny(producerTask, consumerReadTask, Task.Run(() => procB.WaitForExit()));
            }
            finally
            {
                // 4. Handshake: Close the bridge
                procB.StandardInput.Close();

                // 5. Cleanup the Producer (The crash was here!)
                try
                {
                    // Use a local variable to avoid race conditions
                    var p = producer.ActiveProcess;
                    if (p != null && !p.HasExited)
                    {
                        p.Kill();
                    }
                }
                catch (InvalidOperationException)
                {
                    // This happens if the process exited between the null check and the call.
                    // It's safe to ignore because it means the process is already gone!
                }
                catch (Exception)
                {
                    /* Catch-all for other OS-level cleanup issues */
                }

                // 6. Wait for tasks to finish
                procB.WaitForExit();
                consumerReadTask.Wait(500);
                producerTask.Wait(200);
            }
        }
        else if (builtins.Commands.TryGetValue(consumer.CommandName, out Action<IInstruction>? value))
        {// We use a MemoryStream to catch the Producer's output and feed it to the Built-in
            using var bridge = new MemoryStream();
            using var writer = new StreamWriter(bridge);
            writer.AutoFlush = true;
            using var reader = new StreamReader(bridge);

            producer.OutputSink = writer;

            // 1. Run the Producer (ls)
            RunCommand(producer);
        
            // 2. Reset bridge for the Consumer to read
            bridge.Position = 0;
            consumer.InputSource = reader; // Ensure your IInstruction has this property!

            // 3. Run the Built-in (type exit)
            SetupStreams(consumer);
            value(consumer);
            
        }

}

    private void ExecuteSingle(IInstruction instruction)
    {
        SetupStreams(instruction);
        switch (string.IsNullOrWhiteSpace(instruction.CommandName))
        {
            case false when builtins.Commands.TryGetValue(instruction.CommandName, out var command):
                command.Invoke(instruction);
                break;
            case false when fileSystem.IsExecutable(instruction.CommandName, out var path):
                fileSystem.Execute(path, instruction);
                break;
            case false:
                instruction.WriteErrorLine($"{instruction.CommandName}: command not found");
                break;
        }
    }

    private void RunCommand(IInstruction instruction)
    {
        if (builtins.Commands.TryGetValue(instruction.CommandName, out var builtIn))
        {
            builtIn(instruction);
        }
        else if (fileSystem.IsExecutable(instruction.CommandName, out var path))
        {
            // For external producers, we need to manually copy their output to the sink
            ExecuteExternalToSink(instruction, path);
        }
    }

    private void ExecuteExternalToSink(IInstruction instruction, string path)
    {
        var info = instruction.ToStartInfo(path);
        info.RedirectStandardOutput = true;

        using var proc = Process.Start(info);
        if (proc == null) return;

        // Store the process so ExecutePipe can kill it if the pipe breaks
        instruction.ActiveProcess = proc;

        try
        {
            // Use a basic loop to move data from the process to the Sink
            // If the Sink (B's input) closes, WriteLine will throw an IOException
            while (!proc.StandardOutput.EndOfStream)
            {
                var line = proc.StandardOutput.ReadLine();
                if (line != null) 
                {
                    instruction.WriteLine(line);
                    // FORCE the data into the pipe immediately
                    instruction.OutputSink.Flush(); 
                }
            }
        }
        catch (IOException)
        {
            // This is a "Broken Pipe" - expected if the consumer (head) exits early
        }
        finally
        {
            if (!proc.HasExited) proc.Kill();
        }
    }

    private static void SetupStreams(IInstruction instruction)
    {
        if (string.IsNullOrWhiteSpace(instruction.RedirectDestination))
            return;

        switch (instruction.Redirect)
        {
            case Redirect.AppendError:
                instruction.ErrorSink = GenerateStream(instruction.RedirectDestination, true);
                break;
            case Redirect.AppendOutput:
                instruction.OutputSink = GenerateStream(instruction.RedirectDestination, true);
                break;
            case Redirect.Error:
                instruction.ErrorSink = GenerateStream(instruction.RedirectDestination, false);
                break;
            case Redirect.Output:
                instruction.OutputSink = GenerateStream(instruction.RedirectDestination, false);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static StreamWriter GenerateStream(string destination, bool append)
    {
        return new StreamWriter(new FileStream(destination, append ? FileMode.Append : FileMode.Create,
            FileAccess.Write)) { AutoFlush = true };
    }
}