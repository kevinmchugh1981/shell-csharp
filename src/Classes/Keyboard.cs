

public class Keyboard : IKeyboard
{
    private static string ResetLine => "\r ${0}";
    
    public string GetInput()
    {
        ConsoleKeyInfo key;
        var input = string.Empty;
        do
        {
            key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Tab && Constants.EchoName.StartsWith(input, StringComparison.InvariantCultureIgnoreCase))
            {
                input = Constants.EchoName+" ";
                Console.Write(ResetLine, input);
            }
            if (key.Key == ConsoleKey.Tab && Constants.ExitName.StartsWith(input, StringComparison.InvariantCultureIgnoreCase))
            {
                input = Constants.ExitName;
                Console.Write(ResetLine, input);
            }
            else
            {
                input += key.KeyChar;
                Console.Write(key.KeyChar);
            }
               
            
            
        } while (key.Key != ConsoleKey.Enter);
        return input;
    }
}