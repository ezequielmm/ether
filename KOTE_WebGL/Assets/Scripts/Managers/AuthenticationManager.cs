using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AuthenticationManager : SingleTon<AuthenticationManager>
{
    public string Token;
    private void Start()
    {
        GameManager.Instance.EVENT_REQUEST_LOGOUT_COMPLETED.AddListener(ClearSessionToken);
    }

    public bool Authenticated => !string.IsNullOrEmpty(GetSessionToken());

    public string Wallet = "0xAFeBa5DD120a3Ea8b44BBB13c5190715772dc9aB";

    public async UniTask<bool> Login()
    {
        string token = await FetchData.Instance.GetToken();
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