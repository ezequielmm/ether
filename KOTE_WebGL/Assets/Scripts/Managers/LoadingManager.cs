using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    [SerializeField] TextMeshPro loadingText;
    [SerializeField] Slider slideBar;

    private void Start()
    {
        if (GameManager.Instance.firstLoad)
        {
            LoadWithEnvironmentCheck();
            return;
        }

        StartCoroutine(LoadAsynchronously(GameManager.Instance.nextSceneToLoad.ToString()));
        DontDestroyOnLoad(gameObject);
    }

    private async void LoadWithEnvironmentCheck()
    {
        await ClientEnvironmentManager.Instance.StartEnvironmentManger();
        StartCoroutine(LoadAsynchronously(GameManager.Instance.nextSceneToLoad.ToString()));
        GameManager.Instance.firstLoad = false;
    }

    IEnumerator LoadAsynchronously(string sceneName)
    {
        // The Application loads the Scene in the background as the current Scene runs.

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            if (asyncLoad != null && loadingText != null && slideBar != null)
            {
                loadingText.text = "Loading...\n(" + 100 * asyncLoad.progress + "%)"; //shows percentage
                slideBar.value = asyncLoad.progress; //charges the load bar
            }

            //Debug.Log(asyncLoad.progress);
            yield return null;
        }

        // Game manager is now listening for the sceneLoaded event from SceneManager, instead of calling it directly
        Destroy(gameObject);
    }
}