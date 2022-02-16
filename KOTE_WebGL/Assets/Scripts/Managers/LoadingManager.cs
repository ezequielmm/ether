using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


public class LoadingManager : MonoBehaviour
{
    [SerializeField] GameManager GameManagerLocal;
    [SerializeField] TextMeshPro loadingText;
    [SerializeField] Slider slideBar;
    [SerializeField] bool enableLoad = true;
    public string NextSceneToLoad = default(inGameScenes).ToString();

    private void Start()
    {
        GameManagerLocal = GameManager.instance.gameObject.GetComponent<GameManager>(); // getting the current GameManager from the singleton
    }
    private void OnEnable()
    {
        enableLoad = true;
    }

    void Update()
    {
        if (enableLoad)
        {
            NextSceneToLoad = GameManagerLocal.nextSceneToLoad.ToString();
            LoadLevel(NextSceneToLoad); //loads the next scene
            enableLoad = false;
        }
    }

    public void LoadLevel(string sceneName)
    {
        StartCoroutine(LoadAsynchronously(sceneName));
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
        GameManagerLocal.nextSceneToLoad = inGameScenes.LoaderScene; //sets the next scene to load to loaderscene to prevent multiple loads and to come back to the loading scene when there is a need to change again

    }







}
