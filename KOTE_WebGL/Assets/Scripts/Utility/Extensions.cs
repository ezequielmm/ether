using System.Collections.Generic;
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
        string token = UserDataManager.Instance.GetSessionToken() ?? string.Empty;
        request.SetRequestHeader("Authorization", $"Bearer {token}");
    }
}
