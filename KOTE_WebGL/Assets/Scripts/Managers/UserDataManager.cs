using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UserDataManager : SingleTon<UserDataManager>
{
    public string UserEmail => profile?.email ?? string.Empty;
    public string ExpeditionId { get; private set; } = "";
    public bool HasExpedition => expeditionStatus?.HasExpedition ?? false;
    public int ActiveNft => expeditionStatus?.NftId ?? -1;
    public List<string> VerifiedWallets => profile?.ownedWallets ?? new();

    ProfileData profile = null;
    ExpeditionStatus expeditionStatus = null;

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

    protected override void Awake()
    {
        base.Awake();
    }
    
    private void Start()
    {
        GameManager.Instance.EVENT_PLAYER_STATUS_UPDATE.AddListener(OnExpeditionUpdate);
    }

    public async UniTask<bool> Login(string email, string password) 
    {
        string hashedPassword = HashPassword(password);
        string token = await FetchData.Instance.GetTokenByLogin(email, hashedPassword);
        return Authenticate(token);
    }

    public async UniTask<bool> Register(string name, string email, string password)
    {
        string hashedPassword = HashPassword(password);
        string token = await FetchData.Instance.GetTokenByRegistration(name, email, hashedPassword);
        return Authenticate(token);
    }

    private bool Authenticate(string token) 
    {
        SetSessionToken(token);
        if (string.IsNullOrEmpty(token))
        {
            return false;
        }
        AuthenticationFlow();
        return true;
    }

    private async void AuthenticationFlow() 
    {
        await UpdatePlayerProfile();
        
        WalletManager.Instance.SetActiveWallet();

        GameManager.Instance.EVENT_AUTHENTICATED.Invoke();
    }

    public void SetSessionToken(string token) 
    {
        PlayerPrefs.SetString("session_token", token);
        PlayerPrefs.Save();
    }

    public string GetSessionToken() 
    {
        return PlayerPrefs.GetString("session_token");
    }

    public async UniTask UpdatePlayerProfile() 
    {
        profile = await FetchData.Instance.GetPlayerProfile();
        GameManager.Instance.EVENT_UPDATE_NAME_AND_FIEF.Invoke(profile.name, profile.fief);
    }

    public async UniTask UpdateExpeditionStatus() 
    {
        expeditionStatus = await FetchData.Instance.GetExpeditionStatus();
    }

    public async void ClearExpedition()
    {
        expeditionStatus = null;
        await SendData.Instance.ClearExpedition();
    }


    private void OnExpeditionUpdate(PlayerStateData playerState)
    {
        ExpeditionId = playerState.data.expeditionId;
    }

    private string HashPassword(string password) 
    {
        // TODO: In theory this should hash the password
        // with a known algorithm to make it harder
        // to make a Man in the Middle Attack.
        // This needs the support of the Admin Panel 
        // to make sure resetting a password works too.

        // This warning will be here until this is implemented because it's a secutiry risk
        Debug.LogWarning($"[UserDataManager] Warning: Passwords are not being hashed. Sending raw passwords over the internet is dangerous! (Yes, even over https!!)");

        return password;
    } 
}
