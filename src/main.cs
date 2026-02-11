using System.Diagnostics;
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
            if (!string.IsNullOrWhiteSpace(input) &&
                input.Equals(ExitName, StringComparison.InvariantCultureIgnoreCase))
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
                    if (IsExecutable(input.Split(" ")[0], out var path))
                        Execute(path, string.Join(string.Empty, input.Split().Skip(1)));
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
}