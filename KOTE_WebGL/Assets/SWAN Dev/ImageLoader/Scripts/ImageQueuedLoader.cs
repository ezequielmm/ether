// Created By SwanDEV 2019
using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// ImageBox namespace.
namespace IMBX
{
    public class ImageQueuedLoader : MonoBehaviour
    {
        [Tooltip("Destroy this GameObject when all loading tasks finished.")]
        public bool m_DestroyOnComplete;

        [Tooltip("Clear the old LoadingTask list when the loader kick started from Idle state.")]
        public bool m_AutoClearLoadHistory = true;

        [Tooltip("Max number of ImageLoaders to use concurrently.")]
        public uint m_MaxLoaderNum = 3;

        [Tooltip("Max number of loading tasks allowed to queue in the URL/path(task) list. If the tasks exceeded this limit, the older waiting tasks will be removed. 0 = no limit (default).")]
        public uint m_MaxQueueSize;

        [Tooltip("The loader Cache Management and Loading Settings.")]
        public LoaderManagement LMGT = new LoaderManagement();

        [Space]
        [Tooltip("Recent image loading tasks.")]
        public List<ImageLoader.LoadingTask> m_LoadingTasks = new List<ImageLoader.LoadingTask>();

        /// <summary>
        /// A callback to fire when each image URL load is finished. (Register this callback manually if needed)
        /// </summary>
        public Action<ImageBatchLoader.Result> m_OnImageLoaded;

        /// <summary>
        /// A callback to fire when all the image URL loading tasks are finished. (Register this callback manually if needed)
        /// </summary>
        public Action m_OnAllImagesLoaded;

        /// <summary>
        /// A callback to fire when failing to load an image, returns the ErrorType and URL(or message) in the callback. (Register this callback manually if needed)
        /// </summary>
        public Action<ImageLoader.ErrorType, string> m_OnImageLoadError;

        private Action<ImageBatchLoader.Result> _onProgressCallback = null;
        private Action<ImageBatchLoader.Results> _onCompleteCallback = null;

        private List<ImageLoader> _loaders = new List<ImageLoader>();
        private ImageBatchLoader.Results _results = new ImageBatchLoader.Results();
        private uint _currentIndex = 0;
        private float _progress = 0f;
        private int _finishCount = 0;

        /// <summary>
        /// Indicates if the last Url/task is executed (not necessarily finished), which no image URL waiting to load.
        /// </summary>
        public bool EndOfUrl { get { return _currentIndex >= m_LoadingTasks.Count; } }

        /// <summary>
        /// Indicates if all Urls/tasks are loaded(and finished). If 'true', means this loader is Idle.
        /// </summary>
        public bool Idle { get; private set; }

        /// <summary>
        /// Create an ImageQueuedLoader to load images with a specific number of ImageLoaders.
        /// </summary>
        /// <param name="maxLoaderNum"> Max number of ImageLoaders to use concurrently. (Requires at least 1 to load the images) </param>
        /// <param name="maxQueueSize"> Max number of loading tasks allowed to queue in the URL/path(task) list.
        /// If the tasks exceeded this limit, the oldest waiting tasks will be removed. 0 = no limit (default). </param>
        public static ImageQueuedLoader Create(uint maxLoaderNum, uint maxQueueSize = 0)
        {
            ImageQueuedLoader loader = new GameObject("[ImageQueuedLoader]").AddComponent<ImageQueuedLoader>();
            loader.m_MaxLoaderNum = maxLoaderNum;
            loader.m_MaxQueueSize = maxQueueSize;
            return loader;
        }

        /// <summary>
        /// Delete all files in the cache folder, i.e. LMGT.CacheFolderPath.
        /// </summary>
        public void ClearStorageCache()
        {
            ImageLoader.ClearFilesByFolder(LMGT.CacheFolderPath, fileExtension: null);
        }

        public void DontDestroyOnLoad()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            int total = m_LoadingTasks.Count;
            if (_currentIndex >= total)
            {   // No more Url/Path.
                if (m_DestroyOnComplete)
                {
                    _CleanLoaderList();
                    if (_loaders.Count == 0) Destroy(gameObject);
                }
                return;
            }

