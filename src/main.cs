class Program
{
    static void Main()
    {

        while (true)
        {
            Console.Write("$ ");
            var input = Console.ReadLine();
            Console.Write($"{input}: command not found");    
        }
    }
}
