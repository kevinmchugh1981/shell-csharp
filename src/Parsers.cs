internal static class Parsers
{
    private static char SingleQuote => '\'';
    private static char DoubleQuote => '"';
    private static char EscapeChar => '\\';
    
    private static List<char> Quotes => [SingleQuote, DoubleQuote];
    
    
    internal static List<string> Parse(string input)
    {
        var delimiter = Char.MinValue;
        var result = new List<string>();
        var currentLine = string.Empty;
        for (var x = 0; x < input.Length; ++x)
        {
            //Open quotes.
            if (!input.PreviousElement(x).Equals(EscapeChar) && Quotes.Contains(input[x]) &&
                delimiter == Char.MinValue)
            {
                //If the line isn't blank, and you aren't just opening a new quote in the next char.
                if (!string.IsNullOrWhiteSpace(currentLine) &&  
                    (char.IsWhiteSpace(input.NextElement(x)) || input.NextElement(x).Equals(char.MinValue)))
                {
                    result.Add(currentLine);
                    currentLine = string.Empty;
                }

                delimiter = input[x];
                continue;
            }

            //Close quotes, if you aren't being told to escape the char, or ignore the escape char if you are single quotes.
            if ((input[x] == delimiter && !input.PreviousElement(x).Equals(EscapeChar)) ||
                (input[x] == delimiter && input.PreviousElement(x).Equals(EscapeChar) && delimiter.Equals(SingleQuote)))
            {
                //If the line isn't blank, and you aren't just opening a new quote in the next char.
                if (input.LastElement(x) || (char.IsWhiteSpace(input.NextElement(x)) || input.NextElement(x).Equals(char.MinValue)) &&
                    !string.IsNullOrEmpty(currentLine))
                {
                    result.Add(currentLine);
                    currentLine = string.Empty;
                }
                
                delimiter = char.MinValue;
                continue;
            }
            
            //If current char is escape indicator, and you are outside quotes.
            if (input[x].Equals(EscapeChar) && delimiter.Equals(char.MinValue) && !input.PreviousElement(x).Equals(EscapeChar))
            {
                continue;
            }

            //Add any char as you as inside quotes.
            if (delimiter != char.MinValue)
                currentLine += input[x];
            //Outside quotes, the result isn't blank, and it's already ending in a blank space, add a char.
            else if (!string.IsNullOrWhiteSpace(currentLine) && char.IsWhiteSpace(currentLine.Last()))
            {
                if (input.PreviousElement(x).Equals(EscapeChar) || !char.IsWhiteSpace(input[x]))
                    currentLine += input[x];
            }
            //Outside of quotes.
            else
            {
                //If the char is not a white space, or you are escaping it then add it.
                if(input.PreviousElement(x).Equals(EscapeChar) || !char.IsWhiteSpace(input[x]))
                    currentLine += input[x];
                //If the char is a whitespace and there is a value in the current line then add it.
                else if (char.IsWhiteSpace(input[x]) && !string.IsNullOrWhiteSpace(currentLine))
                {
                    result.Add(currentLine);
                    currentLine = string.Empty;
                }
            }

            //If you have run out of letters, add it so it's not lost.
            if (input.LastElement(x) && !string.IsNullOrEmpty(currentLine))
                result.Add(currentLine);
        }

        return result;
    }
}