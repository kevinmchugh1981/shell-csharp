using System.Diagnostics;

public interface IInstruction
{
    string CommandName { get; set; }
    
    List<string> Arguments { get; set; }
    
    string RedirectDestination { get; set; }

    void Write(string input);

    void WriteLine(string input);
    
    void WriteError(string input);
    
    void WriteErrorLine(string input);
    
    ProcessStartInfo ToStartInfo(string filePath);
    
    Redirect Redirect { get; set; }
    
}