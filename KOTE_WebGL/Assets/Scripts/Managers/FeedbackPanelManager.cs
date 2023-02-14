using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FeedbackPanelManager : MonoBehaviour
{
    public GameObject feedbackContainer;
    public TMP_InputField titleInput, detailInput;
    
    // Start is called before the first frame update
    void Start()
    {
     feedbackContainer.SetActive(false);   
     GameManager.Instance.EVENT_SHOW_FEEDBACK_PANEL.AddListener(OnShowFeedbackPanel);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F8))
        {
            OnShowFeedbackPanel();
        }
    }

    private void OnShowFeedbackPanel()
    {
        feedbackContainer.SetActive(true);
        // TODO add screenshot capabilities
    }

    public void OnBackPressed()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        feedbackContainer.SetActive(false);
    }

    public void OnSubmitPressed()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        GameManager.Instance.EVENT_SEND_BUG_FEEDBACK.Invoke(titleInput.text, detailInput.text);
        feedbackContainer.SetActive(false);
        titleInput.text = "";
        detailInput.text = "";
    }
}
