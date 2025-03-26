namespace DotLox.Extensions;

public static class DictionaryExtension
{
    public static void Put<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if (!dictionary.TryAdd(key, value))
        {
            dictionary[key] = value;
        } 
    }
}