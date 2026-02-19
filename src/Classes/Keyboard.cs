

public class Keyboard : IKeyboard
{
    private static string ResetLine => "\r$ {0}";
    
    public string GetInput()
    {
        ConsoleKeyInfo key;
        var input = string.Empty;
        do
        {
            key = Console.ReadKey(true);
            switch (key.Key)
            {
                case ConsoleKey.Tab when Constants.EchoName.StartsWith(input, StringComparison.InvariantCultureIgnoreCase):
                    input = Constants.EchoName + " ";
                    Console.Write(ResetLine, input);
                    break;
                case ConsoleKey.Tab when Constants.ExitName.StartsWith(input, StringComparison.InvariantCultureIgnoreCase):
                    input = Constants.ExitName+ " ";
                    Console.Write(ResetLine, input);
                    break;
                default:
                    input += key.KeyChar;
                    Console.Write(key.KeyChar);
                    break;
            }
        } while (key.Key != ConsoleKey.Enter);
        return input;
    }
}