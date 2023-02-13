using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public static class Utils
{
    public static string ReadJsonFile(string fileName)
    {
        string path = "Assets/Resources/" + fileName;
        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(path);
        string result = reader.ReadToEnd();
        reader.Close();
        return result;
    }

    public static TEnum ParseEnum<TEnum>(string dataString) where TEnum : struct, Enum
    {
        TEnum parsedEnum;

        bool parseSuccess = Enum.TryParse(dataString, out parsedEnum);
        if (parseSuccess) return parsedEnum;
        try
        {
            parsedEnum = (TEnum)Enum.Parse(typeof(TEnum), dataString, true);
            return parsedEnum;
        }
        catch
        {
            Debug.LogError("Warning: Enum not parsed. No value '" + dataString + "' in enum type " + typeof(TEnum));
            return default(TEnum);
        }
    }

    // utility function to clean up an enum when we're going to display it to the player
    public static string CapitalizeEveryWordOfEnum<T>(T inEnumValue)
    {
        if (!inEnumValue.GetType().IsEnum)
        {
            throw new Exception("Input value is not an Enum");
        }

        string enumString = inEnumValue.ToString();
        string[] splitEnum = enumString.Split(new[] { '_' });
        splitEnum[0] = char.ToUpper(splitEnum[0][0]) + splitEnum[0].Substring(1);
        string returnString = "";
        foreach (string word in splitEnum)
        {
            string capitalizeWord = char.ToUpper(word[0]) + word.Substring(1);
            returnString += capitalizeWord + " ";
        }

        return returnString.Trim();
    }

    public static string PrettyText(string input) 
    {
        StringBuilder sb = new StringBuilder();
        var charArr = input.ToCharArray();
        for (int i = 0; i < charArr.Length; i++)
        {
            char c = charArr[i];
            if (Char.IsUpper(c))
            {
                sb.Append(" ");
            }

            if (i == 0)
            {
                c = Char.ToUpper(c);
            }

            sb.Append(c);
        }

        return sb.ToString().Trim();
    }

    public static float PixelToSceneSize(float pixelSize, int screenHeight = 1080)
    {
        float height = 2 * (Camera.main?.orthographicSize ?? 5f); // 10
        float pixToScene = height / screenHeight;
        return pixelSize * pixToScene;
    }

    public static float GetSceneSize(Size size)
    {
        switch (size)
        {
            case Size.tiny:
                return PixelToSceneSize(75, 1080);
            case Size.small:
                return PixelToSceneSize(154, 1080);
            default:
            case Size.medium:
                return PixelToSceneSize(233, 1080);
            case Size.medium_wide:
                return PixelToSceneSize(312, 1080);
            case Size.large:
                return PixelToSceneSize(470, 1080);
            case Size.giant:
                return PixelToSceneSize(707, 1080);
        }
    }

    public static int GetPixelSize(Size size, int screenHeight = 1080)
    {
        switch (size)
        {
            case Size.tiny:
                return (int)((75 / 1080f) * 1080);
            case Size.small:
                return (int)((154 / 1080f) * 1080);
            default:
            case Size.medium:
                return (int)((233 / 1080f) * 1080);
            case Size.medium_wide:
                return (int)((312 / 1080f) * 1080);
            case Size.large:
                return (int)((470 / 1080f) * 1080);
            case Size.giant:
                return (int)((707 / 1080f) * 1080);
        }
    }

    public static IEnumerator RunAfterTime(Action action, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        action.Invoke();
    }

    /// <summary>
    /// Finds a Enemy/Player's UUID. Checks Parents then Checks Children. Unknown if not found.
    /// </summary>
    /// <param name="source">The gameobject to check</param>
    /// <returns>A String UUID or "unknown"</returns>
    public static string FindEntityId(GameObject source)
    {
        return source.GetComponentInParent<EnemyManager>()?.EnemyData?.id ?? source.GetComponentInParent<PlayerManager>()?.PlayerData?.id ??
            source.GetComponentInChildren<EnemyManager>()?.EnemyData?.id ?? source.GetComponentInChildren<PlayerManager>()?.PlayerData?.id ?? "unknown";
    }
}