            if (m_MaxQueueSize > 0 && (total - (int)_currentIndex) > (int)m_MaxQueueSize)
            {
                _currentIndex = (uint)total - m_MaxQueueSize;
            }

            _CleanLoaderList();
            if (_loaders.Count < m_MaxLoaderNum)
            {
                ImageLoader loader = ImageLoader.Create();
                loader.m_OnImageLoadError = m_OnImageLoadError;
                _loaders.Add(loader);

                bool cacheAsPerUrl = LMGT.CacheAsPerUrl;
                uint fileIndexFormatDigitsCount = LMGT.FileIndexFormatDigitsCount;
                string fileNamePrefix = LMGT.FileNamePrefix;
                ImageLoader.CacheMode cacheMode = LMGT.CacheMode;

                ImageLoader.LoadingTask currTask = m_LoadingTasks[(int)_currentIndex];
                if (!string.IsNullOrEmpty(currTask.customFilename))
                {
                    LMGT.CacheAsPerUrl = false;
                    LMGT.FileNamePrefix = currTask.customFilename;
                    LMGT.FileIndexFormatDigitsCount = 0;
                }
                LMGT.CacheMode = currTask.cacheMode;

                _currentIndex++;

                currTask._executed = true;
                loader.Load((uint)currTask.customIndex, currTask.imageUrl, LMGT, (texture, index) =>
                {
                    _finishCount++;
                    _progress = (float)_finishCount / total;

                    bool fileDataReserved = loader.FileDataReserved;

                    // Reminded: If the texture cannot be loaded, it will return a null. So check null before use it.
                    ImageBatchLoader.Result result = new ImageBatchLoader.Result(loader.Data, texture, index, currTask.imageUrl, _progress,
                        loader.DetectedFileMime, loader.DetectedFileExtension, loader.CacheFilename, fileDataReserved);

                    if (_onProgressCallback != null) _onProgressCallback(result);

                    if (m_OnImageLoaded != null && (texture || fileDataReserved)) m_OnImageLoaded(result);

                    // Ensure using the corrected result.m_Index if this is created by ImageBatchLoader with memory cache enabled
                    if (_onCompleteCallback != null) _results.SetResult(result.m_Index, result, 1);

                    _CleanLoaderList();

                    if (_loaders.Count == 1 && EndOfUrl) // On Complete, this is the last loader and the index reached the end of URL list
                    {
                        if (_onCompleteCallback != null)
                        {
                            _results.OrderByIndex();
                            _onCompleteCallback(_results);
                        }

                        if (m_OnAllImagesLoaded != null) m_OnAllImagesLoaded();

                        Idle = true;
                    }
                });

                LMGT.FileNamePrefix = fileNamePrefix;
                LMGT.FileIndexFormatDigitsCount = fileIndexFormatDigitsCount;
                LMGT.CacheAsPerUrl = cacheAsPerUrl;
                LMGT.CacheMode = cacheMode;
            }
        }

        /// <summary>
        /// Remove null references in the loader list.
        /// </summary>
        private void _CleanLoaderList()
        {
            for (int i = 0; i < _loaders.Count; i++)
            {
                if (_loaders[i] == null) _loaders.RemoveAt(i);
            }
        }

        private void _Init()
        {
            for (int i = 0; i < _loaders.Count; i++)
            {
                if (_loaders[i])
                {
                    _loaders[i].Cancel(); //Destroy(_loaders[i].gameObject);
                }
            }
            _loaders = new List<ImageLoader>();

            _results = new ImageBatchLoader.Results();

            m_LoadingTasks = new List<ImageLoader.LoadingTask>();

            _currentIndex = 0;
            _finishCount = 0;

            _nextAutoClearTime = Time.time + 3f;
        }
        private float _nextAutoClearTime;

