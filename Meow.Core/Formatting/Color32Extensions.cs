using UnityEngine;

namespace Meow.Core.Formatting;

public static class Color32Extensions
{
    public static string ToHex(this Color32 color)
    {
        return BitConverter.ToString(new byte[] {color.r, color.g, color.b}).Replace("-", "");
    }

    public static string ColorText(this Color32 color, string text)
    {
        return $"<color=#{color.ToHex()}>{text}</color>";
    }
}
