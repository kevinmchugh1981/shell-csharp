class Program
{
    static void Main()
    {
        var fileSystem = new FileSystem();
        var builtIns = new BuiltIns(fileSystem);
        var keyboard = new Keyboard(fileSystem);
        var parser = new InstructionParser();
        
        var item = new Bash(parser, fileSystem, builtIns, keyboard);
        item.Start();
    }
}