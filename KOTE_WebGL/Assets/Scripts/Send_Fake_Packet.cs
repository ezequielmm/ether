using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Send_Fake_Packet : MonoBehaviour
{
    [Tooltip("The Json To Send.")]
    [SerializeField]
    string PacketJson;

    [SerializeField]
    bool Run;

    private void Update()
    {
        if (Run) 
        {
            Run = false;
            SWSM_Parser.ParseJSON(PacketJson);
        }
    }
}
