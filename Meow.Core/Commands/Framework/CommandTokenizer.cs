namespace Meow.Core.Commands.Framework;

internal class CommandTokenizer
{
    private readonly string Text;

    public CommandTokenizer(string text)
    {
        Text = text;
    }

    private string ParseQuote(CharEnumerator enumerator)
    {
        string buffer = string.Empty;
        while (enumerator.MoveNext())
        {
            char c = enumerator.Current;
            if (c == '"')
            {
                return buffer;
            }

            buffer += c;
        }

        return buffer;
    }

    private IEnumerable<string> Tokenize()
    {
        string text = Text.TrimStart().TrimStart('/');

        string buffer = string.Empty;

        CharEnumerator enumerator = text.GetEnumerator();
        while (enumerator.MoveNext())
        {
            char c = enumerator.Current;
            if (c == '"')
            {
                yield return buffer;
                buffer = string.Empty;
                yield return ParseQuote(enumerator);
                continue;
            }

            if (c == ' ')
            {
                yield return buffer;
                buffer = string.Empty;
                continue;
            }

            buffer += c;
        }

        yield return buffer;
    }

    private IEnumerable<string> Sanitize(IEnumerable<string> tokens)
    {
        foreach (string token in tokens)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                continue;
            }

            yield return token.TrimStart().TrimEnd();
        }
    }

    public IEnumerable<string> Parse()
    {
        return Sanitize(Tokenize());
    }
}