        /// <summary>
        /// Set this Q-Loader to initial state, which stop all the loading and pending tasks, and empty the LoadingTask list.
        /// (Suggest to use with the Add method, no need to call this method if you are using Load methods)
        /// </summary>
        /// <param name="onComplete"> The callback for receiving all the loaded results. (This will replace the existing callback, if any) </param>
        /// <param name="onProgress"> Update the loading progress and receive the latest loaded texture. (This will replace the existing callback, if any) </param>
        /// <param name="lmgt"> Optional to provide a new LoaderManagement object to replace the existing load/cache settings. </param>
        public void Init(Action<ImageBatchLoader.Results> onComplete, Action<ImageBatchLoader.Result> onProgress = null, LoaderManagement lmgt = null)
        {
            if (lmgt != null) LMGT = new LoaderManagement(lmgt);

            _Init();
            _onCompleteCallback = onComplete;
            _onProgressCallback = onProgress;
        }

        /// <summary>
        /// Add a LoadingTask to the loading list.
        /// </summary>
        public void Add(ImageLoader.LoadingTask loadingTask)
        {
            if (string.IsNullOrEmpty(loadingTask.imageUrl)) return;

            if (m_LoadingTasks == null) m_LoadingTasks = new List<ImageLoader.LoadingTask>();

            if (Idle && m_AutoClearLoadHistory && Time.time > _nextAutoClearTime)
            {
                _Init();
            }

            if (!LMGT.AllowDuplicateDownload)
            {
                // Check if imageUrl already in pendings
                int loadObjCnt = m_LoadingTasks.Count;
                for (int i = (int)_currentIndex; i < loadObjCnt; i++)
                {
                    if (loadingTask.imageUrl == m_LoadingTasks[i].imageUrl)
                    {
                        //if (LMGT.IsDebug) Debug.Log("Skip adding URL, already in pendings: " + imageUrl);
                        return; // drop this imageUrl
                    }
                }

                // Check if imageUrl being loaded
                for (int i = 0; i < _loaders.Count; i++)
                {
                    if (_loaders[i] && _loaders[i].isActiveAndEnabled && _loaders[i].URL == loadingTask.imageUrl)
                    {
                        //if (LMGT.IsDebug) Debug.Log("Skip! This URL is being loaded: " + imageUrl);
                        return; // drop this imageUrl
                    }
                }
            }

            m_LoadingTasks.Add(loadingTask);
        }

        /// <summary>
        /// Add an image URL or local path to the loading list. Optional to provide a specific filename.
        /// </summary>
        /// <param name="imageUrl"> Image URL or local path. </param>
        /// <param name="customFilename"> (Optional) Specify a custom filename(without extension) for caching/saving the downloaded image in the loacal cache folder. </param>
        /// <param name="customIndex"> (Optional) The index number can be assigned before loading multiple images, for sorting the results when needed.
        /// If this is provided (larger than -1), this index will be set to the index of its Result object, which can be accessed in the callbacks. </param>
        public void Add(string imageUrl, string customFilename = null, int customIndex = -1)
        {
            Add(imageUrl, LMGT.CacheMode, customFilename, customIndex);
        }

