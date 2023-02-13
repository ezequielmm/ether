using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FeedbackPanelManager : MonoBehaviour
{
    public TMP_InputField titleInput, detailInput;
    
    // Start is called before the first frame update
    void Start()
    {
     gameObject.SetActive(false);   
    }

    public void OnSubmitPressed()
    {
        string title = titleInput.text;
        string detail = detailInput.text;
    }

    public void OnDebugButtonPressed()
    {
        
    }
}
