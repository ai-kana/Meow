using Microsoft.Extensions.Configuration;
using UnityEngine;
using Meow.Core.Configuration;
using Meow.Core.Translations;
using System.Text;
using System.Collections;

namespace Meow.Core.Formatting;

public static class Formatter
{
    public static Color32 MainColor = new(0xF5, 0xA9, 0xB8, 0);
    public static Color32 FormatColor = new(0x5B, 0xCE, 0xFE, 0);
    public static Color32 RedColor = new(0xFF, 0x00, 0x00, 0);

    static Formatter()
    {
        ConfigurationEvents.OnConfigurationReloaded += OnConfigurationReloaded;
        SetIcon();
    }

    private static void SetIcon()
    {
        ChatIconUrl = MeowHost.Configuration.GetValue<string>("ChatIconUrl");
    }

    private static void OnConfigurationReloaded()
    {
        SetIcon();
    }

    public static string? ChatIconUrl {get; private set;}

    // Will do more prettying up later
    // Later is complete
    public static string Format(string format, params object[] args)
    {
        string[] strings = new string[args.Length];
        for (int i = 0; i < strings.Length; i++)
        {
            strings[i] = FormatColor.ColorText(args[i].ToString());
        }

        return string.Format(MainColor.ColorText(format), strings);
    }

    public static string FormatNoColor(string format, params object[] args)
    {
        return string.Format(format, args);
    }

    public static string FormatList(IEnumerable<string> strings, string seperator)
    {
        return String.Join(seperator, strings);
    }

    private static IEnumerable<TranslationPackage> GetTime(long seconds)
    {
        TimeSpan span = new(seconds * TimeSpan.TicksPerSecond);
        if (span.Days > 0)
        {
            yield return new(span.Days == 1 ? TranslationList.Day : TranslationList.Days, span.Days);
        }
        if (span.Hours > 0)
        {
            yield return new(span.Hours == 1 ? TranslationList.Hour : TranslationList.Hours, span.Hours);
        }
        if (span.Minutes > 0)
        {
            yield return new(span.Minutes == 1 ? TranslationList.Minute : TranslationList.Minutes, span.Minutes);
        }
        if (span.Seconds > 0)
        {
            yield return new(span.Seconds == 1 ? TranslationList.Second : TranslationList.Seconds, span.Seconds);
        }
    }

    private static readonly Translation[] _Arguments = new Translation[4]
    {
        TranslationList.OneArgument,
        TranslationList.TwoArguments,
        TranslationList.ThreeArguments,
        TranslationList.FourArguments,
    };

    public static TranslationPackage FormatTime(long seconds)
    {
        IEnumerable<TranslationPackage> args = GetTime(seconds);
        return new(_Arguments[args.Count() - 1], args.ToArray());
    }

    private struct FastCharEnumerator : IEnumerator<char>
    {
        public FastCharEnumerator(string str)
        {
            _String = str;
            _Index = -1;
        }

        private int _Index;
        private readonly string _String;

        public char Current => _String[_Index];
        object IEnumerator.Current => _String[_Index];

        public bool MoveNext()
        {
            _Index++;
            return _Index < _String.Length;
        }

        public void Reset()
        {
            _Index = -1;
        }

        public void Dispose()
        {
        }
    }

    public static string RemoveRichText(string text)
    {
        StringBuilder builder = new(text.Length);
        IEnumerator<char> enumerator = new FastCharEnumerator(text);
        while (enumerator.MoveNext())
        {
            if (enumerator.Current == '<')
            {
                while (enumerator.MoveNext() && enumerator.Current != '>');
                continue;
            }

            builder.Append(enumerator.Current);
        }

        return builder.ToString();
    }
}
