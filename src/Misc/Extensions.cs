

public static class Extensions
{
    public static string GetFirstElement(this string input)
    {
        if(string.IsNullOrWhiteSpace(input) || !input.Contains(' '))
            return input;
        return input.Split(" ")[0];
    }

    public static char NextElement(this string input, int index)
    {
        if (string.IsNullOrWhiteSpace(input) || index + 1 > input.Length - 1)
            return char.MinValue;
        return input[index + 1];
    }

    public static char PreviousElement(this string input, int index)
    {
        if(string.IsNullOrWhiteSpace(input) || index-1 < 0)
            return char.MinValue;
        else
            return   input[index - 1];
    }

    public static bool LastElement(this string input, int index)
    {
        return index == input.Length - 1;
    }

    public static bool EmptyQuotes(this string input, int index, char delimiter)
    {
        //If previous is not empty and matches current, then it's empty.
        if (index < input.Length-1 && input.PreviousElement(index).Equals(delimiter))
            return true;
        //If next is not empty and matches current, then it's empty, otherwise return false;
        return index < input.Length-1 && input.NextElement(index).Equals(delimiter);

    }
}