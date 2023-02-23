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
        socket = WebSocketManager.Instance;
        popupPanel.Disable();
        lastHealthyTimeInSeconds = Time.time;
    }

    void Update()
    {
        CheckSocketHealth();
    }

    private void CheckSocketHealth() 
    {
        if (!socket.SocketOpened) 
        {
            lastHealthyTimeInSeconds = Time.time;
            return;
        }

        if (socket.IsSocketHealthy)
        {
            SetSocketHealthy();
        }
        else
        {
            SocketUnhealthy();
        }
    }

    private void SetSocketHealthy() 
    {
        SetMessageState(PopupMessageState.healthy);
        lastHealthyTimeInSeconds = Time.time;
    }

    private void SocketUnhealthy() 
    {
        if (UnhealthyFor(GameSettings.MAX_TIMEOUT_SECONDS))
        {
            SetMessageState(PopupMessageState.timeout);
        }
        else if (UnhealthyFor(GameSettings.DISCONNECTED_CONNECTION_SECONDS))
        {
            SetMessageState(PopupMessageState.disconnected);
        }
        else if (UnhealthyFor(GameSettings.UNSTABLE_CONNECTION_SECONDS))
        {
            SetMessageState(PopupMessageState.unstable);
        }
    }

    private bool UnhealthyFor(float seconds) 
    {
        return Time.time - lastHealthyTimeInSeconds >= seconds;
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
                OnSocketHealthy();
                break;
            case PopupMessageState.unstable:
                OnSocketUnstable();
                break;
            case PopupMessageState.disconnected:
                OnSocketDisconnected();
                break;
            case PopupMessageState.timeout:
                OnSocketTimeout();
                break;
        }
    }

    private void OnSocketHealthy() 
    {
        popupPanel.Disable();
        if (clickDisabled)
        {
            UIDisableGameWhenOpen.EnableClick();
        }
    }

    private void OnSocketUnstable() 
    {
        popupPanel.Popup("The Connection is Unstable...");
    }

    private void OnSocketDisconnected()
    {
        popupPanel.Popup("Disconnected. Trying to reconnect...");
        if (!clickDisabled)
        {
            UIDisableGameWhenOpen.DisableClick();
        }
        clickDisabled = true;
    }

    private void OnSocketTimeout() 
    {
        popupPanel.Disable();
        string pannelMessage = "Could not reconnect to server. Please try again later.";
        string[] buttons = { "Return to Main Menu", string.Empty };
        GameManager.Instance.EVENT_SHOW_CONFIRMATION_PANEL_WITH_FULL_CONTROL.Invoke(pannelMessage, () =>
        {
            GameManager.Instance.LoadScene(inGameScenes.MainMenu);
        }, null, buttons);
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
