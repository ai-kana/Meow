namespace Meow.Core.Extensions;

public static class DictionaryExtensions
{
    public static bool TryAdd<T1, T2>(this Dictionary<T1, T2> instance, T1 key, T2 value)
    {
        if (instance.ContainsKey(key))
        {
            return false;
        }

        instance.Add(key, value);
        return true;
    }

    public static void AddOrUpdate<T1, T2>(this Dictionary<T1, T2> instance, T1 key, T2 value)
    {
        if (instance.ContainsKey(key))
        {
            instance[key] = value;
            return;
        }

        instance.Add(key, value);
    }
}
