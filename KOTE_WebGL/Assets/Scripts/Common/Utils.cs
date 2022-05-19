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
}
