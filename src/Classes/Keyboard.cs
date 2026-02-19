public class Keyboard(IFileSystem fileSystem) : IKeyboard
{
    private IFileSystem fileSystem = fileSystem;
    private static string ResetLine => "\r$ {0}";

    public string GetInput()
    {
        ConsoleKeyInfo key;
        var input = string.Empty;
        ConsoleKeyInfo previousKey = default;
        do
        {
            key = Console.ReadKey(true);
            switch (key.Key)
            {
                case ConsoleKey.Tab:
                    if (Constants.EchoName.StartsWith(input, StringComparison.InvariantCultureIgnoreCase))
                    {
                        input = Constants.EchoName + " ";
                        Console.Write(ResetLine, input);
                        break;
                    }
                    if (Constants.ExitName.StartsWith(input, StringComparison.InvariantCultureIgnoreCase))
                    {
                        input = Constants.ExitName + " ";
                        Console.Write(ResetLine, input);
                        break;
                    }
                    if (fileSystem.AutoComplete(input, out List<string> executableNames))
                    {
                        if (executableNames.Count == 1)
                        {
                            input = executableNames[0]+" ";
                            Console.Write(ResetLine, input);
                            break;
                        }
                        if (previousKey.Key == ConsoleKey.Tab && executableNames.Count > 1)
                        {
                            Console.WriteLine();
                            Console.WriteLine(string.Join("  ", executableNames.OrderBy(x => x)));
                            if (fileSystem.GetLongestCommonPrefix(executableNames, out var result))
                                input = result;
                            Console.Write(ResetLine, input);
                            break;
                        }
                        Console.Write('\a');
                    }
                    Console.Write('\a');
                    break;
                default:
                    input += key.KeyChar;
                    Console.Write(key.KeyChar);
                    break;
            }

            previousKey = key;
        } while (key.Key != ConsoleKey.Enter);

        return input;
    }
}