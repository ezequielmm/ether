using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace.Common
{

    [ExecuteAlways]
    public class UIScaler : MonoBehaviour
    {
        private CanvasScaler canvasScaler;

        private void Start()
        {
            canvasScaler ??= GetComponent<CanvasScaler>();
        }

        private void Update()
        {
            var x = canvasScaler.referenceResolution.x; // 1090
            var y = canvasScaler.referenceResolution.y; // 1080
            canvasScaler.matchWidthOrHeight = Screen.width / Screen.height > x/y ? 1 : 0;
        }
    }
}