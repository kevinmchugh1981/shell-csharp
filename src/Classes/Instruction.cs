public enum Redirect
{
    Error, Output
}


public class Instruction : IInstruction
{
    private string redirectDestination = string.Empty;
    
    public string CommandName { get; set; } = string.Empty;

    public List<string> Arguments { get; set; } = [];

    public string RedirectDestination
    {
        get => redirectDestination;
        set
        {
            redirectDestination = value;
            if(!string.IsNullOrWhiteSpace(redirectDestination) && !File.Exists(redirectDestination))
                File.Create(redirectDestination).Dispose();
        }
    }
    
    public Redirect Redirect { get; set; } = Redirect.Output;
    
    public void Write(string input)
    {
        if (!string.IsNullOrEmpty(RedirectDestination) && Redirect == Redirect.Output)
            WriteToFile(input);
        else
            Console.Write(input);
    }

    public void WriteError(string input)
    {
        if(!string.IsNullOrEmpty(RedirectDestination) && Redirect == Redirect.Error)
            WriteToFile(input);
        else
            Console.Write(input);
    }

    public void WriteLine(string input)
    {
        if (!string.IsNullOrEmpty(RedirectDestination) && Redirect == Redirect.Output)
            WriteToFile(input);
        else
            Console.WriteLine(input);
    }

    public void WriteErrorLine(string input)
    {
        if (!string.IsNullOrEmpty(RedirectDestination) && Redirect == Redirect.Error)
            WriteToFile(input);
        else
            Console.WriteLine(input);
    }
    
    private void WriteToFile(string content)
    {
        if (File.Exists(RedirectDestination))
            File.Delete(RedirectDestination);
        if(!content.EndsWith(Environment.NewLine))
            content += Environment.NewLine;
        File.WriteAllText(RedirectDestination, content);
    }
}
