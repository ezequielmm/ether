// Created by SwanDEV 2018
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_2017_3_OR_NEWER
using UnityEngine.Networking;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

// ImageBox namespace.
namespace IMBX
{
    /// <summary>
    /// Load image from web or local, with cache, retry and timeout options.
    /// </summary>
    public class ImageLoader : MonoBehaviour
    {
        [Serializable] public class LoadingTask
        {
            public string imageUrl;
            public CacheMode cacheMode;
            public string customFilename;
            [Tooltip("The index number can be assigned before loading multiple images, for sorting the results when needed." +
                "\nIf this is provided (larger than -1), this index will be set to the index of its Result object, which can be accessed in the callbacks.")]
            public int customIndex;

            [Tooltip("Indicates if this LoadingTask has been executed by the loader (Not necessarily finished).\n(Do not manually modify this variable)")]
            public bool _executed;
        }

        public enum CacheMode
        {
            /// <summary> Do not save the download image to the local cache directory. And, do not check and load the cached image from local. </summary>
            NoCache = 0,

            /// <summary> Check and load the cached image from local cache directory. Save the download image to the local cache directory if no same filename image exists. </summary>
            UseCached,

            /// <summary> Download and save the image to the local cache directory without checking cached images. Replace the local image if same filename. </summary>
            Replace,
        }

        public enum ErrorType
        {
            /// <summary> Failed to download the image due to Network/Connection/HTTP error when using UnityWebRequest/WWW. </summary>
            NetworkError,

            /// <summary> Failed to download the image within the provided time limit! Is the provided timeout value too small? Is the network connection good? </summary>
            TimeOut,

            /// <summary> Failed to load the local cached image because filename not provided! </summary>
            MissingFilename,

            /// <summary> Failed to create texture from the loaded byte array. Is it a Unity supported image format? </summary>
            InvalidImageData,
        }

        /// <summary>
        /// A callback to fire when failing to load an image, returns the ErrorType and URL(or message) in the callback. (Register this callback manually if needed)
        /// </summary>
        public Action<ErrorType, string> m_OnImageLoadError;

        /// <summary>
        /// The image byte array.
        /// </summary>
        public byte[] Data { get; private set; }
        public bool IsGif { get; private set; }
        public string DetectedFileMime { get; private set; }
        public string DetectedFileExtension { get; private set; }
        public string CacheFilename { get; private set; }
        public string URL { get; private set; }
        public uint Index { get; private set; }
        /// <summary>
        /// Indicate if the image file data is kept for this image.
        /// </summary>
        public bool FileDataReserved { get { return IsGif || LMGT.LoadFileMode != LoaderManagement.LoadingMode.Default; } }

        private float _timeOut = 30f;
        private Action<Texture2D, uint> _onComplete;

        private static int _GID = 0;
        public static int NextID { get { return ++_GID; } }
        /// <summary>
        /// The ID of the superior loader (e.g. ImageBatchLoader) - the parent loader that creates and uses this ImageLoader instance. (-1 means no superior loader, the instance is separately created)
        /// </summary>
        public int _ParentLoaderID = -1;

        /// <summary>
        /// The loader Cache Management and Loading Settings.
        /// </summary>
        public LoaderManagement LMGT = new LoaderManagement();

        /// <summary>
        /// Create an ImageLoader for loading image from web or local. (This instance will be automatically destroyed when finished)
        /// </summary>
        /// <param name="maxCacheFilePerFolder"> The maximum number of files can be saved per folder. Less or equals Zero means unlimited. </param>
        /// <param name="cacheDirectoryEnum"> The application path, default: Application.persistentDataPath. </param>
        public static ImageLoader Create(uint maxCacheFilePerFolder, FilePathName.AppPath cacheDirectoryEnum = FilePathName.AppPath.PersistentDataPath)
        {
            ImageLoader loader = Create();
            loader.LMGT.MaxCacheFilePerFolder = maxCacheFilePerFolder;
            loader.LMGT.CacheDirectoryEnum = cacheDirectoryEnum;
            return loader;
        }
        public static ImageLoader Create()
        {
            ImageLoader loader = new GameObject("[ImageLoader]").AddComponent<ImageLoader>();
            loader._stopped = false;
            return loader;
        }

