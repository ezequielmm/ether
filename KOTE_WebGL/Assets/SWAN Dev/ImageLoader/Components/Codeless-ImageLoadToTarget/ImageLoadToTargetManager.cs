/// <summary>
/// Created by SWAN DEV 2023
/// </summary>
using System.Collections.Generic;
using UnityEngine;

namespace IMBX
{
    public class ImageLoadToTargetManager : MonoBehaviour
    {
        private static List<ImageLoadToTargetHandler> _handlers = new List<ImageLoadToTargetHandler>();

        private static ImageLoadToTargetManager _instance;
        public static ImageLoadToTargetManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("[ImageLoadToTargetManager]").AddComponent<ImageLoadToTargetManager>();
                }
                return _instance;
            }
        }

        [Tooltip("Max number of ImageLoaders to use concurrently.")]
        [Range(1, 8)] public int m_MaxLoaderNum = 2;

        public void AddHandler(ImageLoadToTargetHandler handler)
        {
            if (!_handlers.Contains(handler))
            {
                handler.STATE = ImageLoadToTargetHandler.State.Waiting;
                _handlers.Add(handler);
                gameObject.SetActive(true);
            }
        }

        private void Awake()
        {
            if (_instance == null) _instance = this; else Destroy(gameObject);
        }

        private void Update()
        {
            int hCnt = _handlers.Count;
            if (hCnt > 0 && ImageLoader.LoadingCount < m_MaxLoaderNum)
            {
                for (int i = 0; i < _handlers.Count; i++)
                {
                    var h = _handlers[i];
                    if (h.STATE == ImageLoadToTargetHandler.State.Waiting)
                    {
                        h.STATE = ImageLoadToTargetHandler.State.ReadyToLoad;
                        h.Load();
                        _handlers.Remove(h);
                        break;
                    }
                }
            }
            else if (hCnt == 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
