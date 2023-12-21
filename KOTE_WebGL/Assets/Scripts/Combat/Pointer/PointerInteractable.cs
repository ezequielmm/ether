using UnityEngine;
using UnityEngine.Events;

namespace Combat.Pointer
{
    public class PointerInteractable : MonoBehaviour
    {
        [SerializeField] private GameObject target;

        public GameObject Target
        {
            get
            {
                if (target) return target;
                
                if (transform.parent.TryGetComponent<EnemyManager>(out var enemy))
                    return enemy.gameObject;
                if (transform.parent.TryGetComponent<PlayerManager>(out var player))
                    return player.gameObject;
                
                return null;
            }
        }

        public UnityEvent onCursorEnter;
        public UnityEvent onCursorExit;
        
        private void OnMouseEnter()
        {
            onCursorEnter.Invoke();
        }
        private void OnMouseExit()
        {
            onCursorExit.Invoke();
        }
    }
}