        private static List<ImageLoader> currentLoaders = new List<ImageLoader>();
        /// <summary>
        /// Total existing loaders/loading processes (download, not local load).
        /// </summary>
        public static int LoadingCount
        {
            get
            {
                return currentLoaders.Count;
            }
        }
        /// <summary>
        /// Check if a given image URL is being loaded in any existing ImageLoader.
        /// </summary>
        public static bool IsUrlLoading(string imageUrl)
        {
            foreach (var loader in currentLoaders)
            {
                if (imageUrl == loader.URL)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Cancel and Destroy existing ImageLoader(s) by image URL, all loaders with the provided URL will be cancelled.
        /// (Unable to cancel a local load, as it finishes immediately)
        /// </summary>
        public static void CancelLoading(string imageUrl)
        {
            for (int i = 0; i < currentLoaders.Count; i++)
            {
                if (currentLoaders[i] && imageUrl == currentLoaders[i].URL)
                {
                    currentLoaders[i]._StopDownload();
                    currentLoaders.RemoveAt(i);
                }
            }
        }
        /// <summary>
        /// Cancel and Destroy existing ImageLoader(s) by custom index, all loaders with the provided Index will be cancelled.
        /// (Unable to cancel a local load, as it finishes immediately)
        /// </summary>
        public static void CancelLoading(int index)
        {
            for (int i = 0; i < currentLoaders.Count; i++)
            {
                if (currentLoaders[i] && index == currentLoaders[i].Index)
                {
                    currentLoaders[i]._StopDownload();
                    currentLoaders.RemoveAt(i);
                }
            }
        }
        /// <summary>
        /// Cancel and Destroy all the existing ImageLoaders. (Unable to cancel a local load, as it finishes immediately)
        /// </summary>
        public static void CancelAllLoading()
        {
            for (int i = 0; i < currentLoaders.Count; i++)
            {
                if (currentLoaders[i]) currentLoaders[i]._StopDownload();
                currentLoaders.RemoveAt(i);
            }
        }

        public static void _CancelLoadingsByParentLoaderID(int ID)
        {
            for (int i = 0; i < currentLoaders.Count; i++)
            {
                if (currentLoaders[i] && currentLoaders[i]._ParentLoaderID == ID)
                {
                    currentLoaders[i]._StopDownload();
                    currentLoaders.RemoveAt(i);
                }
            }
        }

        private static void _AddLoading(ImageLoader loader)
        {
            if (!currentLoaders.Contains(loader))
            {
                currentLoaders.Add(loader);
            }
#if UNITY_EDITOR
            if (loader.LMGT.IsDebug) Debug.Log("(Add) Loading count: " + LoadingCount);
#endif
        }
        private static void _RemoveLoading(ImageLoader loader)
        {
            if (currentLoaders.Contains(loader))
            {
                currentLoaders.Remove(loader);
            }
#if UNITY_EDITOR
            if (loader.LMGT.IsDebug) Debug.Log("(Remove) Loading count: " + LoadingCount);
#endif
        }

        /// <summary>
        /// Cancel and Destroy this loader. (Unable to cancel a local load, as it finishes immediately)
        /// </summary>
        public void Cancel()
        {
            if (currentLoaders.Contains(this)) currentLoaders.Remove(this);
            _StopDownload();
        }

        /// <summary>
        /// Delete all files in the cache folder, i.e. LMGT.CacheFolderPath.
        /// </summary>
        public void ClearStorageCache()
        {
            ClearFilesByFolder(LMGT.CacheFolderPath, fileExtension: null);
        }

        /// <summary>
        /// Start to load an image with the settings of the provided LoaderManagement object, return a Texture2D and the given index in the onComplete callback.
        /// </summary>
        /// <param name="index"> A number specified by you. Can be used to indicate the identity or purpose of this Imageloader. It will be returned with the onComplete callback when finished. </param>
        /// <param name="url"> The url or local path of the image. </param>
        /// <param name="lmgt"> The loader Cache Management and Loading Settings. </param>
        /// <param name="onComplete"> The callback for returning the loaded Texture2D and Index. </param>
        public void Load(uint index, string url, LoaderManagement lmgt, Action<Texture2D, uint> onComplete)
        {
            LMGT = new LoaderManagement(lmgt);
            Load(index, url, lmgt.GenerateFileName((int)index), lmgt.FolderName, lmgt.CacheMode, onComplete, lmgt.LoadingRetry, lmgt.LoadingTimeOut);
        }

        /// <summary>
        /// Start to load an image without using the cache feature (do not save/load in any local cache folder), return a Texture2D and the given index in the onComplete callback.
        /// </summary>
        /// <param name="index"> A number specified by you. Can be used to indicate the identity or purpose of this Imageloader. It will be returned with the onComplete callback when finished. </param>
        /// <param name="url"> The url or local path of the image. </param>
        /// <param name="onComplete"> The callback for returning the loaded Texture2D and Index. </param>
        /// <param name="retry"> How many time to retry if fail to load the file. </param>
        /// <param name="timeOut"> The maximum duration in seconds for waiting for a load. </param>
        public void Load(uint index, string url, Action<Texture2D, uint> onComplete, uint retry = 0, float timeOut = 30f)
        {
            Load(index, url, "", "", CacheMode.NoCache, onComplete, retry, timeOut);
        }

        /// <summary>
        /// Start to load an image using specific cache and load settings, return a Texture2D and the given index in the onComplete callback.
        /// </summary>
        /// <param name="index"> A number specified by you. Can be used to indicate the identity or purpose of this Imageloader. It will be returned with the onComplete callback when finished. </param>
        /// <param name="url"> The url or local path of the image. </param>
        /// <param name="fileName"> The filename for storing(caching) the image file. </param>
        /// <param name="folderName"> The target folder for storing(caching) the image file. </param>
        /// <param name="cacheMode"> The behavior for handling Load and Cache files. (NoCache: do not auto save the image; UseCached: use the locally cached file if exist; Replace: download and replace the locally cached file is exist)  </param>
        /// <param name="onComplete"> The callback for returning the loaded Texture2D and Index. </param>
        /// <param name="retry"> How many time to retry if fail to load the file. </param>
        /// <param name="timeOut"> The maximum duration in seconds for waiting for a load. </param>
        public void Load(uint index, string url, string fileName, string folderName, CacheMode cacheMode, Action<Texture2D, uint> onComplete = null, uint retry = 0, float timeOut = 30f)
        {
            LMGT.FolderName = folderName;
            LMGT.CacheMode = cacheMode;
            LMGT.LoadingRetry = retry;
            LMGT.LoadingTimeOut = timeOut;

            this.name += " - " + index;
            _timeOut = timeOut;
            Index = index;
            _onComplete = onComplete;
            isRetry = false;

            string fullFolderPath = "";
            if (cacheMode != CacheMode.NoCache)
            {
                fullFolderPath = Path.Combine(LMGT.CacheDirectory, folderName);

                if (string.IsNullOrEmpty(fileName))
                {
#if UNITY_EDITOR
                    if (LMGT.IsDebug) Debug.LogError("Trying to load image with cache feature enabled, but missing filename!");
#endif
                    if (m_OnImageLoadError != null) m_OnImageLoadError(ErrorType.MissingFilename, url);

                    if (onComplete != null) onComplete(null, index);
                    Destroy(gameObject);
                    return;
                }

                if (LMGT.CacheAsPerUrl && url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    fileName = MD5Util.ToMD5Hash(url);
                }

                fileName = fileName + LMGT.FileExtension;
            }

            _LoadFile(index, url, fileName, fullFolderPath, cacheMode, retry, onComplete);
        }

        private void _LoadFile(uint index, string url, string fileName, string fullFolderPath, CacheMode cacheMode, uint retry = 0, Action<Texture2D, uint> onComplete = null)
        {
            string fullFilePath = Path.Combine(fullFolderPath, fileName);

            if (cacheMode != CacheMode.NoCache && !Directory.Exists(fullFolderPath))
            {
                Directory.CreateDirectory(fullFolderPath);
            }

            bool isLoadCache = false;
            bool supportSystemIO = true;
            if (cacheMode != CacheMode.NoCache && LMGT.CacheDirectoryEnum == FilePathName.AppPath.StreamingAssetsPath && _IsAndroidOrWebGL)
            {   // Un-Support SystemIO for saving and loading files on current platform!
                supportSystemIO = false;
                isLoadCache = true;

                StartCoroutine(_LoadRoutine(index, fullFilePath, fileName, fullFolderPath, fullFilePath, true, cacheMode, retry, (texture, idx) =>
                {
                    if (texture == null)
                    {
                        StartCoroutine(_LoadRoutine(index, url, fileName, fullFolderPath, fullFilePath, false, cacheMode, retry, onComplete));
                    }
                    else
                    {
                        onComplete(texture, index);
                    }
                }));
            }

            if (supportSystemIO && cacheMode == CacheMode.UseCached)
            {   // Use cached file if exist (file with same fileName as requested).
                if (File.Exists(fullFilePath))
                {
                    isLoadCache = true;

                    if (onComplete != null)
                    {
                        byte[] data = File.ReadAllBytes(fullFilePath);
                        string mime = string.Empty;
                        string extensionName = string.Empty;
                        new FileMimeAndExtension().GetFileMimeAndExtension(data, ref mime, ref extensionName);
                        DetectedFileMime = mime;
                        DetectedFileExtension = extensionName;
                        CacheFilename = fileName;
                        IsGif = extensionName == "gif";

                        if (LMGT.LoadFileMode != LoaderManagement.LoadingMode.Default || IsGif) Data = data;

                        Texture2D texture = null;
                        if (!IsGif && LMGT.LoadFileMode != LoaderManagement.LoadingMode.LoadFileDataOnly)
                        {
                            texture = new Texture2D(1, 1);
                            if (!(data != null && texture.LoadImage(data)))
                            {
                                Destroy(texture);
                                texture = null;
                            }
                        }

                        onComplete?.Invoke(texture, index);
                        _RemoveLoading(this);
                        Destroy(gameObject);
                    }
                }
            }

            if (!isLoadCache)
            {   // Load file using WWW / UWR method
                StartCoroutine(_LoadRoutine(index, url, fileName, fullFolderPath, fullFilePath, false, cacheMode, retry, onComplete));
            }
            else if (cacheMode != CacheMode.NoCache)
            {
                ManageCachedFiles(fullFolderPath, LMGT.MaxCacheFilePerFolder, LMGT.MinTimeForKeepingFiles, LMGT.MaxTimeForKeepingFiles, LMGT.FileExtension, LMGT.IsDebug);
            }
        }

        private bool isRetry = false;
        private IEnumerator _LoadRoutine(uint index, string url, string fileName, string fullFolderPath, string fileSavePath, bool isLoadLocal, CacheMode cacheMode, uint retry = 0, Action<Texture2D, uint> onComplete = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                if (LMGT.IsDebug) Debug.LogError("Fail to load the file, URL is empty!");
                _StopDownload();
                yield break;
            }

            if (!isRetry) url = FilePathName.Instance.EnsureValidPath(url);
            if (!LMGT.AllowDuplicateDownload)
            {
                if (IsUrlLoading(url))
                {   // Duplicated, stop silently without invoking callbacks, just wait for the previous one to complete the download.
                    if (LMGT.IsDebug) Debug.LogWarning("Duplicated loading the same image at the same time. The later loading has stopped.");
                    _RemoveLoading(this);
                    Destroy(gameObject);
                    yield break;
                }
            }
            _AddLoading(this);
            URL = url;

            bool isSameFolder = false;
            //if (Equals(url, fullFolderPath)) //(url.ToLower().Contains(fullFolderPath.ToLower()))
            if (cacheMode != CacheMode.NoCache && Path.GetDirectoryName(url).Equals(Path.GetDirectoryName(fullFolderPath)))
            {   // The loading & caching folder is the same
                isSameFolder = true;
            }

            if (_timeOut > 0) Invoke("_TimeOut", _timeOut);

#if UNITY_2017_3_OR_NEWER
            using (UnityWebRequest uwr = UnityWebRequest.Get(url)) // UnityWebRequestTexture.GetTexture(url))
            {
                //if (_timeOut > 0) uwr.timeout = (int)_timeOut;
                yield return uwr.SendWebRequest();
                if (_timeOut > 0) CancelInvoke("_TimeOut");

#if UNITY_2020_1_OR_NEWER
                if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError || uwr.result == UnityWebRequest.Result.DataProcessingError)
#else
                if (uwr.isNetworkError || uwr.isHttpError)
#endif
                {
#if UNITY_EDITOR
                    if (LMGT.IsDebug) Debug.LogWarning("Failed to download file! Retry: " + retry);
#endif
                    if (retry > 0)
                    {
                        yield return new WaitForSeconds(1);
                        retry--;
                        isRetry = true;
                        StartCoroutine(_LoadRoutine(index, url, fileName, fullFolderPath, fileSavePath, isLoadLocal, cacheMode, retry, onComplete));
                    }
                    else
                    {
                        m_OnImageLoadError?.Invoke(ErrorType.NetworkError, url);
                        _RemoveLoading(this);
                        _StopDownload();
                    }
                }
                else
                {
                    byte[] data = uwr.downloadHandler.data;
                    string mime = string.Empty;
                    string extensionName = string.Empty;
                    new FileMimeAndExtension().GetFileMimeAndExtension(data, ref mime, ref extensionName);
                    DetectedFileMime = mime;
                    DetectedFileExtension = extensionName;
                    CacheFilename = fileName;
                    IsGif = extensionName == "gif";

                    if (LMGT.LoadFileMode != LoaderManagement.LoadingMode.Default || IsGif) Data = data;

                    bool textureOK = false;
                    Texture2D texture = null;
                    if (!IsGif)
                    {
                        if (LMGT.LoadFileMode == LoaderManagement.LoadingMode.LoadFileDataOnly)
                        {
                            textureOK = true; // skip creating texture and assume the image data is valid here
                        }
                        else
                        {
                            texture = new Texture2D(1, 1);
                            textureOK = data != null && texture.LoadImage(data);
                            if (!textureOK)
                            {
                                Destroy(texture);
                                texture = null;
                            }
                        }
                    }

                    if (textureOK || IsGif)
                    {
                        if (cacheMode != CacheMode.NoCache && data != null)
                        {
                            if (!isLoadLocal && !isSameFolder)
                            {
#if UNITY_WEBGL && !UNITY_EDITOR
                                SDev.EasyIO._Save(fileSavePath, data);
#else
                                File.WriteAllBytes(fileSavePath, data);
#endif
                            }
                            ManageCachedFiles(fullFolderPath, LMGT.MaxCacheFilePerFolder, LMGT.MinTimeForKeepingFiles, LMGT.MaxTimeForKeepingFiles, LMGT.FileExtension, LMGT.IsDebug);
                        }
                    }
                    else
                    {
                        m_OnImageLoadError?.Invoke(ErrorType.InvalidImageData, url);
                    }

                    onComplete?.Invoke(texture, index); //onComplete(DownloadHandlerTexture.GetContent(uwr), index);
                    _RemoveLoading(this);
                    Destroy(gameObject);
                }
            }

#else
            WWW www = new WWW(url);
            yield return www;
            if (_timeOut > 0) CancelInvoke("_TimeOut");

            if (www.error == null)
            {
                byte[] data = www.bytes;
                string mime = string.Empty;
                string extensionName = string.Empty;
                FileMimeAndExtension fme = new FileMimeAndExtension();
                fme.GetFileMimeAndExtension(data, ref mime, ref extensionName);
                DetectedFileMime = mime;
                DetecedFileExtension = extensionName;
                CacheFilename = fileName;
                IsGif = extensionName == "gif";

                if (LMGT.KeepImageData || IsGif) Data = data;

                bool textureOK = www.texture != null;
                Texture2D texture = www.texture;
                if (textureOK || IsGif)
                {
                    if (cacheMode != CacheMode.NoCache && data != null)
                    {
                        if (!isLoadLocal && !isSameFolder)
                        {
#if UNITY_WEBGL && !UNITY_EDITOR
                            SDev.EasyIO._Save(fileSavePath, data);
#else
                            File.WriteAllBytes(fileSavePath, data);
#endif
                        }
                        ManageCachedFiles(fullFolderPath, LMGT.MaxCacheFilePerFolder, LMGT.MinTimeForKeepingFiles, LMGT.MaxTimeForKeepingFiles, LMGT.FileExtension, LMGT.IsDebug);
                    }
                }
                else
                {
                    m_OnImageLoadError?.Invoke(ErrorType.InvalidImageData, url);
                }

                if (onComplete != null) onComplete(texture, index);
                _RemoveLoading(this);
                Destroy(gameObject);
            }
            else
            {
#if UNITY_EDITOR
                if (LMGT.IsDebug) Debug.LogWarning("Failed to download file! Retry: " + retry);
#endif
                if (retry > 0)
                {
                    yield return new WaitForSeconds(1);
                    retry--;
                    isRetry = true;
                    StartCoroutine(_LoadRoutine(index, url, fileName, fullFolderPath, fileSavePath, isLoadLocal, cacheMode, retry, onComplete));
                }
                else
                {
                    if (m_OnImageLoadError != null) m_OnImageLoadError(ErrorType.NetworkError, url);
                    if (onComplete != null) onComplete(null, index);
                    _RemoveLoading(this);
                    _StopDownload();
                }
            }

            www.Dispose();
            www = null;
#endif
        }

