using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FeedbackPanelManager : MonoBehaviour
{
    public GameObject feedbackContainer;
    public GameObject SettingsPanel;
    public TMP_InputField titleInput, detailInput;
    public Image screenshot;
    public Camera screenshotCam;
    private string screenshotData;
    
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
        StartCoroutine(ScreenshotBeforeOpenPanel());
    }
    
    IEnumerator ScreenshotBeforeOpenPanel()
    {
        screenshotCam.gameObject.SetActive(true);
        if (SettingsPanel != null)
        {
            SettingsPanel.SetActive(false);
        }
        RenderTexture screenTexture = new RenderTexture(Screen.width, Screen.height, 16);
         screenshotCam.targetTexture = screenTexture;
        RenderTexture.active = screenTexture;
        yield return new WaitForEndOfFrame();
        screenshotCam.Render();
        Texture2D renderedTexture = new Texture2D(Screen.width, Screen.height);
        renderedTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        RenderTexture.active = null;
        renderedTexture.Apply();
        screenshot.sprite =  Sprite.Create(renderedTexture, new Rect(0, 0, renderedTexture.width, renderedTexture.height),
            Vector2.zero);
        byte[] byteArray = renderedTexture.EncodeToPNG();
        screenshotData = Convert.ToBase64String(byteArray);
        if (SettingsPanel != null)
        {
            SettingsPanel.SetActive(true);
        }
        feedbackContainer.SetActive(true);
        screenshotCam.gameObject.SetActive(false);

    }

    public void OnBackPressed()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        feedbackContainer.SetActive(false);
    }

    public void OnSubmitPressed()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        GameManager.Instance.EVENT_SEND_BUG_FEEDBACK.Invoke(titleInput.text, detailInput.text, screenshotData);
        feedbackContainer.SetActive(false);
        titleInput.text = "";
        detailInput.text = "";
    }
}
