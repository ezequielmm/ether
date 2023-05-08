using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.PostProcessing;

namespace map
{
    public class PostProcessingTransition : MonoBehaviour
    {
        [SerializeField] private PostProcessVolume _volume;
        [SerializeField] private float _duration = 1f;

        public UnityEvent OnTransitionStart;
        public UnityEvent OnTransitionInEnd;
        public UnityEvent OnTransitionOut;
        
        public void SetDuration(float duration)
        {
            _duration = duration;
        }
        public void StartTransition()
        {
            StopAllCoroutines();
            StartCoroutine(StartRoutine());
            IEnumerator StartRoutine()
            {
                OnTransitionStart?.Invoke();
                yield return LerpPostProcessing(1);
                OnTransitionInEnd?.Invoke();
            }
        }
        public void EndTransition()
        {
            StopAllCoroutines();
            StartCoroutine(StartRoutine());
            IEnumerator StartRoutine()
            {
                yield return LerpPostProcessing(0);
                OnTransitionOut?.Invoke();
            }
        }
        
        private IEnumerator LerpPostProcessing(float finalVal)
        {
            var t = 0f;
            var startVal = _volume.weight;
            
            while (t < _duration)
            {
                t += Time.deltaTime;
                SetValue(Mathf.Lerp(startVal, finalVal, t / _duration));
                yield return null;
            }

            SetValue(finalVal);
        }

        public void SetValue(float value)
        {
            _volume.weight = value;
        }
    }
}