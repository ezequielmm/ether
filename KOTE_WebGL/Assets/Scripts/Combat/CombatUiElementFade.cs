using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatUiElementFade : MonoBehaviour
{
    public Image uiImage;
    public TMP_Text uiText;
    
    private float fadeoutSecondsRemaining;
    private bool fadeImage;
    private bool fadeText;

    private bool fadeOut;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.EVENT_FADE_OUT_UI.AddListener(OnFadeOutUi);
        fadeImage = uiImage != null;
        fadeText = uiText != null;
    }
    
    private void OnFadeOutUi()
    {
        if(gameObject.activeInHierarchy) fadeoutSecondsRemaining = GameSettings.UI_FADEOUT_TIME;
    }

    // Update is called once per frame
    void Update()
    {
        if (fadeoutSecondsRemaining > 0)
        {
            fadeoutSecondsRemaining -= Time.deltaTime;
            if (fadeoutSecondsRemaining <= 0) fadeoutSecondsRemaining = 0;
            
            // control these separately so we can use a script instance for a single module
            if (fadeImage) FadeOutImage();
            if (fadeText) FadeOutText();
        }
    }

    private void FadeOutImage()
    {
        Color color = uiImage.color;
        color.a = fadeoutSecondsRemaining / GameSettings.UI_FADEOUT_TIME;
        uiImage.color = color;
    }

    private void FadeOutText()
    {
        Color color = uiText.color;
        color.a = fadeoutSecondsRemaining / GameSettings.UI_FADEOUT_TIME;
        uiText.color = color;
    }

    
}