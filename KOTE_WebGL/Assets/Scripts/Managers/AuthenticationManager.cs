using System.Collections;
using System.Collections.Generic;
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
            PlayerPrefs.SetString("session_token", token);
        PlayerPrefs.Save();
    }

    public string GetSessionToken() 
    {
        return PlayerPrefs.GetString("session_token");
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
