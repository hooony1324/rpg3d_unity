using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextReplacer
{
    public static string Replace(string text, IReadOnlyDictionary<string, string> textsByKeyword)
    {
        if (textsByKeyword != null)
        {
            foreach (var pair in textsByKeyword)
                text = text.Replace($"$[{pair.Key}]", pair.Value);
        }

        return text;
    }

    public static string Replace(string text, string prefixKeyword, IReadOnlyDictionary<string, string> textsByKeyword)
    {
        if (textsByKeyword != null)
        {
            foreach (var pair in textsByKeyword)
                text = text.Replace($"$[{prefixKeyword}.{pair.Key}]", pair.Value);
        }
        return text;
    }

    public static string Replace(string text, IReadOnlyDictionary<string, string> textsByKeyword, string suffixKeyword)
    {
        if (textsByKeyword != null)
        {
            foreach (var pair in textsByKeyword)
                text = text.Replace($"$[{pair.Key}.{suffixKeyword}]", pair.Value);
        }
        return text;
    }

    public static string Replace(string text, string prefixKeyword, IReadOnlyDictionary<string, string> textsByKeyword, string suffixKeyword)
    {
        if (textsByKeyword != null)
        {
            foreach (var pair in textsByKeyword)
                text = text.Replace($"$[{prefixKeyword}.{pair.Key}.{suffixKeyword}]", pair.Value);
        }
        return text;
    }
}
