using System.Runtime.InteropServices;

class Program
{

    private static readonly List<string> BuiltIns = [ExitName, EchoName, TypeName];
    private static string ExitName => "exit";
    private static string TypeName => "type";
    private static string EchoName => "echo";
    
    static void Main()
    {

        while (true)
        {
            Console.Write("$ ");
            var input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input) && input.Equals(ExitName, StringComparison.InvariantCultureIgnoreCase))
                return;
            switch (string.IsNullOrWhiteSpace(input))
            {
                case false when
                    input!.StartsWith(EchoName, StringComparison.InvariantCultureIgnoreCase):
                    Console.WriteLine(input.Replace($"{EchoName} ", string.Empty));
                    break;
                case false when
                    input.StartsWith(TypeName, StringComparison.InvariantCultureIgnoreCase):
                {
                    var target = input.Replace($"{TypeName} ", string.Empty);
                    switch (string.IsNullOrWhiteSpace(target))
                    {
                        case false when BuiltIns.Contains(target):
                            Console.WriteLine($"{target} is a shell builtin");
                            break;
                        case false when IsExecutable(target, out var filePath):
                            Console.WriteLine($"{target} is {filePath}");
                            break;
                        default:
                            Console.WriteLine($"{target}: not found");
                            break;
                    }
                    break;
                }
                default:
                    Console.WriteLine( $"{input}: command not found");
                    break;
            }
        }
    }
    private static bool IsExecutable(string name, out string filePath)
    {
        filePath = string.Empty;
        
        if (!TryGetDirectories(out var directories))
            return false;       

        foreach (var directory in directories.Where(x=> x.Contains("Windows")))  
        {
            var fullPath = Path.Combine(directory.Trim(), name);

            if (IsFileExecutable(fullPath))
            {
                filePath = fullPath;
                return true;
            }
        }

        return  false;
    }

    private static bool TryGetDirectories(out List<string> directories)
    {
        directories = [];
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathEnv)) return false;
        var pathSeparator = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ';' : ':';

        directories = pathEnv.Split(pathSeparator).ToList();
        return directories.Count > 0;
    }
    
    private static bool IsFileExecutable(string filePath)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var ext = Path.GetExtension(filePath).ToLower();
            string[] executableExts = { ".exe", ".bat", ".cmd", ".ps1" };
            return executableExts.Contains(ext);
        }

        var fileInfo = new FileInfo(filePath);

        return (fileInfo.UnixFileMode & (UnixFileMode.UserExecute |
                                         UnixFileMode.GroupExecute |
                                         UnixFileMode.OtherExecute)) != 0;
    }
}
