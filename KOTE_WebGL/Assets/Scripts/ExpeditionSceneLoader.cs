using System;
using map;
using UnityEngine;

namespace DefaultNamespace
{
    public class ExpeditionSceneLoader : MonoBehaviour
    {
        [SerializeField] private GameObject _loader;
        [SerializeField] private Canvas _overlayUI;
        [SerializeField] private PostProcessingTransition _postProcessingTransition;
        
        private void Awake()
        {
            _postProcessingTransition.SetValue(1f);
            _loader.SetActive(true);
            _overlayUI.gameObject.SetActive(false);
            
            GameManager.Instance.EVENT_ALL_MAP_NODES_UPDATE.AddListener((_) => HideLoader());
        }

        private void HideLoader()
        {
            _loader.SetActive(false);
            _overlayUI.gameObject.SetActive(true);
            _postProcessingTransition.EndTransition();
        }
    }
}