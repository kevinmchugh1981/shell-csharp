

public interface IFileSystem
{
    bool IsExecutable(string name, out string filePath);
    
    void Execute(string filePath, IInstruction instruction);
    
    bool AutoComplete(string input, out List<string> executableName);

    bool GetLongestCommonPrefix(List<string>? matches, out string result);

}