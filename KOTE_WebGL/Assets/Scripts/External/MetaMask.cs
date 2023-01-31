using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

public class MetaMask : MonoBehaviour
{
    public static MetaMask Instance { get; private set; } = null;

    /// <summary>
    /// Returns true if Metamask is installed
    /// </summary>
    public bool HasMetamask => hasMetaMask();

    /// <summary>
    /// When true, we're waiting for the user to input their details about their account.
    /// </summary>
    public bool AwaitingAccount => awaitingAccountDetails;

    private bool awaitingAccountDetails = false;
    private UnityEvent<string> accountSuccess = null;
    private UnityEvent accountFail = null;

    /// <summary>
    /// Returns the user's account. You can check if this has been populated via <see cref="HasAccount"/>.
    /// </summary>
    public string Account => account;

    private string account = null;

    /// <summary>
    /// Returns if the account has been set up yet.
    /// If false, you can call <see cref="GetAccount"/> to get the user's Account.
    /// </summary>
    public bool HasAccount => account != null;

    /// <summary>
    /// True when the user is in the middle of signing a message.
    /// </summary>
    public bool SigningMessage => currentlySigningMessage;

    private bool currentlySigningMessage = false;
    private UnityEvent<string> messageSuccess = null;
    private UnityEvent messageFail = null;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning($"[MetaMask] There is more than 1 instance of MetaMask.cs. Deleting it.");
            Destroy(this);
        }

        DontDestroyOnLoad(gameObject);
        gameObject.name = "MetaMask";
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    #region JS Calls

    /// <summary>
    /// Asks the user for their account info. Be sure to check if <see cref="AwaitingAccount"/> is true.
    /// If ture, the previous call will be over written.
    /// </summary>
    /// <param name="OnSuccess">Will be called when the account has been grabbed.</param>
    /// <param name="OnFail">Will be called if we can't get the user's account.</param>
    public void GetAccount(UnityEvent<string> OnSuccess = null, UnityEvent OnError = null)
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        Debug.LogError($"[MetaMask | {gameObject.name}] Enviroment not in WebGL. Can not access MetaMask.");
        OnError?.Invoke();
        return;
#endif
        if (!HasMetamask)
        {
            Debug.LogError(
                $"[MetaMask | {gameObject.name}] MetaMask is not found in the browser. Be sure to install it!");
            OnError?.Invoke();
            return;
        }

        if (awaitingAccountDetails)
        {
            Debug.LogError(
                $"[MetaMask | {gameObject.name}] Still waiting for user to give account info. Making multiple calls overrides the previous call.\n" +
                $"Please check if 'AwaitingAccount' before trying to get an account again.");
            OnError?.Invoke();
            return;
        }

        awaitingAccountDetails = true;
        accountSuccess = OnSuccess;
        accountFail = OnError;
        MetamaskSelectedAccount("AccountSuccess", "AccountError", gameObject.name);
    }

    /// <summary>
    /// Asks the user to sign a message. Please check <see cref="SigningMessage"/> to see if the user is currently signing a message or not.
    /// You can not make multiple calls when signing messages.
    /// </summary>
    /// <param name="OnSuccess">Sends the signed message back.</param>
    /// <param name="OnError">Gets called if the message could not be signed.</param>
    public void SignMessage(string message, UnityEvent<string> OnSuccess, UnityEvent OnError = null)
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        Debug.LogError($"[MetaMask | {gameObject.name}] Enviroment not in WebGL. Can not access MetaMask.");
        OnError?.Invoke();
        return;