        /// <summary>
        /// Add an image URL or local path to the loading list, and specify the cache mode for this image. Optional to provide a specific filename and index.
        /// </summary>
        /// <param name="imageUrl"> Image URL or local path. </param>
        /// <param name="cacheMode"> Specific cache mode for this image. </param>
        /// <param name="customFilename"> (Optional) Specify a custom filename(without extension) for caching/saving the downloaded image in the loacal cache folder. </param>
        /// <param name="customIndex"> (Optional) The index number can be assigned before loading multiple images, for sorting the results when needed.
        /// If this is provided (larger than -1), this index will be set to the index of its Result object, which can be accessed in the callbacks. </param>
        public void Add(string imageUrl, ImageLoader.CacheMode cacheMode, string customFilename = null, int customIndex = -1)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            ImageLoader.LoadingTask loadingTask = new ImageLoader.LoadingTask()
            {
                imageUrl = imageUrl,
                cacheMode = cacheMode,
                customFilename = customFilename,
                customIndex = customIndex,
            };
            Add(loadingTask);
        }

        /// <summary>
        /// Add multiple image URLs or local paths to the loading list.
        /// </summary>
        /// <param name="imageUrls"> A list of image URL. </param>
        private void _AddMultiple(List<string> imageUrls)
        {
            if (imageUrls != null)
            {
                for (int i = 0; i < imageUrls.Count; i++)
                {
                    Add(imageUrls[i], LMGT.CacheMode, null, i);
                }
            }
        }

        /// <summary>
        /// Add multiple LoadingTasks to the queue to load the images based on their own settings in the LoadingTask.
        /// </summary>
        public void Add(List<ImageLoader.LoadingTask> loadingTasks)
        {
            if (loadingTasks != null)
            {
                for (int i = 0; i < loadingTasks.Count; i++)
                {
                    Add(loadingTasks[i]);
                }
            }
        }

        /// <summary>
        /// Load multiple LoadingTasks using their own settings, return the Result objects in the onComplete and onProgress callbacks.
        /// </summary>
        /// <param name="loadingTasks"> LoadingTasks containing the image URL and load settings. </param>
        /// <param name="onComplete"> The callback for receiving all the loaded results. </param>
        /// <param name="onProgress"> Update the loading progress and receive the latest loaded texture. </param>
        public void Load(List<ImageLoader.LoadingTask> loadingTasks, Action<ImageBatchLoader.Results> onComplete, Action<ImageBatchLoader.Result> onProgress = null)
        {
            _Init();
            Add(loadingTasks);
            _onCompleteCallback = onComplete;
            _onProgressCallback = onProgress;
        }

        /// <summary>
        /// Load multiple images using the settings of the provided LoaderManagement object, return the Result objects in the onComplete and onProgress callbacks.
        /// </summary>
        /// <param name="imageUrls"> Image urls or local paths. </param>
        /// <param name="lmgt"> The loader Cache Management and Loading Settings. </param>
        /// <param name="onComplete"> The callback for receiving all the loaded results. </param>
        /// <param name="onProgress"> Update the loading progress and receive the latest loaded texture. </param>
        public void Load(List<string> imageUrls, LoaderManagement lmgt, Action<ImageBatchLoader.Results> onComplete, Action<ImageBatchLoader.Result> onProgress = null)
        {
            LMGT = new LoaderManagement(lmgt);

            _Init();
            _AddMultiple(imageUrls);
            _onCompleteCallback = onComplete;
            _onProgressCallback = onProgress;
        }

        /// <summary>
        /// Load multiple images without using the cache feature (do not save/load in any local cache folder), return the Result objects in the onComplete and onProgress callbacks.
        /// </summary>
        /// <param name="imageUrls"> Image urls or local paths. </param>
        /// <param name="onComplete"> The callback for receiving all the loaded results. </param>
        /// <param name="onProgress"> Update the loading progress and receive the latest loaded texture. </param>
        /// <param name="retry"> The retry number for each image. </param>
        /// <param name="timeOut"> The timout for each image. </param>
        public void Load(List<string> imageUrls, Action<ImageBatchLoader.Results> onComplete, Action<ImageBatchLoader.Result> onProgress = null, uint retry = 0, float timeOut = 30f)
        {
            LMGT.LoadingRetry = retry;
            LMGT.LoadingTimeOut = timeOut;
            LMGT.CacheMode = ImageLoader.CacheMode.NoCache;

            _Init();
            _AddMultiple(imageUrls);
            _onCompleteCallback = onComplete;
            _onProgressCallback = onProgress;
        }

        /// <summary>
        /// Load multiple images using specific cache and load settings, return the Result objects in the onComplete and onProgress callbacks.
        /// </summary>
        /// <param name="imageUrls"> Image urls or local paths. </param>
        /// <param name="filenamePrefix"> The filename prefix for storing(caching) the image files. The final filename will be combined by this prefix and the index together. </param>
        /// <param name="folderName"> The target folder for storing(caching) the image file. </param>
        /// <param name="cacheMode"> The behavior for handling Load and Cache files. (NoCache: do not auto save the image; UseCached: use the locally cached file if exist; Replace: download and replace the locally cached file is exist) </param>
        /// <param name="onComplete"> The callback for receiving all the loaded results. </param>
        /// <param name="onProgress"> Update the loading progress and receive the latest loaded texture. </param>
        /// <param name="retry"> The retry number for each image. </param>
        /// <param name="timeOut"> The timout for each image. </param>
        public void Load(List<string> imageUrls, string filenamePrefix, string folderName, ImageLoader.CacheMode cacheMode = ImageLoader.CacheMode.Replace,
            Action<ImageBatchLoader.Results> onComplete = null, Action<ImageBatchLoader.Result> onProgress = null, uint retry = 0, float timeOut = 30f)
        {
            LMGT.FileNamePrefix = filenamePrefix;
            LMGT.FolderName = folderName;
            LMGT.CacheMode = cacheMode;
            LMGT.LoadingRetry = retry;
            LMGT.LoadingTimeOut = timeOut;

            _Init();
            _AddMultiple(imageUrls);
            _onCompleteCallback = onComplete;
            _onProgressCallback = onProgress;
        }

        /// <summary>
        /// Cancel a loading task by image Url/Path. (Both pending task and in-complete loader with the provided imageUrl will be cleared)
        /// </summary>
        public void CancelByUrl(string imageUrl)
        {
            for (int i = 0; i < _loaders.Count; i++)
            {
                if (imageUrl == _loaders[i].URL)
                {
                    _loaders[i].Cancel();
                    _loaders.RemoveAt(i);
                }
            }

            for (int i = 0; i < m_LoadingTasks.Count; i++)
            {
                if (imageUrl == m_LoadingTasks[i].imageUrl)
                {
                    m_LoadingTasks.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Cancel a loading task by the custom index that previously given by youself. (Both pending task and in-complete loader with the provided Index will be cleared)
        /// </summary>
        /// <param name="index"></param>
        public void CancelByIndex(int index)
        {
            for (int i = 0; i < _loaders.Count; i++)
            {
                if (index == _loaders[i].Index)
                {
                    _loaders[i].Cancel();
                    _loaders.RemoveAt(i);
                }
            }

            for (int i = 0; i < m_LoadingTasks.Count; i++)
            {
                if (index == m_LoadingTasks[i].customIndex)
                {
                    m_LoadingTasks.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Cancel the current running loaders, except those loaders that URL/path contained in the exceptURLs list.
        /// </summary>
        public void CancelAllLoading(List<string> exceptURLs = null)
        {
            _CleanLoaderList();
            int exceptCount = exceptURLs == null ? 0 : exceptURLs.Count;
            for (int i = 0; i < _loaders.Count; i++)
            {
                if (exceptCount == 0 || !exceptURLs.Contains(_loaders[i].URL))
                {
                    _loaders[i].Cancel();
                    _loaders.RemoveAt(i);
                }
            }
            _loaders.TrimExcess();
        }

        /// <summary>
        /// Cancel all the pending tasks in this loader.
        /// (The loading tasks were queued on the waiting list as the total tasks more than the maximum Loader limit)
        /// </summary>
        public void CancelAllPending(List<string> exceptURLs = null)
        {
            int beforeUrlObjCnt = m_LoadingTasks == null ? 0 : m_LoadingTasks.Count;

            if (beforeUrlObjCnt > 0)
            {
                int exceptCount = exceptURLs == null ? 0 : exceptURLs.Count;
                bool clearAllPendings = exceptCount == 0;
                for (int i = 0; i < m_LoadingTasks.Count; i++)
                {
                    if (!m_LoadingTasks[i]._executed && (clearAllPendings || !exceptURLs.Contains(m_LoadingTasks[i].imageUrl)))
                    {
                        m_LoadingTasks.RemoveAt(i);
                    }
                }
                m_LoadingTasks.TrimExcess();

#if UNITY_EDITOR
                if (LMGT.IsDebug) Debug.Log("Pending Urls Cleared. Current LoadingTask count: " + m_LoadingTasks.Count + ", before: " + beforeUrlObjCnt);
#endif
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ImageQueuedLoader))]
    public class ImageQueuedLoaderCustomEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            ImageQueuedLoader mono = (ImageQueuedLoader)target;
            GUILayout.Space(20);

            if (GUILayout.Button("Show Cache Directory (Editor)"))
            {
                string directory = mono.LMGT.CacheDirectory;
                if (string.IsNullOrEmpty(directory)) return;
                if (System.IO.Directory.Exists(directory))
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
