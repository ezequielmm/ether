using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Combat
{
    public class UltiFeedback : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI testText;
        [SerializeField] private Animator anim;
        
        public void DoFeedback(string actionName, Vector3 textPosition, GameObject enemy)
        {
            testText.text = actionName;
            
            StartCoroutine(Delay());
            IEnumerator Delay()
            {
                yield return new WaitForSeconds(.2f);
                anim.Play("Default");
                anim.Play("UltiFeedback");
                
                StartCoroutine(Lerper(.2f, value => Time.timeScale = Mathf.Lerp(1, .4f, value)));

                yield return new WaitForSecondsRealtime(1.2f);
                Debug.Log("[ULTIFEEDBACk] Fade out");
                // Lerp time to end
                StartCoroutine(Lerper(.5f, value => Time.timeScale = Mathf.Lerp(0.4f, 1f, value)));
            }
        }

        private IEnumerator Lerper(float duration, Action<float> onValue)
        {
            var t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                onValue(t / duration);
                yield return null;
            }
            t = duration;
            onValue(t / duration);
        }
    }
}