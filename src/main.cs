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
            if (!string.IsNullOrWhiteSpace(input) &&
                input.StartsWith("echo", StringComparison.InvariantCultureIgnoreCase))
                Console.WriteLine(input.Replace("echo ", string.Empty));
            else if (!string.IsNullOrWhiteSpace(input) &&
                     input.StartsWith("type", StringComparison.InvariantCultureIgnoreCase))
            {
                var target = input.Replace("type ", string.Empty);
                if (!string.IsNullOrWhiteSpace(target) && (target.Equals("echo", StringComparison.InvariantCultureIgnoreCase) ||
                                                           target.Equals("type", StringComparison.InvariantCultureIgnoreCase) ||
                                                           target.Equals("exit", StringComparison.InvariantCultureIgnoreCase)))
                    Console.WriteLine($"{target} is a shell builtin");
                else
                    Console.WriteLine($"{target}: not found");
            }
            else 
                Console.WriteLine($"{input}: command not found");
        }
    }
}