        private void OnDisable()
        {
            _stopped = true;
        }

        private void _TimeOut()
        {
            if (m_OnImageLoadError != null) m_OnImageLoadError(ErrorType.TimeOut, URL);
            _StopDownload();
        }

        private bool _stopped;
        private void _StopDownload()
        {
            if (_stopped) return;

            if (_onComplete != null)
            {
                _onComplete(null, Index);
            }
            Destroy(gameObject);
        }

        private bool _IsAndroidOrWebGL
        {
            get
            {
                bool flag = false;
#if UNITY_ANDROID || UNITY_WEBGL
                flag = true;
#endif
                return flag;
            }
        }


        #region ----- Static Methods -----
        /// <summary>
        /// A simple System.IO method for loading jpg/png from local path directly (e.g. Application.persistentDataPath/.., does not support streamingAssetsPath on some platforms). 
        /// * No platform checking and cache management.
        /// </summary>
        public static Texture2D GetImageByPath(string fullFilePath)
        {
            if (File.Exists(fullFilePath))
            {
                Texture2D getTexture = new Texture2D(1, 1);
                if (getTexture.LoadImage(File.ReadAllBytes(fullFilePath)))
                {
                    return getTexture;
                }
                else
                {
                    Destroy(getTexture);
                }
            }
            return null;
        }