#endif
        if (!HasMetamask)
        {
            Debug.LogError(
                $"[MetaMask | {gameObject.name}] MetaMask is not found in the browser. Be sure to install it!");
            OnError?.Invoke();
            return;
        }

        if (SigningMessage)
        {
            Debug.LogError(
                $"[MetaMask | {gameObject.name}] You need to wait for the previous message to be signed or to fail first. Check 'SingingMessage' to see if MetaMask is busy or not.");
            OnError?.Invoke();
            return;
        }

        if (!HasAccount)
        {
            Debug.LogError(
                $"[MetaMask | {gameObject.name}] You need to have the account set up before you can sign a messgae.");
            OnError?.Invoke();
            return;
        }

        messageSuccess = OnSuccess;
        messageFail = OnError;

        MetamaskPersonalSign(Account, message, "MessageSuccess", "MessageError", gameObject.name);
    }

    
    // callbacks to clear the awaiting account details blocker
    private void OnSignSuccess(string data)
    {
        currentlySigningMessage = false;
    }

    private void OnSignFailure()
    {
        currentlySigningMessage = false;
    }
    #endregion

    #region JS Callbacks

    /// <summary>
    /// Called by metamask when we are given an address
    /// </summary>
    /// <param name="wallet">The account address</param>
    private void AccountSuccess(string wallet)
    {
        awaitingAccountDetails = false;
        Debug.Log($"[MetaMask | {gameObject.name}] Account Selected: {wallet}");
        account = wallet;
        accountFail = null;
        if (accountSuccess != null)
        {
            accountSuccess.Invoke(wallet);
            accountSuccess = null;
        }
    }

    /// <summary>
    /// Called by metamask when we are denied an address
    /// </summary>
    /// <param name="json">The related error</param>
    private void AccountError(string json)
    {
        awaitingAccountDetails = false;
        account = null;
        var error = GetRequestError(json);
        Debug.LogError($"[MetaMask] {error.ToString()}");
        accountSuccess = null;
        if (accountFail != null)
        {
            accountFail.Invoke();
            accountFail = null;
        }
    }

    /// <summary>
    /// Grabs the signed message from metamask
    /// </summary>
    /// <param name="signedMessage">The signed message</param>
    private void MessageSuccess(string signedMessage)
    {
        messageFail = null;
        currentlySigningMessage = false;
        messageSuccess.Invoke(signedMessage);
    }

    /// <summary>
    /// Gets an error if metamask could not sign a message
    /// </summary>
    /// <param name="json">The error as json</param>
    private void MessageFail(string json)
    {
        var error = GetRequestError(json);
        Debug.LogError($"[MetaMask] {error.ToString()}");
        messageSuccess = null;
        if (messageFail != null)
        {
            messageFail.Invoke();
        }

        currentlySigningMessage = false;
    }

    private void AccountChanged(string account)
    {
        account = account.TrimStart('[').TrimEnd(']');
        if (string.IsNullOrEmpty(account))
        {
            GameManager.Instance.EVENT_WALLET_DISCONNECTED.Invoke();
        }
        account = account.Trim('"');
        Debug.Log("[MetaMask] Account Change Received, new account data: " + account);

        AccountSuccess(account);
        GameManager.Instance.EVENT_WALLET_ADDRESS_RECEIVED.Invoke(account);
    }
    #endregion

    private bool hasMetaMask()
    {
        bool metaMask = false;
#if UNITY_WEBGL && !UNITY_EDITOR
        metaMask = IsMetamaskInstalled();
#endif
        return metaMask;
    }

    private static RequestError GetRequestError(string json)
    {
        return (RequestError)JsonUtility.FromJson(json, typeof(RequestError));
    }

    #region JavascriptServiceLayer

    /// <summary>
    /// Checks to see if a unity instance is set or not
    /// </summary>
    /// <returns>True if "unity" is set.</returns>
    [DllImport("__Internal")]
    private static extern bool HasUnityInstance();

    /// <summary>
    /// Checks to see if the client has MetaMask installed.
    /// </summary>
    /// <returns>True if any eth wallet is found</returns>
    [DllImport("__Internal")]
    private static extern bool IsMetamaskInstalled();

    /// <summary>
    /// Asks the user for their wallet's address
    /// </summary>
    /// <param name="success">Method to call on success (Method Name)</param>
    /// <param name="error">Method to call on error (Method Name)</param>
    /// <param name="gameobject">Which game object to send the info to (By Name). Default: "MetaMask"</param>
    [DllImport("__Internal")]
    private static extern void MetamaskSelectedAccount(string success, string error = null,
        string gameobject = "MetaMask");

    /// <summary>
    /// Asks a user to sign a message with their wallet.
    /// </summary>
    /// <param name="account">The user's wallet address.</param>
    /// <param name="message">The message to sign.</param>
    /// <param name="success">Method to call on success (Method Name)</param>
    /// <param name="error">Method to call on error (Method Name)</param>
    /// <param name="gameobject">Which game object to send the info to (By Name). Default: "MetaMask"</param>
    [DllImport("__Internal")]
    private static extern void MetamaskPersonalSign(string account, string message, string success, string error = null,
        string gameobject = "MetaMask");

    #endregion

    #region Helper Classes

    /// <summary>
    /// Errors that are returned by MetaMask
    /// </summary>
    [Serializable]
    public class RequestError
    {
        public RequestError(int code, string message, string stack = "")
        {
            this.code = code;
            this.message = message;
            this.stack = stack;
        }

        public RequestError()
        {
        }

        /// <summary>
        /// The error code
        /// </summary>
        public int code;

        /// <summary>
        /// The error message
        /// </summary>
        public string message;

        /// <summary>
        /// The error stack
        /// </summary>
        public string stack;

        public override string ToString()
        {
            return $"({code}): {message}";
        }
    }

    #endregion
}