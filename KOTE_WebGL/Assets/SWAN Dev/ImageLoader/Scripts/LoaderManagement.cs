using System;
using System.IO;
using UnityEngine;

// ImageBox namespace.
namespace IMBX
{
    /// <summary>
    /// The loader Cache Management and Loading Settings.
    /// </summary>
    [Serializable]
    public class LoaderManagement
    {
        [Tooltip("If true, show debug log in the editor console.")]
        public bool IsDebug;

        public LoaderManagement() { }

        public LoaderManagement(LoaderManagement LM)
        {
            this.IsDebug = LM.IsDebug;
            this.CacheDirectoryEnum = LM.CacheDirectoryEnum;
            this.CacheMode = LM.CacheMode;
            this.FileExtension = LM.FileExtension;
            this.FileIndexFormatDigitsCount = LM.FileIndexFormatDigitsCount;
            this.FileNameAndIndexSeparator = LM.FileNameAndIndexSeparator;
            this.CacheAsPerUrl = LM.CacheAsPerUrl;
            this.FileNamePrefix = LM.FileNamePrefix;
            this.FileNameStartingIndex = LM.FileNameStartingIndex;
            this.FolderName = LM.FolderName;
            this.LoadingRetry = LM.LoadingRetry;
            this.LoadingTimeOut = LM.LoadingTimeOut;
            this.AllowDuplicateDownload = LM.AllowDuplicateDownload;
            this.MaxCacheFilePerFolder = LM.MaxCacheFilePerFolder;
            this.MaxTimeForKeepingFiles = LM.MaxTimeForKeepingFiles;
            this.MinTimeForKeepingFiles = LM.MinTimeForKeepingFiles;
            this.LoadFileMode = LM.LoadFileMode;
        }

        public enum LoadingMode
        {
            /// <summary>
            /// Load the image as texture normally.
            /// </summary>
            Default = 0,

            /// <summary>
            /// Load the image as texture and Keep the image bytes in the Result/CacheItem objects. *This variable has no effect in ProTexturePlayer*
            /// </summary>
            LoadTextureAndKeepData,

            /// <summary>
            /// Load the image bytes into the Result/CacheItem objects, but do not create textures. Which you can decode later (using your own image decoder).
            /// *This variable has no effect in ProTexturePlayer*
            /// </summary>
            LoadFileDataOnly,
        }

        [Tooltip("The behavior for handling Load and Cache files. 'NoCache': do not save and load from the local cache folder; " +
            "'UseCached': download at the first time, use the locally cached file if exist; 'Replace': always download and replace the locally cached file.")]
        public ImageLoader.CacheMode CacheMode = ImageLoader.CacheMode.UseCached;

        [Tooltip("The enum that determines which application path to load and save(cache) file to.")]
        public FilePathName.AppPath CacheDirectoryEnum = FilePathName.AppPath.PersistentDataPath;

        /// <summary>
        /// The root directory for loading and storing(caching) image files.
        /// </summary>
        public string CacheDirectory
        {
            get
            {
                return FilePathName.Instance.GetAppPath(CacheDirectoryEnum);
            }
        }

        /// <summary>
        /// The cache folder path that combined by the (root) CacheDirectory and FolderName.
        /// </summary>
        public string CacheFolderPath
        {
            get
            {
                return string.IsNullOrEmpty(FolderName) ? CacheDirectory : Path.Combine(CacheDirectory, FolderName);
            }
        }

        [Tooltip("The sub-folder under cache directory for loading and storing(caching) files to.")]
        public string FolderName = "";

        [Tooltip("If 'true', for URL start with 'http', the loader will cache the image as per URL address. " +
            "This will bypass the filename generating function to use an MD5 hash(generated based on the URL) as the filename.")]
        public bool CacheAsPerUrl = false;

        [Tooltip("The filename prefix for storing images. The final filename is combined by this prefix, separator, and index together.")]
        public string FileNamePrefix = "Pic";
        [Tooltip("e.g.: .jpg or .png.")]
        public string FileExtension = ".png";
        [Tooltip("Number of Digits for the Index follow the Filename Prefix.")]
        public uint FileIndexFormatDigitsCount = 4;
        [Tooltip("File Name Starting Index. (Set this value to set an offset for the filename index. The default starting index is 0.)")]
        public uint FileNameStartingIndex = 0;
        [Tooltip("Separator text between the File Name and Index. (Please use filename friendly characters only)")]
        public string FileNameAndIndexSeparator = "_";