        public static Texture2D CreateTextureByData(byte[] imageData)
        {
            if (imageData == null) return null;

            Texture2D getTexture = new Texture2D(1, 1);
            if (getTexture.LoadImage(imageData))
            {
                return getTexture;
            }
            else
            {
                Destroy(getTexture);
                return null;
            }
        }

        public static void DeleteFileByPath(string fullFilePath)
        {
            if (File.Exists(fullFilePath))
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                SDev.EasyIO._Delete(fullFilePath);
#else
                File.Delete(fullFilePath);
#endif
            }
        }

        /// <summary> Delete files in the target folder. Optional to specify the file type to delete, e.g: .jpg, .png </summary>
        /// <param name="fullFolderPath"> The complete path to the target local folder, which is the folder for storing your images. </param>
        /// <param name="fileExtension"> File type, e.g: .jpg, .png (make sure this matches the file type that used to save your files). Null or empty means all types. </param>
        public static void ClearFilesByFolder(string fullFolderPath, string fileExtension = null)
        {
            if (!Directory.Exists(fullFolderPath)) return;

            string[] allFilePaths = null;
            if (string.IsNullOrEmpty(fileExtension))
            {
                allFilePaths = Directory.GetFiles(fullFolderPath);
            }
            else
            {
                allFilePaths = Directory.GetFiles(fullFolderPath).Where(file => Path.GetExtension(file).ToLower() == fileExtension.ToLower()).ToArray();
            }

            if (allFilePaths != null && allFilePaths.Length > 0)
            {
                for (int i = 0; i < allFilePaths.Length; i++)
                {
#if UNITY_WEBGL && !UNITY_EDITOR
                    SDev.EasyIO._Delete(allFilePaths[i]);
#else
                    File.Delete(allFilePaths[i]);
#endif
                }
            }
        }

