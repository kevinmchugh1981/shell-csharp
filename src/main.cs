using System.Diagnostics;
using System.Runtime.InteropServices;

class Program
{
    private static readonly List<string> BuiltIns = [ExitName, EchoName, TypeName, PwdName, ChangeDirectoryTitle];
    private static string ExitName => "exit";
    private static string TypeName => "type";
    private static string EchoName => "echo";
    private static string PwdName => "pwd";
    private static string ChangeDirectoryTitle => "cd";
    
    static void Main()
    {
        while (true)
        {
            Console.Write("$ ");
            var input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input) &&
                input.Equals(ExitName, StringComparison.InvariantCultureIgnoreCase))
                return;
            switch (string.IsNullOrWhiteSpace(input))
            {
                case false when
                    input!.StartsWith(EchoName, StringComparison.InvariantCultureIgnoreCase):
                    Console.WriteLine(ParseEcho(input.Replace($"{EchoName} ", string.Empty)));
                    break;
                case false when
                    input.StartsWith(PwdName, StringComparison.InvariantCultureIgnoreCase):
                    Console.WriteLine(Directory.GetCurrentDirectory());
                    break;
                case false when
                    input.StartsWith(ChangeDirectoryTitle, StringComparison.InvariantCultureIgnoreCase):
                    var targetDirectory = input.Split(" ")?.Skip(1)?.ToArray()[0] ?? string.Empty;
                    if (targetDirectory == "~")
                    {
                        Directory.SetCurrentDirectory(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
                        break;
                    }
                    if (!string.IsNullOrWhiteSpace(targetDirectory) && Directory.Exists(targetDirectory))
                        Directory.SetCurrentDirectory(targetDirectory);
                    else
                        Console.Out.WriteLine($"cd: {targetDirectory}: No such file or directory");
                    break;
                case false when
                    input.StartsWith(TypeName, StringComparison.InvariantCultureIgnoreCase):
                {
                    var targetFile = input.Replace($"{TypeName} ", string.Empty);
                    switch (string.IsNullOrWhiteSpace(targetFile))
                    {
                        case false when BuiltIns.Contains(targetFile):
                            Console.WriteLine($"{targetFile} is a shell builtin");
                            break;
                        case false when IsExecutable(targetFile, out var filePath):
                            Console.WriteLine($"{targetFile} is {filePath}");
                            break;
                        default:
                            Console.WriteLine($"{targetFile}: not found");
                            break;
                    }

                    break;
                }
                default:
                    if (IsExecutable(input.Split(" ")[0], out var path))
                        Execute(path, string.Join(" ", input.Split().Skip(1)));
                    else
                        Console.WriteLine($"{input}: command not found");
                    break;
            }
        }
    }

    private static bool IsExecutable(string name, out string filePath)
    {
        filePath = string.Empty;

        if (!TryGetDirectories(out var directories))
            return false;

        foreach (var fullPath in directories.Select(directory => Path.Combine(directory.Trim(), name))
                     .Where(IsFileExecutable))
        {
            filePath = fullPath;
            return true;
        }

        return false;
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
        if (!File.Exists(filePath))
            return false;

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

    private static void Execute(string filePath, string arguments)
    {
        var command = Path.GetFileName(filePath);
        var directory = Path.GetDirectoryName(filePath);

        var startInfo = new ProcessStartInfo(command, arguments)
        {
            WorkingDirectory = directory,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };
        using var process = System.Diagnostics.Process.Start(startInfo);
        var output = process?.StandardOutput.ReadToEnd();
        var error = process?.StandardError.ReadToEnd();
        process?.WaitForExit();
        if (!string.IsNullOrWhiteSpace(output))
            Console.Write(output);
        if (!string.IsNullOrWhiteSpace(error))
            Console.Write(error);
    }

    private static string ParseEcho(string input)
    {
        var insideSingleQuote = false;
        var result = string.Empty;
        for (int x = 0; x < input.Length; ++x)
        {
            if (input[x] == '\'')
            {
                insideSingleQuote = !insideSingleQuote;
                continue;
            }
            
            if(insideSingleQuote)
                result += input[x];
            else if (!string.IsNullOrWhiteSpace(result) && char.IsWhiteSpace(result.Last()))
            {
                if(!char.IsWhiteSpace(input[x]))
                    result += input[x];
            }
            else
            {
                result += input[x];
            }
        }
        
        return result;
    }
    
}