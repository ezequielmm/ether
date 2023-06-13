using System.Collections;
using TMPro;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    public GameObject LoadGroup;
    [SerializeField] TextMeshPro loadingText;
    [SerializeField] Slider slideBar;
    [SerializeField] private Loader loader;
    
    [SerializeField] private AssetReference soundManager;
    public static bool Won { get; internal set; }

    private bool IsBusy = false;

#if UNITY_EDITOR
    public string editorToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIweDQzQjg0MUE3NTM1RkQzMTBCYmVkZkNGNTREZURmNURDNDY4NUU1NUIiLCJ0eXBlIjoic2Vzc2lvbiIsImlhdCI6MTY4NDYwMzg2MywiZXhwIjoxNjg1MjA4NjYzfQ.hhOLSksyslwPqUDQZcXM-XYSHNKkyN9uf-AS2Md6-zE";

#endif
    private void Start()
    {
#if UNITY_EDITOR
        StartCoroutine(MockLogin());

        IEnumerator MockLogin()
        {
            yield return new WaitForSeconds(2);
            Login(editorToken);
        }
#endif

        if (!GameManager.Instance.firstLoad || Won)
        {
            Debug.Log("Not first load, we went back to loader, now use the saved token");
            Login(AuthenticationManager.Token);
        }


    }
   
    public void Login(string loginData)
    {
        AuthenticationManager.Token = loginData;
        Debug.Log("Init login with " + loginData);
        loader.Show();
        if (GameManager.Instance.firstLoad)
        {
            Debug.Log("load environment, first load");
            LoadWithEnvironmentCheck();
            return;
        }
        Debug.Log("Login not first load ::  " + GameManager.Instance.nextSceneToLoad.ToString());
        LoadScene();
        DontDestroyOnLoad(gameObject);
        if (LoadGroup)
            LoadGroup.SetActive(true);
    }

    private async void LoadWithEnvironmentCheck()
    {
        await ClientEnvironmentManager.Instance.StartEnvironmentManger();
        LoadScene();
        GameManager.Instance.firstLoad = false;
    }

    private void LoadScene()
    {
        StartCoroutine(LoadAsynchronously(GameManager.Instance.nextSceneToLoad.ToString()));
    }
    

    IEnumerator LoadAsynchronously(string sceneName)
    {
        // The Application loads the Scene in the background as the current Scene runs.
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        Debug.Log(" LoadAsynchronously " + sceneName);
        // Wait until the asynchronous scene fully loads

        var handler = soundManager.InstantiateAsync();
        while(!handler.IsDone)
        {
            if (loadingText != null && slideBar != null)
            {
                loadingText.text = $"Loading sounds...\n({100 * asyncLoad.progress}%)"; //shows percentage
                slideBar.value = asyncLoad.progress; //charges the load bar
            }
            yield return null;
        }
        SoundManager.Instance.PlaySfx(SoundTypes.Card, "Play");
        
        while (!asyncLoad.isDone)
        {
            if (loadingText != null && slideBar != null)
            {
                loadingText.text = $"Loading {sceneName}...\n({100 * asyncLoad.progress}%)"; //shows percentage
                slideBar.value = asyncLoad.progress; //charges the load bar
            }
            //Debug.Log(asyncLoad.progress);
            yield return null;
        }
        // Game manager is now listening for the sceneLoaded event from SceneManager, instead of calling it directly
        Destroy(gameObject);
    }
    
    IEnumerator LoadAsynchronouslyFromAddressables(string sceneName)
    {
        var asyncLoad = Addressables.LoadSceneAsync("MainMenu");
        while (!asyncLoad.IsDone)
        {
            if (loadingText != null && slideBar != null)
            {
                loadingText.text = "Loading...\n(" + 100 * asyncLoad.PercentComplete + "%)"; //shows percentage
                slideBar.value = asyncLoad.PercentComplete; //charges the load bar
            }
            yield return null;
        }
        // Game manager is now listening for the sceneLoaded event from SceneManager, instead of calling it directly
        Destroy(gameObject);
    }
}