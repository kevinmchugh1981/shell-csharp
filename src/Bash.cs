public class Bash(IParser parser, IFileSystem fileSystem, IBuiltins builtins)
{
    
    public void Start()
    {
        while (true)
        {
            Console.Write("$ ");
            var input = parser.ParseAlt(Console.ReadLine() ?? string.Empty);
            
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