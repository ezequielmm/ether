using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace.UI
{
    public class TipsContent
    {
        public string[] tips;
    }
    
    public class TipsText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI tipText;

        private void Start()
        {
            SetTip();
        }

        private void SetTip()
        {
            StartCoroutine(
                RequestService.Instance.GetRequestCoroutine<TipsContent>(
                    $"{ClientEnvironmentManager.GetStreamingAssetsPath()}/tips.json",
                    (tipsContent) =>
                    {
                        tipText.text = $"Tip: {tipsContent.tips[Random.Range(0, tipsContent.tips.Length)]}";
                    }
                )
            );
        }
    }
}