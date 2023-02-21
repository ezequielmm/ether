using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Send_Fake_Packet : MonoBehaviour
{
    [Tooltip("The Json To Send.")]
    [SerializeField]
    List<string> PacketJson;

    [SerializeField]
    float delayInSeconds = 0;

    [SerializeField]
    bool Run;

    private void Update()
    {
        if (Run) 
        {
            Run = false;
            StartCoroutine(RunPacket());
        }
    }

    IEnumerator RunPacket() 
    {
        var wait = new WaitForSeconds(delayInSeconds);
        foreach (var packet in PacketJson) 
        {
            if (delayInSeconds == 0)
            {
                yield return null;
            }
            else 
            {
                yield return wait;
            }
            WebSocketParser.ParseJSON(packet);
        }
        yield return null;
    }
}
