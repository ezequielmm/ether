using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UserDataManager : SingleTon<UserDataManager>
{
    [HideInInspector]
    // Active User account email
    public string UserAccount = "";
    [HideInInspector]
    // Active Expedition Id
    public string ExpeditionId = "";
    [HideInInspector] 
    public string activeNft = "";
    [HideInInspector]
    public ClientEnvironmentType ClientEnvironment = ClientEnvironmentType.Unknown;

    private void Awake()
    {
        base.Awake();
        DetermineClientEnvironment();
    }
    
    private void Start()
    {
        GameManager.Instance.EVENT_PLAYER_STATUS_UPDATE.AddListener(OnExpeditionUpdate);
        GameManager.Instance.EVENT_REQUEST_PROFILE_SUCCESSFUL.AddListener(OnPlayerProfileReceived);
        GameManager.Instance.EVENT_REQUEST_LOGOUT_SUCCESSFUL.AddListener(OnLogout);
        GameManager.Instance.EVENT_EXPEDITION_STATUS_UPDATE.AddListener(OnExpeditionStatus);
    }

    // get the unique identifier for this instance of the client
    public string ClientId
    {
        get
        {
            string clientId = PlayerPrefs.GetString("client_id");
            // if the client id doesn't exist, create one and save it
            if (string.IsNullOrEmpty(clientId))
            {
                Guid newId = Guid.NewGuid();
                clientId = newId.ToString();
                PlayerPrefs.SetString("client_id", newId.ToString());
            }

            return clientId;
        }
    }

    private void OnExpeditionStatus(bool hasExpedtion, int nftId)
    {
        activeNft = nftId.ToString();
    }
    
    private void OnPlayerProfileReceived(ProfileData profile)
    {
        UserAccount = profile.data.email;
    }

    private void OnLogout(string data)
    {
        UserAccount = "";
    }

    private void OnExpeditionUpdate(PlayerStateData playerState)
    {
        ExpeditionId = playerState.data.expeditionId;
    }

    private void DetermineClientEnvironment()
    {
        // determine the correct server the client is running on
        string hostName = Application.absoluteURL;
        Debug.Log("hostName:" + hostName);
        
        if (hostName.IndexOf("alpha") > -1 && hostName.IndexOf("knight") > -1)
        {
            ClientEnvironment = ClientEnvironmentType.Alpha;
        }
        
        if (hostName.IndexOf("alpha") > -1 && hostName.IndexOf("robot") > -1)
        {
          
           ClientEnvironment = ClientEnvironmentType.InternalAlpha;
        }

        if (hostName.IndexOf("stage") > -1)
        {
           ClientEnvironment = ClientEnvironmentType.Stage;
        }

        if (hostName.IndexOf("dev") > -1)
        {
            ClientEnvironment = ClientEnvironmentType.Dev;
        }


        // default to the stage server if we're in the editor
#if UNITY_EDITOR
       ClientEnvironment = ClientEnvironmentType.Unity;
#endif
    }
}
