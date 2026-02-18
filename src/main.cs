class Program
{
    static void Main()
    {
        var parser = new Parser();
        var fileSystem = new FileSystem();
        var builtIns = new BuiltIns(fileSystem);
        
        var item = new Bash(parser, fileSystem, builtIns);
        item.Start();
    }
}