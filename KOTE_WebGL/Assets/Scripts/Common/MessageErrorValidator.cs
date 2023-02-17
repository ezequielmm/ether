using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MessageErrorValidator
{
    // Start is called before the first frame update


    
    public static bool ValidateData(string jsonString)
    {
        Errordata error = JsonConvert.DeserializeObject<Errordata>(jsonString);

        if ( error.data.message != null ) 
        {
            Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! Websocket message Error: "+ error.data.message);
            return false;
        }
        return true;
    }
}
