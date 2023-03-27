using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AuthenticationManager : SingleTon<AuthenticationManager>
{
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

    public bool AuthenticateOnResume()
    {
        return Authenticate(GetSessionToken());
    }

    public async void Logout()
    {
        await FetchData.Instance.Logout();
        ClearSessionToken();
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
        GameManager.Instance.EVENT_AUTHENTICATED.Invoke();
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

    public string GetSessionToken()
    {
        string savedLoginTime = PlayerPrefs.GetString("login_time");
        if (string.IsNullOrEmpty(savedLoginTime)) return null;

        int hoursElapsed = CalculateHoursSinceLastLoginTime(savedLoginTime);

        if (hoursElapsed >= 6)
        {
            Debug.Log("Login Token Expired");
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
        TimeSpan timeElapsed = DateTime.Now - time;
        return timeElapsed.Hours;
    }

    private string HashPassword(string password)
    {
        // TODO: In theory this should hash the password
        // with a known algorithm to make it harder
        // to make a Man in the Middle Attack.
        // This needs the support of the Admin Panel 
        // to make sure resetting a password works too.

        // This warning will be here until this is implemented because it's a secutiry risk
        Debug.LogWarning(
            $"[UserDataManager] Warning: Passwords are not being hashed. Sending raw passwords over the internet is dangerous! (Yes, even over https!!)");

        return password;
    }
}