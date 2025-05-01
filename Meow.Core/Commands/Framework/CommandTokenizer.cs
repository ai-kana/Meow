using System.Text;
using Meow.Core.Enumerable;

namespace Meow.Core.Commands.Framework;

internal static class CommandTokenizer
{
    private static void ParseQuote(IEnumerator<char> enumerator, StringBuilder builder)
    {
        while (enumerator.MoveNext())
        {
            char c = enumerator.Current;
            if (c == '"')
            {
                return;
            }

            builder.Append(c);
        }
    }

    private static IEnumerable<string> Tokenize(string text)
    {
        StringBuilder builder = new(32);
        IEnumerator<char> enumerator = new FastCharEnumerator(text.TrimStart().TrimStart('/'));

        while (enumerator.MoveNext())
        {
            char c = enumerator.Current;
            switch (c)
            {
                case '"':
                    yield return BuildString();
                    ParseQuote(enumerator, builder);
                    yield return BuildString();
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

    private static IEnumerable<string> Sanitize(IEnumerable<string> tokens)
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

    public static IEnumerable<string> Parse(string input)
    {
        return Sanitize(Tokenize(input));
    }
}
