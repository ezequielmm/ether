// Created by SWAN DEV 2022
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageBatchLoader_MemoryCache_Demo : MonoBehaviour
{
    [Tooltip("The loader Cache Management and Loading Settings.")]
    public IMBX.LoaderManagement LMGT;

    public GridLayoutGroup m_GridAndContainer;

    public DImageDisplayHandler m_ImageDisplayHandler;

    public GameObject m_ListImageItemPrefab;

    public int m_LoadImageIndex;

    public List<string> m_ImageUrls;

    [Header("Runtime/Debug, do not modify directly:")]
    [SerializeField] private List<IMBX.ImageBatchLoader.CacheItem> _currentCachedItems;

    private IMBX.ImageBatchLoader _imageBatchLoader;

    void Start()
    {
        _imageBatchLoader = new IMBX.ImageBatchLoader(100);
        _imageBatchLoader.LMGT = LMGT;

        // Enable memory cache feature, set cache nums...
        // !!! Please make sure maxMemoryCacheNum is always larger than your total UI objects,
        // else the earliest cached textures will be cleared when the limit exceeded.
        _imageBatchLoader.EnableMemoryCache(maxLoaderNum: 3, maxMemoryCacheNum: 30, maxQueueSize: 30, dontDestroyOnLoad: false);

        _imageBatchLoader.m_OnImageLoaded = (result) =>
        {
            if (result.m_Texture == null && result.m_FileDataReserved)
            {
                // ** Decode texture using custom image decoder.. *** Remember to set the new texture to result.m_Texture !
                result.m_Texture = IMBX.ImageLoader.CreateTextureByData(result.m_Data); // this is an example, still the Unity built-in image decoder
                Debug.Log("Decode texture using your own image decoder..");
            }

            if (result.m_Texture)
            {
                _OnImageGetOrLoaded(result.m_Texture);

                if (LMGT.IsDebug)
                {
                    Debug.Log(string.Format("CacheFilename: {0}, DetectedFileMime: {1}, DetectedFileExtension: {2}\nCacheFolderPath: {3}\nData Len: {4}, URL: {5}", result.m_CacheFilename,
                        result.m_DetectedFileMime, result.m_DetectedFileExtension, LMGT.CacheFolderPath, (result.m_Data != null ? result.m_Data.Length.ToString() : "null"), result.m_URL));
                }
            }

            _currentCachedItems = _imageBatchLoader.MemoryCache.GetCacheItems();
        };

        _imageBatchLoader.m_OnAllImagesLoaded = () =>
        {
            Debug.Log("All added image Urls are loaded.");
        };
    }

    void Update()
    {
        m_GridAndContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(0, m_GridAndContainer.preferredHeight);
    }

    public void LoadNextImage()
    {
        Texture2D texture = _imageBatchLoader.MemoryCache.GetImageByUrl(m_ImageUrls[m_LoadImageIndex++], loadIfNotFound: true, isLock: false, customFilename: null);
        if (texture)
        {
            _OnImageGetOrLoaded(texture);
        }

        // * You can use the GetMemoryCacheItemByUrl method to request the images as well, which more information included in the CacheItem object:
        //var cacheItem = _imageBatchLoader.MemoryCache.GetMemoryCacheItemByUrl(m_ImageUrls[m_LoadImageIndex++], loadIfNotFound: true, isLock: false);
        //if (cacheItem != null && cacheItem.m_Texture)
        //{
        //    _OnImageGetOrLoaded(cacheItem.m_Texture);
        //}

        if (m_LoadImageIndex >= m_ImageUrls.Count) m_LoadImageIndex = 0;
    }

    public void LoadNextImage2()
    {
        LoadNextImage();
        LoadNextImage();
    }

    public void LoadNextImage4()
    {
        LoadNextImage2();
        LoadNextImage2();
    }

    private void _OnImageGetOrLoaded(Texture2D texture)
    {
        GameObject listItem = Instantiate(m_ListImageItemPrefab);
        listItem.transform.SetParent(m_GridAndContainer.transform);
        listItem.SetActive(true);

        RawImage rawImage = listItem.GetComponentInChildren<RawImage>();
        m_ImageDisplayHandler.SetRawImage(rawImage, texture); // set the texture and set size base on the display handler settings
    }
}
