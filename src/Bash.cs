public class Bash(IParser parser, IFileSystem fileSystem, IBuiltins builtins)
{
    
    public void Start()
    {
        while (true)
        {
            Console.Write("$ ");
            var input = parser.ParseAlt(Console.ReadLine() ?? string.Empty);
            
            switch (string.IsNullOrWhiteSpace(input.Command))
            {
                case false when builtins.Commands.TryGetValue(input.Command, out var command):
                    command.Invoke(input);
                    break;
                case false when fileSystem.IsExecutable(input.Command, out var path):
                    fileSystem.Execute(path, input);
                    break;
                case false:
                    Console.WriteLine($"{input.Command}: command not found");
                    break;
            }
        }
    }
}