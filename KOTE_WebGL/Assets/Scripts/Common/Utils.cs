using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class Utils
{
    public static string ReadJsonFile(string fileName)
    {
        string path = "Assets/Resources/" + fileName;
        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(path);
        string result =reader.ReadToEnd();
        reader.Close();
        return result;
    }

    public static TEnum ParseEnum<TEnum>(string dataString) where TEnum : struct
    {
        TEnum parsedEnum;
        bool parseSuccess = Enum.TryParse(dataString, out parsedEnum);
        if (parseSuccess) return parsedEnum;
        Debug.LogError("Warning: Enum not parsed. No value '" + dataString +"' in enum type " + typeof(TEnum));
        return default(TEnum);
    }
    
    // utility function to clean up an enum when we're going to display it to the player
    public static string CapitalizeEveryWordOfEnum<T>(T inEnumValue){
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
}
