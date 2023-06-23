// Created by SwanDEV 2018
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ImageBox namespace.
namespace IMBX
{
    /// <summary>
    /// Load multiple images from web or local, and/or save to the local storage.
    /// </summary>
    public class ImageBatchLoader
    {
        public class Results
        {
            /// <summary>
            /// A dictionary contains all the loaded Results and textures.
            /// </summary>
            public Dictionary<int, Result> ResultDict = new Dictionary<int, Result>();

            public bool SetResult(uint index, Result result, int code = 0)
            {
                if (ResultDict.ContainsKey((int)index))
                {
                    //Debug.LogWarning("An item with the same key has already been added. Key: " + index + ", code: " + code);
                    return false;
                }
                else
                {
                    ResultDict.Add((int)index, result);
                    return true;
                }
            }

            public void OrderByIndex()
            {
                var ordered = ResultDict.OrderBy(item => item.Key);
                ResultDict = ordered.ToDictionary((k) => k.Key, (v) => v.Value);
            }

            public void ClearTextures()
            {
                var list = GetResultList();
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] != null && list[i].m_Texture != null)
                        UnityEngine.Object.Destroy(list[i].m_Texture);
                }
            }

            /// <summary>
            /// The total number of loaded image Result objects.
            /// </summary>
            public int Count { get { return ResultDict.Count; } }

            /// <summary>
            /// Get all loaded results in a list. (Reminded to check null)
            /// </summary>
            public List<Result> GetResultList()
            {
                if (_resultList == null)
                {
                    _resultList = ResultDict.Values.ToList();
                }
                return _resultList;
            }
            private List<Result> _resultList;

            /// <summary>
            /// Get all textures in a list. (Reminded to check null)
            /// </summary>
            public List<Texture2D> GetTextureList()
            {
                if (_textureList == null)
                {
                    _textureList = new List<Texture2D>();
                    for (int i = 0; i < ResultDict.Count; i++)
                    {
                        _textureList.Add(ResultDict[i].m_Texture);
                    }
                }
                return _textureList;
            }
            private List<Texture2D> _textureList;

            /// <summary>
            /// Get a loaded result object by its URL/path that is used to load the image. (Reminded to check null)
            /// </summary>
            public Result GetResultByUrl(string imageUrl)
            {
                List<Result> list = GetResultList();
                if (list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i].m_URL == imageUrl) return list[i];
                    }
                }
                return null;
            }

            /// <summary>
            /// Get a loaded result object by index. (Reminded to check null)
            /// </summary>
            public Result GetResultByIndex(int index)
            {
                if (ResultDict.TryGetValue(index, out Result result)) return result; else return null;
            }

            /// <summary>
            /// Get a texture by index. (Reminded to check null)
            /// </summary>
            public Texture2D GetTexture(int index)
            {
                if (ResultDict.TryGetValue(index, out Result result)) return result.m_Texture; else return null;
            }

            /// <summary>
            /// Get MIME type of a particular image by index.
            /// </summary>
            public string GetMimeType(int index)
            {
                if (ResultDict.TryGetValue(index, out Result result)) return result.m_DetectedFileMime; else return null;
            }

            /// <summary>
            /// Get file extension name of a particular image by index.
            /// </summary>
            public string GetExtensionName(int index)
            {
                if (ResultDict.TryGetValue(index, out Result result)) return result.m_DetectedFileExtension; else return null;
            }
        }

        public class Result
        {
            private Texture2D _texture;
            /// <summary>
            /// Loaded texture. Null if the image does not exist or there are network issues. (Check Null before using)
            /// </summary>
            public Texture2D m_Texture
            {
                get { return _texture; }
                set
                {
                    _texture = value;
                    if (_cacheItemRef != null) _cacheItemRef.m_Texture = value;
                }
            }
            /// <summary>
            /// If Memory Cache is enabled, this is the CacheItem reference for the same image. Null, if Memory Cache is not enabled.
            /// </summary>
            public CacheItem _cacheItemRef;

            /// <summary>
            /// The index of the image, the same as in the image URL/path list.
            /// </summary>
            public uint m_Index;

            /// <summary>
            /// The URL/path that is used to load the image.
            /// </summary>
            public string m_URL;

            /// <summary>
            /// The loading progress when this load is done.
            /// </summary>
            public float m_Progress;

            /// <summary>
            /// Indicates if the image file data is kept for this image.
            /// </summary>
            public bool m_FileDataReserved;
            /// <summary>
            /// The image byte array. (To access the image data, please set LMGT.LoadFileMode to the appropriate mode you want)
            /// </summary>
            public byte[] m_Data;

            public string m_DetectedFileMime;
            public string m_DetectedFileExtension;
            public string m_CacheFilename;

            public Result(byte[] data, Texture2D texture, uint index, string imageUrl, float progress, string mime, string extension, string cacheFilename, bool fileDataReserved)
            {
                m_Data = data;
                m_Texture = texture;
                m_Index = index;
                m_URL = imageUrl;
                m_Progress = progress;
                m_DetectedFileMime = mime;
                m_DetectedFileExtension = extension;
                m_CacheFilename = cacheFilename;
                m_FileDataReserved = fileDataReserved;
            }
        }

        /// <summary>
        /// The loader Cache Management and Loading Settings.
        /// </summary>
        public LoaderManagement LMGT = new LoaderManagement();

        /// <summary>
        /// A callback to fire when each image URL load is finished. (Register this callback manually if needed)
        /// </summary>
        public Action<Result> m_OnImageLoaded;

        /// <summary>
        /// A callback to fire when all the image URL loading tasks are finished. (Register this callback manually if needed)
        /// </summary>
        public Action m_OnAllImagesLoaded;

        /// <summary>
        /// A callback to fire when failing to load an image, returns the ErrorType and URL(or message) in the callback. (Register this callback manually if needed)
        /// </summary>
        public Action<ImageLoader.ErrorType, string> m_OnImageLoadError;

        /// <summary>
        /// The loading progress.
        /// </summary>
        private float _progress = 0f;

        private int _ID; // This loader ID

        /// <summary>
        /// ImageBatchLoader constructor.
        /// </summary>
        /// <param name="maxCacheFilePerFolder"> The maximum number of files can be stored(Cached) in the cache folder. ( 0 means no limit ) </param>
        /// <param name="cacheDirectoryEnum"> Local storage directory (in-app path) for caching images if cache feature is enabled. </param>
        public ImageBatchLoader(uint maxCacheFilePerFolder = 0, FilePathName.AppPath cacheDirectoryEnum = FilePathName.AppPath.PersistentDataPath)
        {
            _ID = ImageLoader.NextID;
            LMGT.CacheDirectoryEnum = cacheDirectoryEnum;
            LMGT.MaxCacheFilePerFolder = maxCacheFilePerFolder;
        }

        /// <summary>
        /// Cancel all the current (downloading) loader instances in this ImageBatchLoader. i.e. Stop/Destroy ImageLoader instances that started but are not yet finished.
        /// </summary>
        /// <param name="exceptLocked"> If 'true', do not cancel those image URLs that are marked as locked. (This parameter applies to images in the Memory Cache only) </param>
        public void CancelAllLoading(bool exceptLocked = false)
        {
            if (MemoryCacheEnabled) MemoryCache.CancelAllLoading(exceptLocked);
            if (_listQueuedLoader) _listQueuedLoader.CancelAllLoading();
            ImageLoader._CancelLoadingsByParentLoaderID(_ID);
        }

        /// <summary>
        /// Cancel all the pending tasks in this ImageBatchLoader.
        /// (The loading tasks were queued on the waiting list as the total tasks more than the maximum Loader limit)
        /// </summary>
        /// <param name="exceptLocked"> If 'true', do not cancel those image URLs that are marked as locked. (This parameter applies to images in the Memory Cache only) </param>
        public void CancelAllPending(bool exceptLocked = false)
        {
            if (MemoryCacheEnabled) MemoryCache.CancelAllPending(exceptLocked);
            if (_listQueuedLoader) _listQueuedLoader.CancelAllPending();
        }

        /// <summary>
        /// Delete all files in the cache folder, i.e. LMGT.CacheFolderPath.
        /// </summary>
        public void ClearStorageCache()
        {
            ImageLoader.ClearFilesByFolder(LMGT.CacheFolderPath, fileExtension: null);
        }


        /// <summary>
        /// Cache the result and handle the onProgress callback.
        /// </summary>
        private void _OnProgressAndMemoryCaching(Result result, Action<Result> onProgress = null)
        {
            if (MemoryCacheEnabled) MemoryCache._TryAddToMemoryCache(result);
            if (onProgress != null) onProgress(result);
        }

        #region ----- Load -----
        /// <summary>
        /// Load multiple images with the settings of the provided LoaderManagement object, return the Result objects in the onComplete and onProgress callbacks.
        /// </summary>
        /// <param name="imageUrls"> Image urls or local paths. </param>
        /// <param name="lmgt"> The loader Cache Management and Loading Settings. </param>
        /// <param name="onComplete"> The callback for receiving all the loaded results. </param>
        /// <param name="onProgress"> Update the loading progress and receive the latest loaded texture. </param>
        public void Load(List<string> imageUrls, LoaderManagement lmgt, Action<Results> onComplete, Action<Result> onProgress = null)
        {
            LMGT = new LoaderManagement(lmgt);
            _Load(imageUrls, onComplete, onProgress);
        }

        /// <summary>
        /// Load multiple images without using the cache feature (do not save/load in any local cache folder),
        /// return the Result objects in the onComplete and onProgress callbacks.
        /// </summary>
        /// <param name="imageUrls"> Image urls or local paths. </param>
        /// <param name="onComplete"> The callback for receiving all the loaded results. </param>
        /// <param name="onProgress"> Update the loading progress and receive the latest loaded texture. </param>
        /// <param name="retry"> The retry number for each image. </param>
        /// <param name="timeOut"> The timout for each image. </param>
        public void Load(List<string> imageUrls, Action<Results> onComplete, Action<Result> onProgress = null, uint retry = 0, float timeOut = 30f)
        {
            LMGT.LoadingRetry = retry;
            LMGT.LoadingTimeOut = timeOut;
            LMGT.CacheMode = ImageLoader.CacheMode.NoCache;
            _Load(imageUrls, onComplete, onProgress);
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
            Action<Results> onComplete = null, Action<Result> onProgress = null, uint retry = 0, float timeOut = 30f)
        {
            LMGT.FileNamePrefix = filenamePrefix;
            LMGT.FolderName = folderName;
            LMGT.CacheMode = cacheMode;
            LMGT.LoadingRetry = retry;
            LMGT.LoadingTimeOut = timeOut;
            _Load(imageUrls, onComplete, onProgress);
        }

        private void _Load(List<string> imageUrls, Action<Results> onComplete, Action<Result> onProgress = null)
        {
            int cachedNum = 0;

            if (MemoryCacheEnabled && MemoryCache.MemoryCachedCount > 0)
            {
                Dictionary<int, string> theRestUrls = null;
                List<Result> cachedResults = null;
                if (MemoryCache.AutoGetMemoryCached) MemoryCache._GetMemoryCached(imageUrls, onProgress, ref cachedNum, ref theRestUrls, ref cachedResults);

                if (theRestUrls != null && theRestUrls.Count > 0) // load the rest Urls, if any
                {
                    var urlPairs = theRestUrls.ToArray();
                    Results results = new Results();
                    int finishCount = 0, urlCount = theRestUrls.Count;
                    for (int i = 0; i < urlCount; i++)
                    {
                        ImageLoader loader = ImageLoader.Create(LMGT.MaxCacheFilePerFolder, LMGT.CacheDirectoryEnum);
                        loader._ParentLoaderID = _ID;
                        loader.m_OnImageLoadError = m_OnImageLoadError;
                        loader.Load((uint)urlPairs[i].Key, urlPairs[i].Value, LMGT, (texture, index) =>
                        {
                            finishCount++;
                            _progress = (float)finishCount / urlCount;

                            bool fileDataReserved = loader.FileDataReserved;

                            // Reminded: If the texture cannot be loaded, it will return a null. So check null before use it.
                            Result result = new Result(loader.Data, texture, index, theRestUrls[(int)index], _progress,
                                loader.DetectedFileMime, loader.DetectedFileExtension, loader.CacheFilename, fileDataReserved);

                            if (onComplete != null) results.SetResult(index, result);

                            _OnProgressAndMemoryCaching(result, onProgress);

                            if (m_OnImageLoaded != null && (texture || fileDataReserved)) m_OnImageLoaded(result);

                            if (finishCount >= urlCount) // On Complete
                            {
                                if (onComplete != null)
                                {
                                    for (int j = 0; j < cachedResults.Count; j++)
                                    {
                                        results.SetResult(cachedResults[j].m_Index, cachedResults[j]);
                                    }
                                    results.OrderByIndex();
                                    onComplete(results);
                                }

                                if (m_OnAllImagesLoaded != null) m_OnAllImagesLoaded();
                            }
                        });
                    }
                }
            }

            if (cachedNum == 0) // no CacheItem retrieved, load the whole imageUrls list
            {
                Results results = new Results();
                int finishCount = 0, urlCount = imageUrls.Count;
                for (int i = 0; i < urlCount; i++)
                {
                    ImageLoader loader = ImageLoader.Create(LMGT.MaxCacheFilePerFolder, LMGT.CacheDirectoryEnum);
                    loader._ParentLoaderID = _ID;
                    loader.m_OnImageLoadError = m_OnImageLoadError;
                    loader.Load((uint)i, imageUrls[i], LMGT, (texture, index) =>
                    {
                        finishCount++;
                        _progress = (float)finishCount / urlCount;

                        bool fileDataReserved = loader.FileDataReserved;

                        // Reminded: If the texture cannot be loaded, it will return a null. So check null before use it.
                        Result result = new Result(loader.Data, texture, index, imageUrls[(int)index], _progress,
                            loader.DetectedFileMime, loader.DetectedFileExtension, loader.CacheFilename, fileDataReserved);

                        if (onComplete != null) results.SetResult(index, result);

                        _OnProgressAndMemoryCaching(result, onProgress);

                        if (m_OnImageLoaded != null && (texture || fileDataReserved)) m_OnImageLoaded(result);

                        if (finishCount >= urlCount) // On Complete
                        {
                            if (onComplete != null)
                            {
                                results.OrderByIndex();
                                onComplete(results);
                            }

                            if (m_OnAllImagesLoaded != null) m_OnAllImagesLoaded();
                        }
                    });
                }
            }
        }
        #endregion ----- Load -----


        #region ----- Load_Queued (Queued Loader) -----
        /// <summary>
        /// Setup to use a single Queued Loader instance for the Load_Queue methods.
        /// In this case the ImageLoader instances will be strictly limited by the maxLoaderNum parameter of Load_Queue methods.
        /// </summary>
        /// <param name="dontDestroyOnLoad"> If 'true', prevent the single Queued Loader instance being destroyed when loading a new scene.
        /// (Reminded! Please ensure this ImageBatchLoader not being cleared when loading a new scene, if dontDestroyOnLoad is set.)
        /// </param>
        public void EnableSingleQueuedLoaderMode(bool dontDestroyOnLoad = false)
        {
            _singleQueueInstance = true;
            _dontDestroyOnLoad_Single = dontDestroyOnLoad;
        }
        /// <summary>
        /// If 'true', use only one Queued Loader for Load_Queue methods of this ImageBatchLoader.
        /// </summary>
        private bool _singleQueueInstance;
        /// <summary>
        /// If 'true', prevent the single Queued Loader instance being destroyed when loading a new scene.
        /// </summary>
        private bool _dontDestroyOnLoad_Single;

        /// <summary>
        /// Load multiple images in a queued manner and using the settings of the provided LoaderManagement object. Return the Result objects in the onComplete and onProgress callbacks.
        /// (Queued manner can prevent creating too many loader instances at once)
        /// </summary>
        /// <param name="maxLoaderNum"> Max number of ImageLoaders to use concurrently. </param>
        /// <param name="imageUrls"> Image urls or local paths. </param>
        /// <param name="lmgt"> The loader Cache Management and Loading Settings. </param>
        /// <param name="onComplete"> The callback for receiving all the loaded results. </param>
        /// <param name="onProgress"> Update the loading progress and receive the latest loaded texture. </param>
        public ImageQueuedLoader Load_Queue(uint maxLoaderNum, List<string> imageUrls, LoaderManagement lmgt, Action<Results> onComplete, Action<Result> onProgress = null)
        {
            LMGT = new LoaderManagement(lmgt);
            return _Load_Queue(maxLoaderNum, imageUrls, onComplete, onProgress);
        }

        /// <summary>
        /// Load multiple images in a queued manner and without using the cache feature (do not save/load in any local cache folder). Return the Result objects in the onComplete and onProgress callbacks.
        /// (Queued manner can prevent creating too many loader instances at once)
        /// </summary>
        /// <param name="maxLoaderNum"> Max number of ImageLoaders to use concurrently. </param>
        /// <param name="imageUrls"> Image urls or local paths. </param>
        /// <param name="onComplete"> The callback for receiving all the loaded results. </param>
        /// <param name="onProgress"> Update the loading progress and receive the latest loaded texture. </param>
        /// <param name="retry"> The retry number for each image. </param>
        /// <param name="timeOut"> The timout for each image. </param>
        public ImageQueuedLoader Load_Queue(uint maxLoaderNum, List<string> imageUrls, Action<Results> onComplete, Action<Result> onProgress = null, uint retry = 0, float timeOut = 30f)
        {
            LMGT.LoadingRetry = retry;
            LMGT.LoadingTimeOut = timeOut;
            LMGT.CacheMode = ImageLoader.CacheMode.NoCache;
            return _Load_Queue(maxLoaderNum, imageUrls, onComplete, onProgress);
        }

        /// <summary>
        /// Load multiple images in a queued manner and using specific cache and load settings. Return the Result objects in the onComplete and onProgress callbacks.
        /// (Queued manner can prevent creating too many loader instances at once)
        /// </summary>
        /// <param name="maxLoaderNum"> Max number of ImageLoaders to use concurrently. </param>
        /// <param name="imageUrls"> Image urls or local paths. </param>
        /// <param name="filenamePrefix"> The filename prefix for storing(caching) the image files. The final filename will be combined by this prefix and the index together. </param>
        /// <param name="folderName"> The target folder for storing(caching) the image file. </param>
        /// <param name="cacheMode"> The behavior for handling Load and Cache files. (NoCache: do not auto save the image; UseCached: use the locally cached file if exist; Replace: download and replace the locally cached file is exist) </param>
        /// <param name="onComplete"> The callback for receiving all the loaded results. </param>
        /// <param name="onProgress"> Update the loading progress and receive the latest loaded texture. </param>
        /// <param name="retry"> The retry number for each image. </param>
        /// <param name="timeOut"> The timout for each image. </param>
        public ImageQueuedLoader Load_Queue(uint maxLoaderNum, List<string> imageUrls, string filenamePrefix, string folderName, ImageLoader.CacheMode cacheMode = ImageLoader.CacheMode.Replace,
            Action<Results> onComplete = null, Action<Result> onProgress = null, uint retry = 0, float timeOut = 30f)
        {
            LMGT.FileNamePrefix = filenamePrefix;
            LMGT.FolderName = folderName;
            LMGT.CacheMode = cacheMode;
            LMGT.LoadingRetry = retry;
            LMGT.LoadingTimeOut = timeOut;
            return _Load_Queue(maxLoaderNum, imageUrls, onComplete, onProgress);
        }

        /// <summary>
        /// A particular ImageQueuedLoader for Load_Queue methods to load a list of image paths/URLs, if Single Queued Loader mode is enabled.
        /// </summary>
        private ImageQueuedLoader _listQueuedLoader;
        private ImageQueuedLoader _Load_Queue(uint maxLoaderNum, List<string> imageUrls, Action<Results> onComplete, Action<Result> onProgress = null)
        {
            int cachedNum = 0;

            if (MemoryCacheEnabled && MemoryCache.MemoryCachedCount > 0)
            {
                Dictionary<int, string> theRestUrls = null;
                List<Result> cachedResults = null;
                if (MemoryCache.AutoGetMemoryCached) MemoryCache._GetMemoryCached(imageUrls, onProgress, ref cachedNum, ref theRestUrls, ref cachedResults);

                if (theRestUrls != null && theRestUrls.Count > 0) // load the rest Urls, if any
                {
                    int[] urlIndexArray = theRestUrls.Keys.ToArray();
                    int loaded = 0, total = imageUrls.Count;

                    if (_singleQueueInstance)
                    {
                        if (_listQueuedLoader == null)
                        {
                            _listQueuedLoader = ImageQueuedLoader.Create(maxLoaderNum, maxQueueSize: 0);
                            _listQueuedLoader.name = "[ImageQueuedLoader-List]";
                            _listQueuedLoader.m_DestroyOnComplete = false;
                            if (_dontDestroyOnLoad_Single) _listQueuedLoader.DontDestroyOnLoad();
                        }
                        _listQueuedLoader.m_MaxLoaderNum = maxLoaderNum;
                        _listQueuedLoader.m_OnImageLoaded = m_OnImageLoaded;
                        _listQueuedLoader.m_OnAllImagesLoaded = m_OnAllImagesLoaded;
                        _listQueuedLoader.m_OnImageLoadError = m_OnImageLoadError;
                        _listQueuedLoader.Load(theRestUrls.Values.ToList(), LMGT,
                            (results) => // On Complete
                            {
                                if (onComplete != null)
                                {
                                    if (cachedResults != null)
                                    {
                                        for (int i = 0; i < cachedResults.Count; i++)
                                        {
                                            results.SetResult(cachedResults[i].m_Index, cachedResults[i], 2);
                                        }
                                    }
                                    results.OrderByIndex();
                                    onComplete(results);
                                }
                            },
                            (result) => // On Progress
                            {
                                loaded++;
                                result.m_Progress = (float)(cachedNum + loaded) / total;        // correct the progress value 
                                result.m_Index = (uint)(urlIndexArray[(int)result.m_Index]);    // modify the Result index, match with imageUrls
                                _OnProgressAndMemoryCaching(result, onProgress);
                            });
                    }
                    else
                    {
                        ImageQueuedLoader qLoader = ImageQueuedLoader.Create(maxLoaderNum);
                        qLoader.m_OnImageLoaded = m_OnImageLoaded;
                        qLoader.m_OnAllImagesLoaded = m_OnAllImagesLoaded;
                        qLoader.m_OnImageLoadError = m_OnImageLoadError;
                        qLoader.Load(theRestUrls.Values.ToList(), LMGT,
                            (results) => // On Complete
                            {
                                if (onComplete != null)
                                {
                                    if (cachedResults != null)
                                    {
                                        for (int i = 0; i < cachedResults.Count; i++)
                                        {
                                            results.SetResult(cachedResults[i].m_Index, cachedResults[i], 3);
                                        }
                                    }
                                    results.OrderByIndex();
                                    onComplete(results);
                                }
                            },
                            (result) => // On Progress
                            {
                                loaded++;
                                result.m_Progress = (float)(cachedNum + loaded) / total;        // correct the progress value 
                                result.m_Index = (uint)(urlIndexArray[(int)result.m_Index]);    // modify the Result index, match with imageUrls
                                _OnProgressAndMemoryCaching(result, onProgress);
                            });
                        return qLoader;
                    }
                }
            }

            if (cachedNum == 0) // no CacheItem retrieved, load the whole imageUrls list
            {
                if (_singleQueueInstance)
                {
                    if (_listQueuedLoader == null)
                    {
                        _listQueuedLoader = ImageQueuedLoader.Create(maxLoaderNum, maxQueueSize: 0);
                        _listQueuedLoader.name = "[ImageQueuedLoader-List]";
                        _listQueuedLoader.m_DestroyOnComplete = false;
                        if (_dontDestroyOnLoad_Single) _listQueuedLoader.DontDestroyOnLoad();
                    }
                    _listQueuedLoader.m_MaxLoaderNum = maxLoaderNum;
                    _listQueuedLoader.m_OnImageLoaded = m_OnImageLoaded;
                    _listQueuedLoader.m_OnAllImagesLoaded = m_OnAllImagesLoaded;
                    _listQueuedLoader.m_OnImageLoadError = m_OnImageLoadError;
                    _listQueuedLoader.Load(imageUrls, LMGT, onComplete,
                        (result) =>
                        {
                            _OnProgressAndMemoryCaching(result, onProgress);
                        });
                }
                else
                {
                    ImageQueuedLoader qLoader = ImageQueuedLoader.Create(maxLoaderNum);
                    qLoader.m_OnImageLoaded = m_OnImageLoaded;
                    qLoader.m_OnAllImagesLoaded = m_OnAllImagesLoaded;
                    qLoader.m_OnImageLoadError = m_OnImageLoadError;
                    qLoader.Load(imageUrls, LMGT, onComplete,
                        (result) =>
                        {
                            _OnProgressAndMemoryCaching(result, onProgress);
                        });
                    return qLoader;
                }
            }

            return _listQueuedLoader;
        }
        #endregion ----- Load_Queued (Queued Loader) -----


        #region ----- In-Memory Cache -----
        /// <summary>
        /// In-Memory Cache object that store and manage the loaded textures if Memory Cache feature is enabled.
        /// *Tips:
        /// (1) Call the EnableMemoryCache method to use Memory Cache.
        /// (2) The MemoryCacheEnabled boolean indicates if MemoryCache is enabled.
        /// (2) Images will be cached as per URL if both Storage Cache and Memory Cache are used.
        /// (3) Textures stored in this memory cache can also be destroyed from outside.
        /// </summary>
        public InMemoryCache MemoryCache = null;

        /// <summary>
        /// If 'true', memory cache has enabled for this loader, which the max MemoryCacheNum has been set to larger than zero.
        /// </summary>
        public bool MemoryCacheEnabled
        {
            get
            {
                return MemoryCache != null && MemoryCache.Enabled;
            }
        }

        /// <summary>
        /// Setup to use the Memory Cache feature for this ImageBatchLoader.
        /// *Tips:
        /// (1) Images will be cached as per URL if both Storage Cache and Memory Cache are used.
        /// (2) Textures stored in this memory cache can also be destroyed from outside.
        /// </summary>
        /// <param name="maxLoaderNum"> Max number of ImageLoaders to use concurrently. (Requires at least 1 to load the images) </param>
        /// <param name="maxMemoryCacheNum"> The max num of images allowed to store in the memory (in the CacheItem list of this loader). </param>
        /// <param name="maxQueueSize"> Max number of loading tasks allowed to queue in the URL/path(task) list.
        /// If the tasks exceeded this limit, the older waiting tasks will be removed. 0 = no limit (default). </param>
        /// <param name="dontDestroyOnLoad"> If 'true', prevent the dedicated Queued Loader instance being destroyed when loading a new scene.
        /// (Reminded! Please ensure this ImageBatchLoader not being cleared when loading a new scene, if dontDestroyOnLoad is set)
        /// </param>
        public void EnableMemoryCache(uint maxLoaderNum, uint maxMemoryCacheNum, uint maxQueueSize = 0, bool dontDestroyOnLoad = false)
        {
            MemoryCache = new InMemoryCache(this, maxLoaderNum, maxMemoryCacheNum, maxQueueSize, dontDestroyOnLoad);
        }

        [Serializable]
        public class CacheItem
        {
            /// <summary>
            /// A locked item has a higher priority to remain in the memory cache. If an item is not locked, it will be cleared first when MemoryCachedCount exceeds the max limit.
            /// You can still clear a locked item with the RemoveMemoryCacheItem methods, or using the ClearMemoryCache method, or just un-lock without removing it immediately.
            /// However, 'loacked' does not prevent its texture from being cleared from outside, like calling the UnityEngine.Object.Destroy(texture) method, etc.
            /// </summary>
            public bool m_Locked;

            /// <summary>
            /// The URL/path that is used to load the image.
            /// </summary>
            public string m_URL;

            /// <summary>
            /// A texture that is loaded/stored in the memory cache of the belonging loader.
            /// </summary>
            public Texture2D m_Texture;

            /// <summary>
            /// Indicate if the image file data is kept for this image.
            /// </summary>
            public bool m_FileDataReserved;
            /// <summary>
            /// The image byte array. (To access the image data, please set LMGT.LoadFileMode to the appropriate mode you want)
            /// </summary>
            [HideInInspector] public byte[] m_Data;

            public string m_DetectedFileMime;
            public string m_DetectedFileExtension;

            public string m_CacheFilename;

            public CacheItem(string imageUrl, byte[] data, Texture2D texture, string mime, string extension, string cacheFilename, bool fileDataReserved)
            {
                m_Data = data;
                m_URL = imageUrl;
                m_Texture = texture;
                m_DetectedFileMime = mime;
                m_DetectedFileExtension = extension;
                m_CacheFilename = cacheFilename;
                m_FileDataReserved = fileDataReserved;
            }
        }

        public class InMemoryCache
        {
            private ImageBatchLoader _parentLoader;
            public InMemoryCache(ImageBatchLoader batchLoader, uint maxLoaderNum, uint maxMemoryCacheNum, uint maxQueueSize = 0, bool dontDestroyOnLoad = false)
            {
                _parentLoader = batchLoader;
                _maxLoaderNum = maxLoaderNum;
                _maxQueueSize = maxQueueSize;
                _dontDestroyOnLoad = dontDestroyOnLoad;
                SetMaxMemoryCacheNum(maxMemoryCacheNum);
            }

            /// <summary>
            /// A particular ImageQueuedLoader for the Memory Cache get method, if Single Queued Loader mode is enabled.
            /// </summary>
            private ImageQueuedLoader _memCacheQueuedLoader;
            private bool _dontDestroyOnLoad;
            private uint _maxLoaderNum, _maxQueueSize;
            private ImageQueuedLoader _GetImageQueuedLoader()
            {
                if (_memCacheQueuedLoader == null)
                {
                    _memCacheQueuedLoader = ImageQueuedLoader.Create(_maxLoaderNum, _maxQueueSize);
                    if (_dontDestroyOnLoad) _memCacheQueuedLoader.DontDestroyOnLoad();
                    _memCacheQueuedLoader.name = "[ImageQueuedLoader-MemCache]";
                    _memCacheQueuedLoader.m_DestroyOnComplete = false;
                    _memCacheQueuedLoader.LMGT = _parentLoader.LMGT;
                    _memCacheQueuedLoader.Init(onComplete: null, (result) =>
                    {   // On Progress
                        _parentLoader._OnProgressAndMemoryCaching(result, null);
                    }, lmgt: null);
                }
                return _memCacheQueuedLoader;
            }

            internal uint _maxMemoryCacheNum = 0;
            /// <summary>
            /// Loaded images(CachedItem) that cached in this ImageBatchLoader.
            /// </summary>
            private List<CacheItem> _cachedItems = new List<CacheItem>();
            /// <summary>
            /// Get all the cached images in this loader.
            /// </summary>\
            public List<CacheItem> GetCacheItems()
            {
                return _cachedItems;
            }

            /// <summary>
            /// If 'true', automatically gets the cached items in this loader's memory cache when calling the Load/Load_Queue methods. (Default = true)
            /// </summary>
            public bool AutoGetMemoryCached = true;

            /// <summary>
            /// If 'true', auto clear the texture in the item when it is being removed from Memory Cache by our internal codes (e.g. when exceeding the cache limit). 
            /// Reminded! Make sure the maxMemoryCacheNum is larger than the total rendering/UI objects, else it may clear some textures used by the objects!
            /// </summary>
            public bool AutoClearMemoryCachedTexture = true;

            /// <summary>
            /// If 'true', memory cache has enabled for this loader, which the max MemoryCacheNum has been set to larger than zero.
            /// </summary>
            public bool Enabled
            {
                get
                {
                    bool enabled = _maxMemoryCacheNum > 0;

                    // Images will be cached as per URL if both Storage Cache and Memory Cache are used.
                    if (enabled && _parentLoader.LMGT.CacheMode != ImageLoader.CacheMode.NoCache) _parentLoader.LMGT.CacheAsPerUrl = true;

                    return enabled;
                }
            }

            /// <summary>
            /// Total num of Results/images currently cached in this ImageBatchLoader.
            /// </summary>
            public int MemoryCachedCount
            {
                get
                {
                    return _cachedItems.Count;
                }
            }

            /// <summary>
            /// Set the max number for caching images in the memory for this ImageBatchLoader, 0 = disable the memory cache.
            /// </summary>
            /// <param name="maxMemoryCacheNum"> The max num of images allowed to store in the memory (in the CacheItem list of this loader). </param>
            public void SetMaxMemoryCacheNum(uint maxMemoryCacheNum)
            {
                _maxMemoryCacheNum = maxMemoryCacheNum;

                int diff = MemoryCachedCount - (int)maxMemoryCacheNum;
                if (diff > 0)
                {
                    for (int i = 0; i < diff; i++)
                    {
                        if (AutoClearMemoryCachedTexture) _DestroyTextureInMemoryCacheItem(_cachedItems[0], canClearLockedTexture: true);
                        _cachedItems.RemoveAt(0);
                    }
                    _cachedItems.TrimExcess();
                }
            }

            private List<string> _lockedImageUrls = new List<string>();
            /// <summary>
            /// Change the Lock flag of a specific CacheItem by the image URL. (A locked flag can avoid the image from being auto-cleared, unless the total locked items exceeded the cache limit)
            /// </summary>
            /// <param name="imageUrl"> The URL of the target CacheItem/image. </param>
            /// <param name="isLock"> The new lock state for the target CacheItem/image. </param>
            public void SetMemoryCacheItemLockState(string imageUrl, bool isLock)
            {
                CacheItem item = _cachedItems.Find(ci => ci.m_URL == imageUrl);
                if (item != null) item.m_Locked = isLock;

                if (isLock)
                {   // add the url to lock list first, will lock the item in the _TryAddToMemoryCache() method
                    if (!_lockedImageUrls.Contains(imageUrl)) _lockedImageUrls.Add(imageUrl);
                }
                else
                {
                    // remove the image URL from the lock list
                    if (_lockedImageUrls.Contains(imageUrl)) _lockedImageUrls.Remove(imageUrl);
                }
            }
            /// <summary>
            /// Provide a URL/path list to this ImageBatchLoader to lock the images by path/URL, this applies to current memory cached images and future loads.
            /// **Reminded! If the URL list is empty and replace = true, then all items will be un-locked.
            /// </summary>
            /// <param name="lockImageUrls"> A list of image URLs to be locked (when they are loaded in the memory cache). </param>
            /// <param name="replace"> If 'true', replace the existing lock list with the new one, else append the new list to the existing one. </param>
            public void LockMemoryCacheItems(List<string> lockImageUrls, bool replace = true)
            {
                if (replace)
                {
                    if (lockImageUrls == null) _lockedImageUrls = new List<string>(); else _lockedImageUrls = lockImageUrls;
                }
                else if (lockImageUrls != null && lockImageUrls.Count > 0)
                {
                    if (_lockedImageUrls == null) _lockedImageUrls = new List<string>();

                    for (int i = 0; i < lockImageUrls.Count; i++)
                    {
                        string newUrl = lockImageUrls[i];
                        if (!string.IsNullOrEmpty(newUrl) && !_lockedImageUrls.Contains(newUrl)) _lockedImageUrls.Add(newUrl);
                    }
                }

                if (MemoryCachedCount > 0)
                {
                    if (_lockedImageUrls != null && _lockedImageUrls.Count > 0)
                    {   // lock items that are in the _lockedImageUrls
                        for (int i = 0; i < _cachedItems.Count; i++) _cachedItems[i].m_Locked = _lockedImageUrls.Contains(_cachedItems[i].m_URL);
                    }
                    else
                    {   // un-lock all items:
                        for (int i = 0; i < _cachedItems.Count; i++) _cachedItems[i].m_Locked = false;
                    }
                }
            }

            /// <summary>
            /// Check if a particular image exists in the memory cache, by comparing the filename.
            /// </summary>
            /// <param name="filename"> The filename of the requesting image, e.g. MyImage001 (optional to provide the file extension) </param>
            public bool HasCache_ByFilename(string filename)
            {
                string filenameExt = filename + _parentLoader.LMGT.FileExtension;
                return Enabled && !string.IsNullOrEmpty(filename) && _cachedItems.Any(item => item.m_CacheFilename == filename || item.m_CacheFilename == filenameExt);
            }

            /// <summary>
            /// Check if a particular image exists in the memory cache, by comparing the URL.
            /// </summary>
            public bool HasCache_ByUrl(string imageUrl)
            {
                return Enabled && !string.IsNullOrEmpty(imageUrl) && _cachedItems.Any(item => item.m_URL == imageUrl);
            }

            /// <summary>
            /// Check if a particular image exists in the memory cache, by comparing the texture reference.
            /// </summary>
            public bool HasCache_ByTexture(Texture texture)
            {
                return Enabled && texture && _cachedItems.Any(item => item.m_Texture == texture);
            }

            /// <summary>
            /// Get an image(CacheItem) by texture reference. Return null if not found.
            /// (Check null before using the returned object)
            /// </summary>
            public CacheItem GetMemoryCacheItemByTexture(Texture texture)
            {
                return _cachedItems.Find(item => item.m_Texture == texture);
            }

            /// <summary>
            /// Get an image(CacheItem) by path/URL, optional to load the image with the current LMGT settings if the image is not found in this loader memory cache.
            /// (Check null before using the returned object)
            /// </summary>
            /// <param name="loadIfNotFound"> If 'true', load the image to the memory cache if not found, so we can access it later. (Requires memory caching to be enabled) </param>
            /// <param name="isLock"> If 'true', lock the item if it exists in the memory cache or lock it when loaded. </param>
            /// <param name="customFilename"> (Optional) Specify a custom filename(without file extension) to cache/load this image in the cache folder. </param>
            /// <param name="customIndex"> (Optional) The index number can be assigned before loading multiple images, for sorting the results when needed.
            /// If this is provided (larger than -1), this index will be set to the index of its Result object, which can be accessed in the callbacks. </param>
            public CacheItem GetMemoryCacheItemByUrl(string imageUrl, bool loadIfNotFound = false, bool isLock = false, string customFilename = null, int customIndex = -1)
            {
                CacheItem item = _cachedItems.Find(ci => ci.m_URL == imageUrl
                    || (!string.IsNullOrEmpty(customFilename) && !string.IsNullOrEmpty(ci.m_CacheFilename)
                    && customFilename == System.IO.Path.GetFileNameWithoutExtension(ci.m_CacheFilename)));

                if (item != null)
                {
                    if (isLock && !_lockedImageUrls.Contains(imageUrl))
                        _lockedImageUrls.Add(imageUrl); // lock an existing item

                    if (item.m_Texture || item.m_FileDataReserved)
                    {
                        item.m_Locked = isLock;
                        MoveMemoryCacheItemToLast(item);
                        return item;
                    }
                    else
                    {
                        _cachedItems.Remove(item); // Remove the item if its texture is null (maybe destroyed from outside!!)
                    }
                }

                if (loadIfNotFound)
                {
                    if (!Enabled)
                    {
#if UNITY_EDITOR
                        Debug.LogError("Images will not be cached! Make sure Memory Caching is enabled for supporting loadIfNotFound. See the SetMaxMemoryCacheNum method.");
#endif
                    }
                    else
                    {
                        if (isLock && !_lockedImageUrls.Contains(imageUrl))
                            _lockedImageUrls.Add(imageUrl); // add the url to lock list first, will lock the item in the _TryAddToMemoryCache() method

                        ImageQueuedLoader qLoader = _GetImageQueuedLoader();
                        qLoader.LMGT = _parentLoader.LMGT;
                        qLoader.m_OnImageLoaded = _parentLoader.m_OnImageLoaded;
                        qLoader.m_OnAllImagesLoaded = _parentLoader.m_OnAllImagesLoaded;
                        qLoader.m_OnImageLoadError = _parentLoader.m_OnImageLoadError;
                        qLoader.Add(imageUrl, customFilename, customIndex);
                    }
                }
                return null;
            }

            /// <summary>
            /// Get an image(Texture2D) by path/URL, optional to load the image with the current LMGT settings if the image is not found in this loader memory cache.
            /// (Check null before using the returned object)
            /// </summary>
            /// <param name="loadIfNotFound"> If 'true', load the image to the memory cache if not found, so we can access it later. (Requires memory caching to be enabled) </param>
            /// <param name="isLock"> If 'true', lock the item if it exists in the memory cache or lock it when loaded. </param>
            /// <param name="customFilename"> (Optional) Specify a custom filename(without file extension) to cache/load this image in the cache folder. </param>
            /// <param name="customIndex"> (Optional) The index number can be assigned before loading multiple images, for sorting the results when needed.
            /// If this is provided (larger than -1), this index will be set to the index of its Result object, which can be accessed in the callbacks. </param>
            public Texture2D GetImageByUrl(string imageUrl, bool loadIfNotFound = false, bool isLock = false, string customFilename = null, int customIndex = -1)
            {
                return GetMemoryCacheItemByUrl(imageUrl, loadIfNotFound, isLock, customFilename, customIndex)?.m_Texture;
            }

            /// <summary>
            /// Force load an image to replace the existing one in the cache and storage.
            /// Note that the newly loaded texture will replace the texture in the existing CacheItem(if any).
            /// </summary>
            /// <param name="isLock"> If 'true', lock the item if it exists in the memory cache or lock it when loaded. </param>
            /// <param name="customFilename"> (Optional) Specify a custom filename(without file extension) to cache/load this image in the cache folder. </param>
            /// <param name="customIndex"> (Optional) The index number can be assigned before loading multiple images, for sorting the results when needed.
            /// If this is provided (larger than -1), this index will be set to the index of its Result object, which can be accessed in the callbacks. </param>
            public void LoadImageByUrl(string imageUrl, bool isLock = false, string customFilename = null, int customIndex = -1)
            {
                if (isLock && !_lockedImageUrls.Contains(imageUrl))
                    _lockedImageUrls.Add(imageUrl); // add the url to lock list first, will lock the item in the _TryAddToMemoryCache() method

                ImageQueuedLoader qLoader = _GetImageQueuedLoader();
                qLoader.LMGT = _parentLoader.LMGT;
                qLoader.m_OnImageLoaded = _parentLoader.m_OnImageLoaded;
                qLoader.m_OnAllImagesLoaded = _parentLoader.m_OnAllImagesLoaded;
                qLoader.m_OnImageLoadError = _parentLoader.m_OnImageLoadError;
                qLoader.Add(imageUrl, ImageLoader.CacheMode.Replace, customFilename, customIndex);
            }

            /// <summary>
            /// Move the target image(CacheItem) to the end of the cache list, just like a new loaded item.
            /// </summary>
            public void MoveMemoryCacheItemToLast(CacheItem item)
            {
                if (item != null && _cachedItems.Contains(item) && _cachedItems.Last() != item)
                {
                    _cachedItems.Remove(item);
                    _cachedItems.Add(item);
                }
            }

            /// <summary>
            /// Remove an image(CacheItem) by path/URL, optional not to remove it if locked.
            /// </summary>
            /// <param name="imageUrl"> The path/URL of the cached item we look for, a CacheItem with this URL will be cleared. </param>
            /// <param name="canRemoveLockedItem"> If 'true', remove the item even if it is Locked or Not. Else, do not remove if it is locked. </param>
            public void RemoveMemoryCacheItemByUrl(string imageUrl, bool canRemoveLockedItem = true)
            {
                CacheItem item = _cachedItems.Find(ci => ci.m_URL == imageUrl);
                if (item != null)
                {
                    _DestroyTextureInMemoryCacheItem(item, canRemoveLockedItem);
                    if (!item.m_Locked || canRemoveLockedItem) _cachedItems.Remove(item);
                }
            }

            /// <summary>
            /// Remove an image(CacheItem) by texture reference, optional not to remove it if locked.
            /// </summary>
            /// <param name="texture"> The texture of the cached item we look for, a CacheItem with this texture will be cleared. </param>
            /// <param name="canRemoveLockedItem"> If 'true', remove the item even if it is Locked or Not. Else, do not remove if it is locked. </param>
            public void RemoveMemoryCacheItemByTexture(Texture texture, bool canRemoveLockedItem = true)
            {
                CacheItem item = GetMemoryCacheItemByTexture(texture);
                if (item != null)
                {
                    _DestroyTextureInMemoryCacheItem(item, canRemoveLockedItem);
                    if (!item.m_Locked || canRemoveLockedItem) _cachedItems.Remove(item);
                }
            }

            /// <summary>
            /// Remove an image from both Storage Cache folder and Memory Cache list of this loader. In which the image was cached with LMGT.CacheAsPerUrl = true.
            /// </summary>
            /// <param name="canRemoveLockedItem"> If 'true', remove the item even if it is Locked or Not. Else, do not remove if it is locked. </param>
            public void RemoveFromCaches(string imageUrl, bool canRemoveLockedItem = true)
            {
                bool removeStorageCacheFile = false;
                CacheItem item = _cachedItems.Find(ci => ci.m_URL == imageUrl);
                if (item != null && (!item.m_Locked || canRemoveLockedItem))
                {
                    _cachedItems.Remove(item);
                    removeStorageCacheFile = true;
                }

                if (removeStorageCacheFile || item == null)
                {
                    string filename = MD5Util.ToMD5Hash(imageUrl); // MD5 hash as filename, for Cache-As-Per-URL
                    string filePath = System.IO.Path.Combine(_parentLoader.LMGT.CacheFolderPath, filename);
                    ImageLoader.DeleteFileByPath(filePath);
                }
            }

            /// <summary>
            /// Cancel all the current (downloading) loader instances in this ImageBatchLoader. i.e. Stop/Destroy ImageLoader instances that started but are not yet finished.
            /// </summary>
            /// <param name="exceptLocked"> If 'true', do not cancel those image URLs that are marked as locked. </param>
            public void CancelAllLoading(bool exceptLocked = false)
            {
                if (_memCacheQueuedLoader) _memCacheQueuedLoader.CancelAllLoading(exceptLocked ? _lockedImageUrls : null);
            }

            /// <summary>
            /// Cancel all the pending tasks in this ImageBatchLoader.
            /// (The loading tasks were queued on the waiting list as the total tasks more than the maximum Loader limit)
            /// </summary>
            /// <param name="exceptLocked"> If 'true', do not cancel those image URLs that are marked as locked. </param>
            public void CancelAllPending(bool exceptLocked = false)
            {
                if (_memCacheQueuedLoader) _memCacheQueuedLoader.CancelAllPending(exceptLocked ? _lockedImageUrls : null);
            }

            /// <summary>
            /// Clear all the cached items and textures in this ImageBatchLoader. Optional not to clear the locked items.
            /// </summary>
            /// <param name="exceptLocked"> If 'true', do not clear the locked items. </param>
            public void ClearMemoryCache(bool exceptLocked = false)
            {
                for (int i = 0; i < _cachedItems.Count; i++)
                {
                    CacheItem item = _cachedItems[i];
                    if (item.m_Locked && exceptLocked)
                    {
                        // Do nothing
                    }
                    else
                    {
                        _DestroyTextureInMemoryCacheItem(_cachedItems[i], canClearLockedTexture: true);
                        _cachedItems.RemoveAt(i--);
                    }
                }
                _cachedItems.TrimExcess();
            }

            private void _DestroyTextureInMemoryCacheItem(CacheItem item, bool canClearLockedTexture)
            {
                if (item.m_Texture && (!item.m_Locked || canClearLockedTexture))
                {
                    UnityEngine.Object.Destroy(item.m_Texture);
                }
            }

            private void _AddToMemoryCache(Result result)
            {
                CacheItem item = new CacheItem(result.m_URL, result.m_Data, result.m_Texture,
                    result.m_DetectedFileMime, result.m_DetectedFileExtension, result.m_CacheFilename, result.m_FileDataReserved);
                item.m_Locked = _lockedImageUrls != null && _lockedImageUrls.Contains(result.m_URL);
                _cachedItems.Add(item);

                result._cacheItemRef = item;
            }

            internal void _TryAddToMemoryCache(Result result)
            {
                if (!Enabled || result == null || (result.m_Texture == null && !result.m_FileDataReserved)) return;

                if (!HasCache_ByUrl(result.m_URL))
                {
                    _AddToMemoryCache(result);
                }
                else // Exists:
                {
                    // Replace the existing texture with the new one, this could be a Result of LoadImageByUrl().
                    CacheItem item = _cachedItems?.Find(ci => ci.m_URL == result.m_URL);
                    if (item != null)
                    {
                        if (item.m_Texture) UnityEngine.Object.Destroy(item.m_Texture);
                        item.m_Texture = result.m_Texture;
                        item.m_Locked = _lockedImageUrls != null && _lockedImageUrls.Contains(result.m_URL);
                    }

                    //// Clear duplicate load result to avoid leaks
                    //UnityEngine.Object.Destroy(result.m_Texture);
                    //result.m_Texture = null;
                }

                if (MemoryCachedCount > _maxMemoryCacheNum) // Clear un-locked item first
                {
                    for (int i = 0; i < _cachedItems.Count; i++)
                    {
                        if (!_cachedItems[i].m_Locked)
                        {
                            if (AutoClearMemoryCachedTexture) _DestroyTextureInMemoryCacheItem(_cachedItems[i], canClearLockedTexture: true);
                            _cachedItems.RemoveAt(i--);
                            if (MemoryCachedCount <= _maxMemoryCacheNum) break;
                        }
                    }
                }

                if (MemoryCachedCount > _maxMemoryCacheNum) // Seems too many items are locked...
                {
#if UNITY_EDITOR
                    if (_parentLoader.LMGT.IsDebug) Debug.LogWarning("Too many items locked? This is not ideal, " +
                        "you may consider setting a larger cache limit, or choose to lock the most commonly used (important) images only.");
#endif
                    if (AutoClearMemoryCachedTexture) _DestroyTextureInMemoryCacheItem(_cachedItems[0], canClearLockedTexture: true);
                    _cachedItems.RemoveAt(0);
                }
            }

            internal void _GetMemoryCached(List<string> imageUrls, Action<Result> onProgress, ref int cachedNum, ref Dictionary<int, string> theRestUrls, ref List<Result> cachedResults)
            {
                int num = 0;
                Dictionary<int, string> urls = new Dictionary<int, string>();
                List<Result> cached = new List<Result>();
                for (int i = 0; i < imageUrls.Count; i++)
                {
                    CacheItem item = GetMemoryCacheItemByUrl(imageUrls[i]);
                    if (item != null)
                    {
                        if (item.m_Texture || item.m_FileDataReserved)
                        {
                            num++;
                            _parentLoader._progress = (float)num / imageUrls.Count;
                            Result result = new Result(item.m_Data, item.m_Texture, (uint)i, imageUrls[i], _parentLoader._progress,
                                item.m_DetectedFileMime, item.m_DetectedFileExtension, item.m_CacheFilename, item.m_FileDataReserved);
                            cached.Add(result);
                            if (onProgress != null) onProgress(result);
                        }
                        else
                        {
                            _cachedItems.Remove(item);
                            urls.Add(i, imageUrls[i]);
                        }
                    }
                    else
                    {
                        urls.Add(i, imageUrls[i]);
                    }
                }
                cachedNum = num;
                theRestUrls = urls;
                cachedResults = cached;
            }
        }
        #endregion ----- In-Memory Cache -----
    }

}
