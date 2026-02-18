public class Instruction : IInstruction
{
    public string CommandName { get; set; } = string.Empty;

    public List<string> Arguments { get; set; } = [];

    public string RedirectDestination { get; set; } = string.Empty;
    
    public void Write(string input)
    {
        if (!string.IsNullOrEmpty(RedirectDestination))
            WriteToFile(input);
        else
            Console.Write(input);
    }

    public void WriteLine(string input)
    {
        if (!string.IsNullOrEmpty(RedirectDestination))
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
