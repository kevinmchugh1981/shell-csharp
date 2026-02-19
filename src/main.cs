class Program
{
    static void Main()
    {
        var parser = new InstructionIParser();
        var fileSystem = new FileSystem();
        var builtIns = new BuiltIns(fileSystem);
        
        var item = new Bash(parser, fileSystem, builtIns);
        item.Start();
    }
}