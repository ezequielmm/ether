using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerPlayCard : MonoBehaviour, IPointerRunable
{
    PointerManager manager;
    private void Start()
    {
        manager = GetComponent<PointerManager>();
        GameManager.Instance.EVENT_CARD_ACTIVATE_POINTER.AddListener(manager.OnPointerActivated);
        GameManager.Instance.EVENT_CARD_DEACTIVATE_POINTER.AddListener(manager.OnPointerDeactivated);
    }
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
