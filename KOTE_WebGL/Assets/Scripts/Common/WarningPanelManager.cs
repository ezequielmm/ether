using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WarningPanelManager : MonoBehaviour
{ [Tooltip("The container for the warning panel")]
    public GameObject warningContainer;
    [Tooltip("The title text for the warning panel")]
    public TMP_Text warningText;
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.EVENT_SHOW_WARNING_MESSAGE.AddListener(ShowWarningMessage);
        GameManager.Instance.EVENT_HIDE_WARNING_MESSAGE.AddListener(HideWarningMessage);
    }

 
    public void ShowWarningMessage(string displayText)
    {
        warningText.text = displayText;
        warningContainer.SetActive(true);
    }

    private void HideWarningMessage()
    {
        warningContainer.SetActive(false);
    }
}
