internal class Parser : IParser
{
    private static char SingleQuote => '\'';
    private static char DoubleQuote => '"';
    private static char EscapeChar => '\\';

    private static string AppendOutputToFile => ">>";
    private static string AppendOutputToFileAlt => "1>>";
    private static string RedirectOutputToFile => ">";
    private static string RedirectOutputToFileAlt => "1>";
    private static string RedirectErrorToFile => "2>";
    private static string AppendErrorToFile => "2>>";
    private static List<string> RedirectOperators => [RedirectOutputToFile, RedirectOutputToFileAlt,  RedirectErrorToFile, AppendOutputToFile, AppendOutputToFileAlt, AppendErrorToFile];
    private static List<char> Quotes => [SingleQuote, DoubleQuote];

    public Instruction ParseAlt(string input)
    {
        
        if (string.IsNullOrEmpty(input))
            return new Instruction();
        
        var delimiter = Char.MinValue;
        var elements = new List<string>();
        var currentLine = string.Empty;
        var escapeNextChar = false;
        for (var x = 0; x < input.Length; ++x)
        {

            //If you find an escape char, that's not already being escaped, and you aren't inside single quotes, escape it.
            if (!input.PreviousElement(x).Equals(EscapeChar) && input[x].Equals(EscapeChar) && !delimiter.Equals(SingleQuote))
            {
                escapeNextChar = true;
                continue;
            }
            
            //Open quotes.
            if (!escapeNextChar && Quotes.Contains(input[x]) &&
                delimiter == Char.MinValue)
            {
                //If the line isn't blank, and you aren't just opening a new quote in the next char.
                if (!string.IsNullOrWhiteSpace(currentLine) &&
                    (char.IsWhiteSpace(input.NextElement(x)) || input.NextElement(x).Equals(char.MinValue)))
                {
                    elements.Add(currentLine);
                    currentLine = string.Empty;
                }

                delimiter = input[x];
                continue;
            }

            //Close quotes, if you aren't being told to escape the char, or ignore the escape char if you are single quotes.
            if (input[x] == delimiter && !escapeNextChar ||
                (input[x] == delimiter && escapeNextChar && delimiter.Equals(SingleQuote)))
            {
                //If the line isn't blank, and you aren't just opening a new quote in the next char.
                if (input.LastElement(x) ||
                    (char.IsWhiteSpace(input.NextElement(x)) || input.NextElement(x).Equals(char.MinValue)) &&
                    !string.IsNullOrEmpty(currentLine))
                {
                    elements.Add(currentLine);
                    currentLine = string.Empty;
                }

                delimiter = char.MinValue;
                continue;
            }
            
            //Add chars inside quotes.
            if (delimiter != char.MinValue)
            {
                //Ignore escape characters
                if (!delimiter.Equals(SingleQuote) && !input.PreviousElement(x).Equals(EscapeChar) && input[x].Equals(EscapeChar))
                    continue;
                
                //Add character.
                currentLine += input[x];
            }
            //Outside quotes, the result isn't blank, and it's already ending in a blank space, add a char.
            else if (!string.IsNullOrWhiteSpace(currentLine) && char.IsWhiteSpace(currentLine.Last()))
            {
                if (escapeNextChar || !char.IsWhiteSpace(input[x]))
                    currentLine += input[x];
            }
            //Outside of quotes.
            else
            {
                //If the char is not a white space, or you are escaping it then add it.
                if (escapeNextChar || !char.IsWhiteSpace(input[x]))
                    currentLine += input[x];
                //If the char is a whitespace and there is a value in the current line then add it.
                else if (char.IsWhiteSpace(input[x]) && !string.IsNullOrWhiteSpace(currentLine))
                {
                    elements.Add(currentLine);
                    currentLine = string.Empty;
                }
            }

            //If you have run out of letters, add it so it's not lost.
            if (input.LastElement(x) && !string.IsNullOrEmpty(currentLine))
                elements.Add(currentLine);
            
            //unset escape next char.
            escapeNextChar = false;
        }

        return elements.Count switch
        {
            0 => new Instruction(),
            1 => new Instruction { CommandName = elements[0] },
            _ => Convert(elements)
        };
    }

    private static Instruction Convert(List<string> elements)
    {
        //Create new parsed element.
        var result = new Instruction
        {
            Arguments = [],
            CommandName = elements[0],
            RedirectDestination = string.Empty
        };

        //If it contains any redirect operators apply them.
        if (elements.Any(x => RedirectOperators.Contains(x, StringComparer.InvariantCultureIgnoreCase)))
        {
            //Get redirection command and then output destination.
            var redirectToNextArg = false;
            foreach (var arg in elements.Skip(1))
            {
                //If you find the redirect operator, move onto the next string.
                if (RedirectOperators.Contains(arg, StringComparer.InvariantCultureIgnoreCase))
                {
                    if(arg.Equals(AppendOutputToFile, StringComparison.InvariantCultureIgnoreCase) 
                       || arg.Equals(AppendOutputToFileAlt, StringComparison.InvariantCultureIgnoreCase))
                        result.Redirect = Redirect.AppendOutput;
                    else if(arg.Equals(RedirectErrorToFile, StringComparison.InvariantCultureIgnoreCase))
                        result.Redirect = Redirect.Error;
                    else if(arg.Equals(AppendErrorToFile, StringComparison.InvariantCultureIgnoreCase))
                        result.Redirect = Redirect.AppendError;
                    redirectToNextArg = true;
                    continue;
                }

                //If you are storing the redirect then, store it and carry on.
                if (redirectToNextArg)
                {
                    result.RedirectDestination = arg;
                    break;
                }
                //otherwise just add the argument and move on.
                result.Arguments.Add(arg);
            }
        }
        //Otherwise just carry on as normal.
        else
        {
            result.RedirectDestination = string.Empty;
            result.Arguments = elements.Skip(1).ToList();
        }
            
        return result;
    }
}