public class Bash(IInstructionParser instructionParser, IFileSystem fileSystem, IBuiltins builtins, IKeyboard keyboard)
{
    
    public void Start()
    {
        while (true)
        {
            Console.Write("$ ");
            var input = instructionParser.Parse(keyboard.GetInput());

            switch (input.Count)
            {
                case > 1:
                    var pipe = new Pipe(input,  fileSystem);
                    pipe.ExecutePipe();
                    break;
                case 1:
                    switch (string.IsNullOrWhiteSpace(input[0].CommandName))
                    {
                        case false when builtins.Commands.TryGetValue(input[0].CommandName, out var command):
                            command.Invoke(input[0]);
                            break;
                        case false when fileSystem.IsExecutable(input[0].CommandName, out var path):
                            fileSystem.Execute(path, input[0]);
                            break;
                        case false:
                            input[0].WriteErrorLine($"{input[0].CommandName}: command not found");
                            break;
                    }

                    break;
            }
        }
    }
}