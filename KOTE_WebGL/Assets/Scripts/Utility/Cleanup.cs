using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cleanup : MonoBehaviour
{
    private static string SceneName = "Cleanup";
    
    void Start()
    {
        StartCoroutine(CleanupRoutine());      
    }
    
    public static void CleanupGame() =>
        SceneManager.LoadScene(SceneName);

    private IEnumerator CleanupRoutine()
    {
        yield return null;
        WebSocketManager.Instance.DestroyInstance();
        WebRequesterManager.Instance.DestroyInstance();
        ContestManager.Instance.DestroyInstance();
        MetaMask.Instance.DestroyInstance();
        AuthenticationManager.Instance.DestroyInstance();
        ScoreboardManager.Instance.DestroyInstance();
        ServerCommunicationLogger.Instance.DestroyInstance();
        SoundManager.Instance.DestroyInstance();
        UserActivityMonitor.Instance.DestroyInstance();
        GameManager.Instance.DestroyInstance();
        yield return null;
        SceneManager.LoadScene(0); 
    }


}
