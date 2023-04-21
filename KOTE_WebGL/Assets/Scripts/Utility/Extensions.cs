using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class Extensions
{
    public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> sequence, int size)
    {
        List<T> partition = new List<T>(size);
        foreach (var item in sequence)
        {
            partition.Add(item);
            if (partition.Count == size)
            {
                yield return partition;
                partition = new List<T>(size);
            }
        }
        if (partition.Count > 0)
            yield return partition;
    }

    public static Sprite ToSprite(this Texture2D texture) 
    {
        if(texture == null) return null;
        Rect imageSize = new Rect(0, 0, texture.width, texture.height);
        return Sprite.Create(texture, imageSize, Vector2.zero);
    }

    public static void AddAuthToken(this UnityWebRequest request)
    {
        if(request == null) return;
        string token = AuthenticationManager.Instance.GetSessionToken() ?? string.Empty;
        request.SetRequestHeader("Authorization", $"Bearer {token}");
    }

    public static TEnum ParseToEnum<TEnum>(this string dataString) where TEnum : struct, Enum
    {
        TEnum parsedEnum;

        dataString = dataString.Replace(" ", "");

        bool parseSuccess = Enum.TryParse(dataString, out parsedEnum);
        if (parseSuccess) return parsedEnum;
        try
        {
            parsedEnum = (TEnum)Enum.Parse(typeof(TEnum), dataString, true);
            return parsedEnum;
        }
        catch
        {
            Debug.LogWarning("Warning: Enum not parsed. No value '" + dataString + "' in enum type " + typeof(TEnum) +
                             "Falling back to default value");
            return default(TEnum);
        }
    }
    
    public static string AddPath(this string url, params string[] paths)
    {
        if (paths == null || paths.Length == 0) return url;
        StringBuilder sb = new StringBuilder();
        sb.Append(url.Trim('/').Trim('\\'));
        foreach (var path in paths)
        {
            sb.Append("/");
            sb.Append(path.Trim('/').Trim('\\'));
        }
        return sb.ToString();
    }
}