        /// <summary>
        /// Manage files in the target cache folder. Delete files that meet the provided conditions, oldest file first delete : 
        /// (1) Delete files that exist longer than minKeepTime(sec) since its LastWriteTime, when stored files exceed the keepFileNum limit;
        /// (2) Delete files that exist longer than maxKeepTime(sec) since its LastWriteTime, even stored files Not exceed the keepFileNum limit.
        /// </summary>
        /// <param name="fullFolderPath"> The complete path to the target local folder, which is the folder for storing your images. </param>
        /// <param name="keepFileNum"> File limit for the target cache folder. (Zero means infinite) </param>
        /// <param name="minKeepTime"> Time duration in seconds, files should keep in the target cache folder for at least this duration. (Zero means infinite) </param>
        /// <param name="maxKeepTime"> Time duration in seconds, files should not keep in the target cache folder for more than this duration. (Zero means infinite) </param>
        /// <param name="fileExtension"> File type, e.g: .jpg, .png (make sure this matches the file type that used to save your files). Null or empty means all types. </param>
        public static void ManageCachedFiles(string fullFolderPath, uint keepFileNum, uint minKeepTime, uint maxKeepTime, string fileExtension = null, bool isDebug = false)
        {
            if (!Directory.Exists(fullFolderPath)) return;

            string[] allFilePaths = null;
            if (string.IsNullOrEmpty(fileExtension))
            {
                allFilePaths = Directory.GetFiles(fullFolderPath);
            }
            else
            {
                allFilePaths = Directory.GetFiles(fullFolderPath).Where(file => Path.GetExtension(file).ToLower() == fileExtension.ToLower()).ToArray();
            }

            List<FileInfo> fileInfoList = new List<FileInfo>();
            if (allFilePaths != null && allFilePaths.Length > 0)
            {
                for (int i = 0; i < allFilePaths.Length; i++)
                {
                    fileInfoList.Add(new FileInfo(allFilePaths[i]));
                }
                fileInfoList.Sort(delegate (FileInfo a, FileInfo b)
                {   // Sort ascending by last write time, so the oldest files in front.
                    if (a.LastWriteTime == b.LastWriteTime) return 0;
                    else if (a.LastWriteTime < b.LastWriteTime) return -1;
                    else return 1;
                });
            }
            else return;

            if (keepFileNum > 0 && fileInfoList.Count >= keepFileNum)
            {
                int exceedNum = fileInfoList.Count - (int)keepFileNum;  // Set the maximum number of files can be deleted.
                if (exceedNum > 0)
                {
                    DateTime timeLimit = DateTime.Now.AddSeconds(-minKeepTime);
                    for (int i = 0; i < exceedNum; i++)
                    {
                        if (i < fileInfoList.Count && fileInfoList[i] != null)
                        {
                            if (minKeepTime <= 0 || fileInfoList[i].LastWriteTime < timeLimit) // Ensure files do not being deleted before expire.
                            {
#if UNITY_EDITOR
                                if (isDebug) Debug.Log(i + " - Delete (Exceed File Limit per folder): Exceed = " + exceedNum + " - FileName & LastWriteTime : " + fileInfoList[i].Name + " - " + fileInfoList[i].LastWriteTime);
#endif
#if UNITY_WEBGL && !UNITY_EDITOR
                                SDev.EasyIO._Delete(fileInfoList[i].FullName);
#else
                                File.Delete(fileInfoList[i].FullName);
#endif
                            }
                        }
                    }
                }
            }

            if (maxKeepTime > 0 && maxKeepTime >= minKeepTime && fileInfoList.Count > 0)
            {
                DateTime timeLimit = DateTime.Now.AddSeconds(-maxKeepTime);
                List<FileInfo> hardExpiredFileList = fileInfoList.FindAll(delegate (FileInfo fi) { return fi.LastWriteTime < timeLimit; });
                for (int i = 0; i < hardExpiredFileList.Count; i++) // Delete hard expired files (Files those must delete)
                {
#if UNITY_EDITOR
                    if (isDebug) Debug.Log(i + " - Delete (Hard Expired Files): Expired Count = " + hardExpiredFileList.Count + " - FileName & LastWriteTime : " + hardExpiredFileList[i].Name + " - " + hardExpiredFileList[i].LastWriteTime);
#endif
#if UNITY_WEBGL && !UNITY_EDITOR
                    SDev.EasyIO._Delete(hardExpiredFileList[i].FullName);
#else
                    File.Delete(hardExpiredFileList[i].FullName);
#endif
                }
            }
        }
#endregion
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ImageLoader))]
    public class ImageLoaderCustomEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            ImageLoader mono = (ImageLoader)target;
            GUILayout.Space(20);

            if (GUILayout.Button("Show Cache Directory (Editor)"))
            {
                string directory = mono.LMGT.CacheDirectory;
                if (string.IsNullOrEmpty(directory)) return;
                if (Directory.Exists(directory))
                    EditorUtility.RevealInFinder(directory);
                else
                    Debug.LogWarning("Directory not exist: " + directory);
            }

            if (GUILayout.Button("Copy Cache Directory (Editor)"))
            {
                string directory = mono.LMGT.CacheDirectory;
                if (string.IsNullOrEmpty(directory)) return;
                TextEditor te = null;
                te = new TextEditor();
                te.text = directory;
                te.SelectAll();
                te.Copy();
                Debug.Log("Copied: " + directory);
            }
        }
    }
#endif
}
