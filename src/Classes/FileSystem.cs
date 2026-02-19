using System.Diagnostics;
using System.Runtime.InteropServices;

internal class FileSystem : IFileSystem
{
    public bool IsExecutable(string name, out string filePath)
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

    private bool TryGetDirectories(out List<string> directories)
    {
        directories = [];
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathEnv)) return false;
        var pathSeparator = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ';' : ':';

        directories = pathEnv.Split(pathSeparator).ToList();
        return directories.Count > 0;
    }

    private bool IsFileExecutable(string filePath)
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

    public void Execute(string filePath, IInstruction instruction)
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

        foreach (var arg in instruction.Arguments)
            startInfo.ArgumentList.Add(arg);

        using var process = Process.Start(startInfo);
        var outputMessage = process?.StandardOutput.ReadToEnd();
        var errorMessage = process?.StandardError.ReadToEnd();
        process?.WaitForExit();

        if (!string.IsNullOrWhiteSpace(outputMessage))
        {
            instruction.Write(outputMessage);
        }

        if (!string.IsNullOrWhiteSpace(errorMessage))
            instruction.WriteError(errorMessage);
    }

    public bool AutoComplete(string input, out List<string> executableNames)
    {
        executableNames = [];

        if (!TryGetDirectories(out var directories))
            return false;

        foreach (var directory in directories)
        {
            if(!Directory.Exists(directory))
                continue;
            
            foreach (var file in Directory.GetFiles(directory))
            {
                if (Path.GetFileName(file).StartsWith(input, StringComparison.InvariantCultureIgnoreCase) && IsFileExecutable(file))
                {
                    executableNames.Add(Path.GetFileName(file));
                }
            }
        }

        return executableNames.Any();
    }
    
    public bool GetLongestCommonPrefix(List<string>? matches, out string result)
    {
        result = string.Empty;
        if (matches == null || matches.Count == 0) return false;

        // Start with the first match as our baseline
        var prefix = matches[0];

        foreach (var match in matches)
        {
            // Use Ordinal comparison for speed and consistency in tests
            while (!match.StartsWith(prefix, StringComparison.Ordinal))
            {
                if (prefix.Length == 0) break;
                prefix = prefix[..^1];
            }
        }

        result = prefix;
    
        // Return true if we found ANY commonality, 
        // even if it's just one character.
        return !string.IsNullOrEmpty(result);
    }
}