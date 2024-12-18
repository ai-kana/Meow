using System.Text;

namespace Meow.Core.Commands.Framework;

internal class CommandTokenizer
{
    private readonly string Text;

    public CommandTokenizer(string text)
    {
        Text = text;
    }

    private string ParseQuote(IEnumerator<char> enumerator, StringBuilder builder)
    {
        while (enumerator.MoveNext())
        {
            char c = enumerator.Current;
            if (c == '"')
            {
                return builder.ToString();
            }

            builder.Append(c);
        }

        string ret = builder.ToString();
        builder.Clear();
        return ret;
    }

    private IEnumerable<string> Tokenize()
    {
        StringBuilder builder = new(32);
        IEnumerator<char> enumerator = Text.TrimStart().TrimStart('/').GetEnumerator();

        while (enumerator.MoveNext())
        {
            char c = enumerator.Current;
            switch (c)
            {
                case '"':
                    yield return BuildString();
                    yield return ParseQuote(enumerator, builder);
                    continue;
                case ' ':
                    yield return BuildString();
                    continue;
            }

            builder.Append(c);
        }

        yield return BuildString();
        yield break;

        string BuildString()
        {
            string ret = builder.ToString();
            builder.Clear();
            return ret;
        }
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
