using System.Diagnostics;

public enum Redirect
{
    Error, Output, AppendOutput, AppendError
}


public class Instruction : IInstruction, IDisposable
{
    public string CommandName { get; set; } = string.Empty;

    public List<string> Arguments { get; set; } = [];

    public string RedirectDestination { get; set; } = string.Empty;

    public Redirect Redirect { get; set; } = Redirect.Output;
    
    public TextWriter OutputSink { get; set; } = Console.Out;
    
    public TextWriter ErrorSink { get; set; } = Console.Error;
    
    public TextReader InputSource { get; set; } = Console.In;
    
    public void Write(string input)=> OutputSink.Write(input);
    
    public void WriteError(string input) => ErrorSink.Write(input);
    
    public void WriteLine(string input) =>  OutputSink.WriteLine(input);
    
    public void WriteErrorLine(string input) => ErrorSink.WriteLine(input);
    
    public Process? ActiveProcess { get; set; }
    
    public ProcessStartInfo ToStartInfo(string filePath)
    {
        var command = Path.GetFileName(filePath);
        var directory = Path.GetDirectoryName(filePath);

        var startInfo = new ProcessStartInfo(command)
        {
            WorkingDirectory = directory,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        foreach (var arg in Arguments)
            startInfo.ArgumentList.Add(arg);
        return startInfo;
    }
    
    public void Dispose()
    {
        if (OutputSink != Console.Out)
        {
            OutputSink.Dispose();
        }
    }
}
