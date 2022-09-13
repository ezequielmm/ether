using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerPlayPotion : MonoBehaviour, IPointerRunable
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
        // Potion --> Enemy/Player
        Debug.Log($"[PointerPlayPotion] Play potion! target => {targetId} | origin => {originId}");
    }
}
