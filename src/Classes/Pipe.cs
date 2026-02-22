using System.Diagnostics;

public class Pipe(List<IInstruction> instructions, IFileSystem fileSystem) : IPipe
{
    public void ExecutePipe()
    {
        if (instructions.Count != 2 || !fileSystem.IsExecutable(instructions[0].CommandName, out var process1Path) ||
            !fileSystem.IsExecutable(instructions[1].CommandName, out var process2Path))
            return;

        var process1 = instructions[0].ToStartInfo(process1Path);
        var process2 = instructions[1].ToStartInfo(process2Path);

        // 1. Setup Redirections
        process1.RedirectStandardOutput = true;
        process2.RedirectStandardInput = true;
        process2.RedirectStandardOutput = true; // If you want to see the final result

        // 2. Start both processes
        using var procA = Process.Start(process1);
        using var procB = Process.Start(process2);

        // 3. Start the bridge in the background
        var bridgeTask = Task.Run(async () =>
        {
            try
            {
                await procA.StandardOutput.BaseStream.CopyToAsync(procB.StandardInput.BaseStream);
            }
            catch (IOException)
            {
                /* This happens when procB closes its input early (like head -n 5) */
            }
            finally
            {
                procB.StandardInput.Close();
            }
        });

        // 4. Read the output of the LAST process line-by-line (don't use ReadToEnd)
        // This allows you to see data as soon as it arrives
        while (!procB.StandardOutput.EndOfStream)
        {
            var line = procB.StandardOutput.ReadLine();
            if (line != null) Console.WriteLine(line);

            // If you are running 'head -n 5', procB will eventually exit here
        }

        // 5. Cleanup
        procB.WaitForExit();

        // 6. CRITICAL: If procB is done, procA no longer needs to run
        if (!procA.HasExited)
        {
            procA.Kill();
        }
    }
}