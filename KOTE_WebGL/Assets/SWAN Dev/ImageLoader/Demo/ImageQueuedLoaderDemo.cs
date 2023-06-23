// Created by SWAN DEV 2022
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageQueuedLoaderDemo : MonoBehaviour
{
    public GridLayoutGroup m_GridAndContainer;

    public IMBX.ImageQueuedLoader m_ImageQueuedLoader;

    public DImageDisplayHandler m_ImageDisplayHandler;

    public GameObject m_ListImageItemPrefab;

    public InputField m_Input_Filename;

    public int m_LoadImageIndex;

    public List<string> m_ImageUrls;

    void Start()
    {
        m_ImageQueuedLoader.m_OnImageLoaded = (result) =>
        {
            Debug.Log("Result index: " + result.m_Index);

            if (result.m_Texture)
            {
                GameObject listItem = Instantiate(m_ListImageItemPrefab);
                listItem.transform.SetParent(m_GridAndContainer.transform);
                listItem.SetActive(true);

                RawImage rawImage = listItem.GetComponentInChildren<RawImage>();
                m_ImageDisplayHandler.SetRawImage(rawImage, result.m_Texture); // set the texture and set size base on the display handler settings
            }
        };

        m_ImageQueuedLoader.m_OnAllImagesLoaded = () =>
        {
            Debug.Log("All added image Urls are loaded.");
        };
    }

    void Update()
    {
        m_GridAndContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(0, m_GridAndContainer.preferredHeight);
    }

    public void LoadImageWithCustomFilename()
    {
        LoadNextImage(m_Input_Filename.text);
    }

    public void LoadNextImage(string customFilename)
    {
        // Use the Add method to add new loading task
        m_ImageQueuedLoader.Add(m_ImageUrls[m_LoadImageIndex++], customFilename, customIndex: m_LoadImageIndex);

        if (m_LoadImageIndex >= m_ImageUrls.Count) m_LoadImageIndex = 0;
    }

    public void LoadNextImage2()
    {
        LoadNextImage(null);
        LoadNextImage(null);
    }

    public void LoadNextImage4()
    {
        LoadNextImage2();
        LoadNextImage2();
    }
}
