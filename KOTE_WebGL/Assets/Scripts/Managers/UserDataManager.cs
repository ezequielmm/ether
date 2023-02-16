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
    public string ActiveNft = "";
    
    // get the unique identifier for this instance of the client
    public string ClientId
    {
        get
        {
            string id = PlayerPrefs.GetString("client_id");
            // if the client id doesn't exist, create one and save it
            if (string.IsNullOrEmpty(id))
            {
                Guid newId = Guid.NewGuid();
                id = newId.ToString();
                PlayerPrefs.SetString("client_id", newId.ToString());
            }

            return id;
        }
    }

    private void Awake()
    {
        base.Awake();
    }
    
    private void Start()
    {
        GameManager.Instance.EVENT_PLAYER_STATUS_UPDATE.AddListener(OnExpeditionUpdate);
        GameManager.Instance.EVENT_REQUEST_PROFILE_SUCCESSFUL.AddListener(OnPlayerProfileReceived);
        GameManager.Instance.EVENT_REQUEST_LOGOUT_SUCCESSFUL.AddListener(OnLogout);
        GameManager.Instance.EVENT_EXPEDITION_STATUS_UPDATE.AddListener(OnExpeditionStatus);
    }

   

    private void OnExpeditionStatus(bool hasExpedtion, int nftId)
    {
        ActiveNft = nftId.ToString();
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
}
