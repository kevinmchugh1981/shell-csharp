class Program
{
    static void Main()
    {

        while (true)
        {
            Console.Write("$ ");
            var input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input) && input.Equals("exit", StringComparison.InvariantCultureIgnoreCase))
                return;
            Console.WriteLine($"{input}: command not found");    
        }
    }
}
