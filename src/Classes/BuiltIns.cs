internal class BuiltIns(IFileSystem fileSystem) : IBuiltins
{
    
    public  Dictionary<string, Action<Instruction>> Commands => new(StringComparer.InvariantCultureIgnoreCase)
    {
        { Constants.ExitName, (_) => Environment.Exit(0) },
        { Constants.EchoName, EchoCommand   },
        { Constants.TypeName, TypeCommand },
        { Constants.PwdName, PresentWorkingDirectoryCommand   },
        { Constants.ChangeDirectoryName, ChangeDirectoryCommand }
    };

    private void EchoCommand(Instruction instruction)
    {
        instruction.WriteLine(string.Join(" ", instruction.Arguments));
    }

    private void PresentWorkingDirectoryCommand(Instruction instruction)
    {
        instruction.WriteLine(Directory.GetCurrentDirectory());
    }

    private void TypeCommand(Instruction instruction)
    {
        var targetFile = instruction.Arguments.Count == 0 ? string.Empty: instruction.Arguments[0];
        switch (string.IsNullOrWhiteSpace(targetFile))
        {
            case false when Commands.ContainsKey(targetFile):
                instruction.WriteLine($"{targetFile} is a shell builtin");
                break;
            case false when fileSystem.IsExecutable(targetFile, out var filePath):
                instruction.WriteLine($"{targetFile} is {filePath}");
                break;
            default:
                Console.WriteLine($"{targetFile}: not found");
                break;
        }
    }
    
    private void ChangeDirectoryCommand(Instruction instruction)
    {
        var targetDirectory = instruction.Arguments.Count == 0 ? string.Empty : instruction.Arguments[0];
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