using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Emit_Fake_Packet : MonoBehaviour
{
    public string toEmit;
    public string toEmit2;
    public bool send = false;

    // Update is called once per frame
    void Update()
    {
        if (send == true) 
        {
            send = false;
#if UNITY_EDITOR
            WebSocketManager.Instance.ForceEmit(toEmit, toEmit2);
#endif
        }
    }
}
