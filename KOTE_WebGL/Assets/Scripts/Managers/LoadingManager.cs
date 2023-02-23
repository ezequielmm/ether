using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


public class LoadingManager : MonoBehaviour
{
    [SerializeField] TextMeshPro loadingText;
    [SerializeField] Slider slideBar;

    private void Start()
    {
        StartCoroutine(LoadAsynchronously(GameManager.Instance.nextSceneToLoad.ToString()));
        DontDestroyOnLoad(gameObject);
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
        GameManager.Instance.SceneLoaded();
        Destroy(gameObject);
    }

}
