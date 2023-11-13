using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestDownloadImages : MonoBehaviour
{
    public string url;
    public Image image;

    private void Awake()
    {
        ClientEnvironmentManager.Instance.StartEnvironmentManger();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
            DownloadImage();
    }

    private void DownloadImage()
    {
        var nft = new Nft()
        {
            adaptedImageURI = url
        };
        PortraitSpriteManager.Instance.GetKnightPortrait(nft, sprite => {
            image.sprite = sprite;
        });
    }
}
