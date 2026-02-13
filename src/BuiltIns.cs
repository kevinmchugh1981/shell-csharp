internal static class BuiltIns
{
    internal static readonly Dictionary<string, Action<string>> Commands = new(StringComparer.InvariantCultureIgnoreCase)
    {
        { Constants.ExitName, (_) => Environment.Exit(0) },
        { Constants.EchoName, EchoCommand   },
        { Constants.TypeName, TypeCommand },
        { Constants.PwdName, (_)=>  Console.WriteLine(Directory.GetCurrentDirectory()) },
        { Constants.ChangeDirectoryName, ChangeDirectoryCommand }
    };

    private static void EchoCommand(string input)
    {
        var text = input.Replace($"{Constants.EchoName} ", string.Empty);
        Console.WriteLine(string.Join(" ", Parsers.Parse(text)));
    }

    private static void TypeCommand(string input)
    {
        var targetFile = input.Replace($"{Constants.TypeName} ", string.Empty);
        switch (string.IsNullOrWhiteSpace(targetFile))
        {
            case false when Commands.ContainsKey(targetFile):
                Console.WriteLine($"{targetFile} is a shell builtin");
                break;
            case false when FileSystem.IsExecutable(targetFile, out var filePath):
                Console.WriteLine($"{targetFile} is {filePath}");
                break;
            default:
                Console.WriteLine($"{targetFile}: not found");
                break;
        }
    }
    
    private static void ChangeDirectoryCommand(string input)
    {
        var targetDirectory = input.Split(" ")?.Skip(1)?.ToArray()[0] ?? string.Empty;
        if (targetDirectory.Equals(Constants.ChangeDirectorySwitch,  StringComparison.InvariantCultureIgnoreCase))
        {
            Directory.SetCurrentDirectory(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            return;
        }

        if (!string.IsNullOrWhiteSpace(targetDirectory) && Directory.Exists(targetDirectory))
            Directory.SetCurrentDirectory(targetDirectory);
        else
            Console.Out.WriteLine($"cd: {targetDirectory}: No such file or directory");
    }


}