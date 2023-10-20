using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;


[System.Serializable]
public class LoginData
{
    [JsonProperty("token")] public string Token;
    [JsonProperty("wallet")] public string Wallet;
}

public class AuthenticationManager : SingleTon<AuthenticationManager>
{
    public static LoginData LoginData;
    public static string Token;


    private void Start()
    {
        GameManager.Instance.EVENT_REQUEST_LOGOUT_COMPLETED.AddListener(ClearSessionToken);
        Login();
    }

    public bool Authenticated => !string.IsNullOrEmpty(GetSessionToken());

    public async UniTask<bool> Login()
    {
        if (Token == null)
        {
            Debug.LogError("Cant init with null token in auth manager");
        }
        var token = Token;


        string wallet = ExtractSubject(token);
        Debug.Log("Wallet is " + wallet);   
        LoginData = new LoginData { Token = Token, Wallet = wallet };
        return Authenticate(LoginData.Token);
    }

    private string ExtractSubject(string token)
    {
        return JwtTokenUtility.ParseJwtToken(token).Sub;
    }

    public bool AuthenticateOnResume()
    {
        return Authenticate(GetSessionToken());
    }

    public async void Logout()
    {
        Token = null;
        LoginData = null;
        try {
            await FetchData.Instance.Logout();
        }
        catch(Exception e)
        {
            Debug.LogException(e);
        }
       
        //ClearSessionToken();
        //WebBridge.SendUnityMessage("logout", "unity-client-logout");
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
        await UserDataManager.Instance.UpdatePlayerProfile();
        WalletManager.Instance.SetActiveWallet();
        GameManager.Instance.OnAuthenticated();
    }

    public void SetSessionToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            PlayerPrefs.DeleteKey("session_token");
        else
        {
            PlayerPrefs.SetString("session_token", token);
            PlayerPrefs.SetString("login_time", DateTime.UtcNow.ToString());
        }

        PlayerPrefs.Save();
    }

    public void ClearSessionToken()
    {
        PlayerPrefs.DeleteKey("session_token");
        PlayerPrefs.Save();
    }

    // override for listener
    public void ClearSessionToken(string data)
    {
        ClearSessionToken();
    }

    public string GetSessionToken()
    {
        string savedLoginTime = PlayerPrefs.GetString("login_time");
        if (string.IsNullOrEmpty(savedLoginTime)) return null;

        int hoursElapsed = CalculateHoursSinceLastLoginTime(savedLoginTime);

        if (hoursElapsed >= GameSettings.TOKEN_EXPIRATION_HOURS)
        {
            Debug.Log(
                $"Login Token Expired. Token Created At: {DateTime.Parse(savedLoginTime)} Expired at: {DateTime.Parse(savedLoginTime).AddHours(6)} Current Time is {DateTime.UtcNow}");
            ClearSessionToken();
            return null;
        }

        // update the cache time, since the player is still active.
        PlayerPrefs.SetString("login_time", DateTime.UtcNow.ToString());
        return PlayerPrefs.GetString("session_token");
    }

    private int CalculateHoursSinceLastLoginTime(string savedTime)
    {
        DateTime time = DateTime.Parse(savedTime);
        TimeSpan timeElapsed = DateTime.UtcNow - time;
        return timeElapsed.Hours;
    }

}

