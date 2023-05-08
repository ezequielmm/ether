using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace DefaultNamespace
{
    public class Loader : MonoBehaviour
    {
        [SerializeField] private GameObject _graphic;
        [SerializeField] private List<TrailRenderer> trails;
        [SerializeField] private bool _showOnStart;

        [SerializeField] private AnimationCurve showCurve;
        [SerializeField] private AnimationCurve hideCurve;
        
        [SerializeField] float lerpDuration = 1f;

        public UnityEvent OnLoaderShow;
        
        private void Awake()
        {
            // GameManager.Instance.EVENT_SCENE_LOADING.AddListener(Show);
            GameManager.Instance.EVENT_ALL_MAP_NODES_UPDATE.AddListener((_) => Hide());
            (_showOnStart ? (Action/*™*/)Show : Hide)();
        }

        [ContextMenu("Show")]
        public void Show()
        {
            trails.ForEach(e => e.emitting = true);

            StopAllCoroutines();
            StartCoroutine(ShowRoutine());
            IEnumerator ShowRoutine()
            {
                yield return GraphicRoutine(5, showCurve);
                yield return new WaitForSeconds(3f);
                OnLoaderShow?.Invoke();
            }
        }
        IEnumerator GraphicRoutine(float finalVal, AnimationCurve curve)
        {
            var t = 0f;
            var startVal = trails[0].time;
            while (t < lerpDuration)
            {
                t += Time.deltaTime;
                var val = Mathf.Lerp(startVal, finalVal, curve.Evaluate(t / lerpDuration));
                trails.ForEach(e => e.time = val);
                yield return null;
            }
        }
        
        [ContextMenu("Hide")]
        public void Hide()
        {
            trails.ForEach(e => e.emitting = false);
            
            StopAllCoroutines();
            StartCoroutine(GraphicRoutine(0, hideCurve));
        }
    }
}