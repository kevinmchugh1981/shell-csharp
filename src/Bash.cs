public class Bash(IInstructionParser instructionParser, IFileSystem fileSystem, IBuiltins builtins, IKeyboard keyboard)
{
    
    public void Start()
    {
        while (true)
        {
            Console.Write("$ ");
            var input = instructionParser.Parse(keyboard.GetInput());

            if (!input.Any())
                continue;

            var pipe = new Pipe(input,  fileSystem, builtins);
            pipe.ExecutePipe();
        }
    }
}