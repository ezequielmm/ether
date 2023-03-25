using UnityEngine;

public class SettingsButtonManager : MonoBehaviour
{
    public GameObject button;

    // Start is called before the first frame update
    void Start()
    {
        button.SetActive(false);
        GameManager.Instance.EVENT_REQUEST_LOGOUT_COMPLETED.AddListener(OnLogout);
        GameManager.Instance.EVENT_AUTHENTICATED.AddListener(OnAuthenticated);
        GameManager.Instance.EVENT_SETTINGSPANEL_ACTIVATION_REQUEST.AddListener(OnSettingsActivated);
    }

    public void OnSettingsButton()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        GameManager.Instance.EVENT_SETTINGSPANEL_ACTIVATION_REQUEST.Invoke(true);
    }

    private void OnSettingsActivated(bool panelActive)
    {
        button.SetActive(!panelActive);
    }

    private void OnAuthenticated()
    {
        button.SetActive(true);
    }

    private void OnLogout(string data)
    {
        button.SetActive(false);
    }
}