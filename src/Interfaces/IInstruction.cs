public interface IInstruction
{
    string Command { get; set; }
    
    List<string> Args { get; set; }
    
    string RedirectDestination { get; set; }

    void Write(string input);

    void WriteLine(string input);
}