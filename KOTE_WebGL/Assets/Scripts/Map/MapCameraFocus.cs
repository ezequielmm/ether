using System.Linq;
using Cinemachine;
using UnityEngine;

namespace map
{
    public class MapCameraFocus : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera zoomVCamera;
        [SerializeField] private CinemachineBrain brain;

        private MapSpriteManager mapSpriteManager;
        [SerializeField] private PostProcessingTransition postProcessingTransition;

        private int _currentNode;
        
        private void Start()
        {
            zoomVCamera.gameObject.SetActive(false);
            GameManager.Instance.EVENT_MAP_NODE_SELECTED.AddListener(SelectNode);
            GameManager.Instance.EVENT_TOGGLE_COMBAT_ELEMENTS.AddListener((val) => { if (val) EndTransition(); });
            GameManager.Instance.EVENT_TOOGLE_TREASURE_ELEMENTS.AddListener((val) => { if (val) EndTransition(); });
            GameManager.Instance.EVENT_TOGGLE_MERCHANT_PANEL.AddListener((val) => { if (val) EndTransition(); });
            GameManager.Instance.EVENT_SHOW_ENCOUNTER_PANEL.AddListener(EndTransition);
            // TODO: This class controls the flow of the game, it's not the ideal solution
            postProcessingTransition.OnTransitionInEnd.AddListener(() => GameManager.Instance.OnNodeTransitionEnd?.Invoke(_currentNode));

            mapSpriteManager = FindObjectOfType<MapSpriteManager>();
        }

        private void EndTransition()
        {
            postProcessingTransition.EndTransition();
            zoomVCamera.gameObject.SetActive(false);
        }

        private void SelectNode(int nodeId)
        {
            _currentNode = nodeId;
            var node = FindObjectsOfType<NodeData>().First(e => e.id == nodeId);

            if (node.type == NODE_TYPES.camp || node.type == NODE_TYPES.royal_house || node.type == NODE_TYPES.portal)
            {
                GameManager.Instance.OnNodeTransitionEnd?.Invoke(_currentNode);
                return;
            }
            
            mapSpriteManager.GoToNode(node.transform);
            zoomVCamera.Follow = node.transform;
            zoomVCamera.gameObject.SetActive(true);

            postProcessingTransition.SetDuration(brain.m_DefaultBlend.BlendTime);
            postProcessingTransition.StartTransition();
        }
    }
}