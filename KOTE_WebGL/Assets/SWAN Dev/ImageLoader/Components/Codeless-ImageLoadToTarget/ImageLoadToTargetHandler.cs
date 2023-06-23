/// <summary>
/// Created by SWAN DEV 2019
/// </summary>
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace IMBX
{
    public class ImageLoadToTargetHandler : MonoBehaviour
    {
        [Tooltip("If 'true', show debug logs for this loading task.")]
        public bool m_IsDebug;

        [Tooltip("Auto-start to load this image if this flag is 'true', or manually call the Load method of this script.")]
        public bool m_AutoLoadOnStart = true;

        [Tooltip("For UI component(Image/RawImage), set size using ImageDisplayHandler settings or use Native Size when the image is loaded. For renderer(e.g. Cube, Plane..), set size base on the X value of the local scale of the Transform.")]
        public bool m_SetSizeOnLoaded = true;

        [Tooltip("Image display handler for setting image size on UGUI Image and RawImage components.")]
        public DImageDisplayHandler m_ImageDisplayHandler;

        [Header("[ Local Directory (Save & Load) ]")]
        [Tooltip("Set this to 'true' if this image is in the StreamingAssets folder that shipped with the app, and you want to copy to the local directory.")]
        public bool m_CopyFromStreamingAssetsPath;
        [Tooltip("The directory to load & save the image.\n* StreamingAssetsPath is not suitable for caching files on some platforms\n* DataPath is for use in Editor only")]
        public FilePathName.AppPath m_LocalDirectory = FilePathName.AppPath.PersistentDataPath;
        [Tooltip("A sub-folder for this image to load & save in the local directory.")]
        public string m_FolderName;
        [Tooltip("(Optional) A custom file name for this image to load & save in the local cache folder. (If this is empty, a unique filename will be generated using the image path/URL)" +
            "\nReminded: please ensure the filename is unique for each image, multiple images using the same path will replace each other repeatedly.")]
        public string m_FileNameWithExtension;

        [Header("[ Load URL ]")]
        [Tooltip("Set this to 'true' if your image has to be downloaded from Web storage for the first time. Provide the image link below.")]
        public bool m_LoadByImageUrl;
        [Tooltip("The image link for downloading the image from Web storage.")]
        public string m_ImageUrl;
        [Tooltip("The maximum retry number when the image can not be loaded by URL.")]
        public uint m_Retry = 3;
        [Tooltip("No Cache: do not save the downloaded images in the cache folder." +
            "\nUse Cached: download the image by URL at the first time, and save in the cache folder. Load it from the cache folder after that." +
            "\nReplace: download the image by URL everytime, and replace the same image in the cache folder.")]
        public ImageLoader.CacheMode m_CacheMode = ImageLoader.CacheMode.UseCached;

        [Header("[ Default Image Replacement ]")]
        [Tooltip("(Optional) If the image can not be loaded, replace it with this.")]
        public Sprite m_DefaultImage;

        [Header("[ Events ]")]
        public UnityEvent m_OnLoaded;
        public UnityEvent m_OnFailed;

        public State STATE = State.None;
        public enum State
        {
            None = 0,
            Waiting,
            ReadyToLoad,
            Loaded,
        }

        private Texture2D _texture2d;
        private ImageLoader _loader;

        private void Start()
        {
            if (m_AutoLoadOnStart) Load();
        }

        private void OnDestroy()
        {
            if (_loader) _loader.Cancel();
            if (_texture2d) Destroy(_texture2d);
        }

        public void Load()
        {
            if ((!m_LoadByImageUrl && string.IsNullOrEmpty(m_FileNameWithExtension)) || (m_LoadByImageUrl && string.IsNullOrEmpty(m_ImageUrl)))
            {
                Debug.Log("Invalid file path! Missing filename or URL?");
                return;
            }

            if (STATE != State.Waiting)
            {
                ImageLoadToTargetManager.Instance.AddHandler(this);
            }
            if (STATE != State.ReadyToLoad) return;
            STATE = State.Loaded;

            // Actually start the loading process....
            string imagePath = string.Empty;
            if (m_LoadByImageUrl)
            {
                imagePath = m_ImageUrl;
            }
            else
            {
                m_CacheMode = ImageLoader.CacheMode.UseCached;

                string directory = FilePathName.Instance.GetAppPath(m_CopyFromStreamingAssetsPath ? FilePathName.AppPath.StreamingAssetsPath : m_LocalDirectory);
                imagePath = directory;
                if (!string.IsNullOrEmpty(m_FolderName))
                {
                    imagePath = Path.Combine(imagePath, m_FolderName);
                }
                imagePath = Path.Combine(imagePath, m_FileNameWithExtension);
            }

            _loader = ImageLoader.Create();
            _loader.LMGT.CacheDirectoryEnum = m_LocalDirectory;
            _loader.LMGT.IsDebug = m_IsDebug;

            bool noFilename = string.IsNullOrEmpty(m_FileNameWithExtension);
            if (noFilename)
            {
                _loader.LMGT.CacheAsPerUrl = true;
            }
            else
            {
                _loader.LMGT.CacheAsPerUrl = false;
                _loader.LMGT.FileExtension = Path.GetExtension(m_FileNameWithExtension);
                m_FileNameWithExtension = Path.GetFileNameWithoutExtension(m_FileNameWithExtension);
                _loader.LMGT.FileNamePrefix = m_FileNameWithExtension;
                _loader.LMGT.FileIndexFormatDigitsCount = 0;
            }

            _loader.Load(0, imagePath, noFilename ? "none" : m_FileNameWithExtension, m_FolderName, m_CacheMode, (texture2d, index) =>
            {
                if (this != null)
                {
                    if (_texture2d) Destroy(_texture2d);
                    if (texture2d)
                    {
                        _texture2d = texture2d;
                        _SetTexture(texture2d);
                        m_OnLoaded.Invoke();
                    }
                    else
                    {
                        Debug.LogWarning("Fail to load image, check if the path is correct and reachable: " + imagePath);
                        if (m_DefaultImage) _SetTexture(null);
                        m_OnFailed.Invoke();
                    }
                }
            }, m_LoadByImageUrl ? m_Retry : 0);
        }

        private void _SetTexture(Texture2D texture2d)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer)
            {
                float aspect = 1f;
                if (texture2d)
                {
                    renderer.material.mainTexture = texture2d;
                    aspect = (float)texture2d.width / texture2d.height;
                }
                else if (m_DefaultImage)
                {
                    renderer.material.mainTexture = m_DefaultImage.texture;
                    aspect = (float)m_DefaultImage.texture.width / m_DefaultImage.texture.height;
                }
                if (m_SetSizeOnLoaded)
                {
                    float x = transform.localScale.x;
                    float y = x / aspect;
                    float z = y;
                    transform.localScale = new Vector3(x, y, z);
                }
                return;
            }


            if (m_ImageDisplayHandler == null) m_ImageDisplayHandler = GetComponent<DImageDisplayHandler>();

            Image image = GetComponent<Image>();
            if (image)
            {
                if (m_SetSizeOnLoaded)
                {
                    if (m_ImageDisplayHandler)
                    {
                        if (texture2d)
                            m_ImageDisplayHandler.SetImage(image, texture2d);
                        else if (m_DefaultImage)
                            m_ImageDisplayHandler.SetImage(image, m_DefaultImage);
                    }
                    else
                    {
                        if (texture2d)
                            image.sprite = Sprite.Create(texture2d, new Rect(0, 0, texture2d.width, texture2d.height), new Vector2(0.5f, 0.5f));
                        else if (m_DefaultImage)
                            image.sprite = m_DefaultImage;
                        image.SetNativeSize();
                    }
                }
                else
                {
                    if (texture2d)
                        image.sprite = Sprite.Create(texture2d, new Rect(0, 0, texture2d.width, texture2d.height), new Vector2(0.5f, 0.5f));
                    else if (m_DefaultImage)
                        image.sprite = m_DefaultImage;
                }
                return;
            }

            RawImage rawImage = GetComponent<RawImage>();
            if (rawImage)
            {
                if (m_SetSizeOnLoaded)
                {
                    if (m_ImageDisplayHandler)
                    {
                        if (texture2d)
                            m_ImageDisplayHandler.SetRawImage(rawImage, texture2d);
                        else if (m_DefaultImage)
                            m_ImageDisplayHandler.SetRawImage(rawImage, m_DefaultImage);
                    }
                    else
                    {
                        if (texture2d)
                            rawImage.texture = texture2d;
                        else if (m_DefaultImage)
                            rawImage.texture = m_DefaultImage.texture;
                        rawImage.SetNativeSize();
                    }
                }
                else
                {
                    if (texture2d)
                        rawImage.texture = texture2d;
                    else if (m_DefaultImage)
                        rawImage.texture = m_DefaultImage.texture;
                }
                return;
            }
        }
    }
}
