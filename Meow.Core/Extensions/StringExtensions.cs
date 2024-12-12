namespace Meow.Core.Extensions;

public static class StringExtensions
{
    public static bool Contains(this string instance, string item, StringComparison comparison)
    {
        return instance.IndexOf(item, comparison) >= 0;
    }
}
