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
            if(!string.IsNullOrWhiteSpace(input) && input.StartsWith("echo", StringComparison.InvariantCultureIgnoreCase))
                Console.WriteLine(input.Replace("echo ", string.Empty));
            Console.WriteLine($"{input}: command not found");    
        }
    }
}
