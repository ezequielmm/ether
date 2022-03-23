using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WalletItem : MonoBehaviour
{
    public List<Image> columns;

    public void SetColor(Color color)
    {
        foreach (Image column in columns)
        {
            column.color = color;
        }
    }

    public void ActivateConfirmation()
    {
        GameManager.Instance.EVENT_DISCONNECTWALLETPANEL_ACTIVATION_REQUEST.Invoke(true, gameObject);
    }
}