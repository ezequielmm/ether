using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerPlayCard : MonoBehaviour, IPointerRunable
{
    public void OnCancel()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke("Card Cancel");
    }

    public void OnSelect()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke("Card Play");
    }

    public void Run(string originId, string targetId)
    {
        // Card --> Enemy/Player
        GameManager.Instance.EVENT_CARD_PLAYED.Invoke(originId, targetId);
    }
}
