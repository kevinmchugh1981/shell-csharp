class Program
{
    static void Main()
    {
        string input;

        do
        {
            Console.Write("$ ");
            input = Console.ReadLine() ?? string.Empty;
            Console.WriteLine($"{input}: command not found");
        } while (!input.Equals("exit", StringComparison.InvariantCultureIgnoreCase));
    }
}