        [Tooltip("The maximum number of files can be stored(Cached) in the cache folder. ( 0 means no limit )")]
        public uint MaxCacheFilePerFolder = 0;

        [Tooltip("Time duration in seconds for keeping files not being deleted. (eg. 86400 = 3600s * 24h = 1 day.) (Zero means infinite)")]
        public uint MinTimeForKeepingFiles = 0;

        [Tooltip("Time duration in seconds, files must be deleted if the last modify time from now more than this duration. (Zero means infinite)" +
            "\nFor example, set this value = 3600(1 hour), then all files in the folder those modified 1 hour ago will be deleted. ")]
        public uint MaxTimeForKeepingFiles = 0;

        [Tooltip("Number of times to retry when a loading failed. Retry per second until the retry value is Zero.")]
        public uint LoadingRetry = 0;

        [Tooltip("The max time duration for waiting for the download process, stop and kill the loader if time exceeds.")]
        public float LoadingTimeOut = 0;

        [Tooltip("If true, allows downloading the same URL using multiple loaders, else it will not start a new download if that URL is being downloaded." +
            "\n* This flag always 'true' for ProTexturePlayer *")]
        public bool AllowDuplicateDownload = true;

        [Tooltip("Default : load the image as texture normally." +
            "\nLoadTextureAndKeepData : load the image as texture and keep the image bytes in the Result/CacheItem objects." +
            "\nLoadFileDataOnly : load the image bytes into the Result/CacheItem objects, but do not create textures. Which you can decode later (using your own image decoder)." +
            "\n* ProTexturePlayer supports Default mode only.")]
        public LoadingMode LoadFileMode;

        /// <summary>
        /// Check if a specific image exists in the cache directory/folder of this LoaderManagement object.
        /// </summary>
        /// <param name="filename"> The filename of the requesting image, e.g. MyImage001 (optional to provide the file extension) </param>
        public bool HasStorageCache(string filename)
        {
            if (CacheDirectoryEnum == FilePathName.AppPath.StreamingAssetsPath)
            {
                Debug.LogWarning("StreamingAssetsPath is not intended for caching files. Please select PersistentDataPath or TemporaryCachePath instead.");
                return false;
            }

            string filePath = Path.Combine(CacheFolderPath, filename);
            return File.Exists(filePath) || File.Exists(filePath + FileExtension);
        }

        public void SetFileNameFormat(uint fileIndexFormatDigitsCount, uint fileNameStartingIndex = 0, string fileNameAndIndexSeparator = "_")
        {
            FileIndexFormatDigitsCount = fileIndexFormatDigitsCount;
            FileNameStartingIndex = fileNameStartingIndex;
            FileNameAndIndexSeparator = fileNameAndIndexSeparator;
        }

        /// <summary>
        /// Generate a FileName(without extension) base on FileNamePrefix, FileIndexFormatDigitsCount, FileNameStartingIndex, FileNameAndIndexSeparator and file index.
        /// ( e.g. "Pic" + "-" + string format "0000" with fileIndex 12 = "Pic-0012" )
        /// </summary>
        public string GenerateFileName(int fileIndex)
        {
            if (FileIndexFormatDigitsCount <= 0) return FileNamePrefix;
            FileIndexFormatDigitsCount = (uint)Mathf.Clamp(FileIndexFormatDigitsCount, 0, 18);
            string fileIndexFormat = "{0," + FileIndexFormatDigitsCount + ":D" + FileIndexFormatDigitsCount + "}";
            string fileName = FileNamePrefix + FileNameAndIndexSeparator + String.Format(fileIndexFormat, FileNameStartingIndex + fileIndex); // e.g. "Pic" + "-" + string format "0000" with fileIndex 12 = "Pic-0012"
            return fileName;
        }
        public string GenerateFileName(uint fileIndex)
        {
            return GenerateFileName((int)fileIndex);
        }
    }

}
