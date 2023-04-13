using UnityEngine;

public class CanvasGroupFade : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    private float fadeoutSecondsRemaining;

    private bool fadeOut;

    public void FadeOutUi()
    {
        if (gameObject.activeInHierarchy) fadeoutSecondsRemaining = GameSettings.UI_FADEOUT_TIME;
    }

    // Update is called once per frame
    void Update()
    {
        if (fadeoutSecondsRemaining > 0)
        {
            fadeoutSecondsRemaining -= Time.deltaTime;
            if (fadeoutSecondsRemaining <= 0) fadeoutSecondsRemaining = 0;
            FadeOutGroup();
        }
    }

    private void FadeOutGroup()
    {
        canvasGroup.alpha = fadeoutSecondsRemaining / GameSettings.UI_FADEOUT_TIME;
    }
}