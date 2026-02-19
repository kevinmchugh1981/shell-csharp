public class Bash(IInstructionIParser instructionIParser, IFileSystem fileSystem, IBuiltins builtins, IKeyboard keyboard)
{
    
    public void Start()
    {
        while (true)
        {
            Console.Write("$ ");
            var input = instructionIParser.ParseAlt(keyboard.GetInput());
            
            switch (string.IsNullOrWhiteSpace(input.CommandName))
            {
                case false when builtins.Commands.TryGetValue(input.CommandName, out var command):
                    command.Invoke(input);
                    break;
                case false when fileSystem.IsExecutable(input.CommandName, out var path):
                    fileSystem.Execute(path, input);
                    break;
                case false:
                    input.WriteErrorLine($"{input.CommandName}: command not found");
                    break;
            }
        }
    }
}