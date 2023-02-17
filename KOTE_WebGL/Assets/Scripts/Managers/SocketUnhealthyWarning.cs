using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocketUnhealthyWarning : MonoBehaviour
{
    [SerializeField]
    TextBoxPopupPanel popupPanel;

    private WebSocketManager socket;
    private float lastHealthyTimeInSeconds;

    private PopupMessageState messageState = PopupMessageState.healthy;
    private bool clickDisabled = false;


    void Start()
    {
        popupPanel.Disable();
        lastHealthyTimeInSeconds = Time.time;
    }

    void Update()
    {
        CheckSocketHealth();
    }

    private void CheckSocketHealth() 
    {
        if(socket.IsSocketHealthy) 
        {
            messageState = PopupMessageState.healthy;
            lastHealthyTimeInSeconds = Time.time;
            return;
        }
        if (Time.time - lastHealthyTimeInSeconds >= GameSettings.MAX_TIMEOUT_SECONDS) 
        {
            SetMessageState(PopupMessageState.timeout);
        }
        else if (Time.time - lastHealthyTimeInSeconds >= GameSettings.DISCONNECTED_CONNECTION_SECONDS)
        {
            SetMessageState(PopupMessageState.disconnected);
        }
        else if (Time.time - lastHealthyTimeInSeconds >= GameSettings.UNSTABLE_CONNECTION_SECONDS)
        {
            SetMessageState(PopupMessageState.unstable);
        }
    }

    private void SetMessageState(PopupMessageState state) 
    {
        if (state == messageState)
        {
            return;
        }
        else 
        {
            messageState = state;
        }
        switch(state) 
        {
            case PopupMessageState.healthy:
                popupPanel.Disable();
                if(clickDisabled) 
                {
                    UIDisableGameWhenOpen.EnableClick();
                }
                break;
            case PopupMessageState.unstable:
                popupPanel.Popup("The Connection is Unstable...");
                break;
            case PopupMessageState.disconnected:
                popupPanel.Popup("Disconnected. Trying to reconnect...");
                if (!clickDisabled) 
                {
                    UIDisableGameWhenOpen.DisableClick();
                }
                clickDisabled = true;
                break;
            case PopupMessageState.timeout:
                popupPanel.Disable();
                string pannelMessage = "Could not reconnect to server. Please try again later.";
                string[] buttons = { "Return to Main Menu", string.Empty };
                GameManager.Instance.EVENT_SHOW_CONFIRMATION_PANEL_WITH_FULL_CONTROL.Invoke(pannelMessage, () =>
                {
                    GameManager.Instance.LoadScene(inGameScenes.MainMenu);
                }, null, buttons);
                break;
        }
    }

    private void OnDestroy()
    {
        if (clickDisabled)
        {
            UIDisableGameWhenOpen.EnableClick();
        }
    }

    private enum PopupMessageState 
    {
        healthy = 0,
        unstable,
        disconnected,
        timeout
    }
}
