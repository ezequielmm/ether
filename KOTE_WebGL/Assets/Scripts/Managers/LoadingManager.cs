using System.Collections;
using TMPro;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    [SerializeField] TextMeshPro loadingText;
    [SerializeField] Slider slideBar;
    [SerializeField] private Loader loader;

    [SerializeField] private bool startLocalEnvironment;
    private bool IsBusy = false;

#if UNITY_EDITOR
    string editorToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIweDQzQjg0MUE3NTM1RkQzMTBCYmVkZkNGNTREZURmNURDNDY4NUU1NUIiLCJ0eXBlIjoic2Vzc2lvbiIsImlhdCI6MTY4NDYwMzg2MywiZXhwIjoxNjg1MjA4NjYzfQ.hhOLSksyslwPqUDQZcXM-XYSHNKkyN9uf-AS2Md6-zE";
#endif

    public void Login(string loginData)
    {   
        if(IsBusy)
            return;

        IsBusy = true;
     

        
        loader.Show();
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
        await ClientEnvironmentManager.Instance.StartEnvironmentManger(startLocalEnvironment);
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