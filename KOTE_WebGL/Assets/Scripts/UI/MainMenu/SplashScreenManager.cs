using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SplashScreenManager : MonoBehaviour
{
    public GameObject Content;
    public TextMeshProUGUI initialScreenText;
    public DOTweenAnimation textAnimation;

    private bool keyPressed = false;
    private bool showText = false;

    private void Awake()
    {
        Content.SetActive(true);
        initialScreenText.text = "Fetching Wallet Contents...";
    }

    private void Start()
    {
        // There is a huge memory leak when the game is loading all the player skins
        // Ideally we want a manager to load everything before showing interactable objects
        PlayerSpriteManager.Instance.OnSkinLoaded.AddListener(OnGameDataReady);
    }

    void Update()
    {
        if (!showText)
        {
            return;
        }
        if (Input.anyKeyDown && !keyPressed)
        {
            keyPressed = true;
            gameObject.SetActive(false);
        }
    }

    private void OnGameDataReady()
    {
        showText = true;
        initialScreenText.text = "Press Any Key To Start";
        textAnimation.DOPlay();
        PlayerSpriteManager.Instance.OnSkinLoaded.RemoveListener(OnGameDataReady);
    }
}
