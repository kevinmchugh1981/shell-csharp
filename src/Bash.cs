public class Bash
{
    public void Start()
    {
        while (true)
        {
            Console.Write("$ ");
            var input = Console.ReadLine() ?? string.Empty;
            
            switch (string.IsNullOrWhiteSpace(input))
            {
                case false when BuiltIns.Commands.TryGetValue(input.GetFirstElement(), out var command):
                    command.Invoke(input);
                    break;
                case false when
                    input.StartsWith(Constants.CatName, StringComparison.InvariantCultureIgnoreCase) &&
                    FileSystem.IsExecutable(input.GetFirstElement(), out var catPath):
                {
                    FileSystem.Execute(catPath, Parsers.Parse(input.Replace(Constants.CatName + " ", string.Empty).Trim()));
                }
                    break;
                case false when FileSystem.IsExecutable(input.GetFirstElement(), out var path):
                    FileSystem.Execute(path, input.Split().Skip(1).ToList());
                    break;
                case false:
                    Console.WriteLine($"{input}: command not found");
                    break;
            }
        }
    }
}