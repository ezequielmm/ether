using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace map.Clouds
{
    public class Cloud : MonoBehaviour
    {
        [SerializeField] private GameObject _graphic;
        [SerializeField] private GameObject _graphicShadow;
        
        private SpriteRenderer _spriteRenderer;
        private SpriteRenderer _spriteRendererShadow;
        private BoxCollider2D _maskCollider;

        public event Action<Cloud> OnEndLifeTime;
        
        public void Init(BoxCollider2D maskCollider)
        {
            _spriteRenderer = _graphic.GetComponent<SpriteRenderer>();
            _spriteRendererShadow = _graphicShadow.GetComponent<SpriteRenderer>();
            _maskCollider = maskCollider;
        }
        
        public void Run(Sprite sprite, float speed)
        {
            _spriteRenderer.sprite = sprite;
            _spriteRendererShadow.sprite = sprite;
            
            var startPosition = GetRandomPointOnRightEdgeOfCollider();
            var randOffset = Random.Range(10f, 30f);
            startPosition.x += _spriteRenderer.bounds.extents.x + randOffset;
            //startPosition += new Vector2(_spriteRenderer.bounds.extents.x + randOffset, 0);
            
            transform.position = startPosition;
            _graphic.SetActive(true);

            StartCoroutine(Cross(speed));
        }
        
        public void Hide()
        {
            _graphic.SetActive(false);
            StopAllCoroutines();
        }

        IEnumerator Cross(float speed)
        {
            while (true)
            {
                var spriteRightLimit = transform.position + new Vector3(_spriteRenderer.bounds.extents.x, 0, 0);
                if (PassTheLeftEdge(spriteRightLimit)) {
                    OnEndLifeTime?.Invoke(this);
                    break;
                }
                
                transform.Translate(Vector2.left * (speed * Time.deltaTime));
                yield return null;
            }
        }

        private Vector2 GetRandomPointOnRightEdgeOfCollider()
        {
            var size = _maskCollider.size;
            var boundsMin = _maskCollider.offset - size * 0.5f;
            var boundsMax = _maskCollider.offset + size * 0.5f;
            
            var rightEdgeX = boundsMax.x;
            
            var randomY = Random.Range(boundsMin.y, boundsMax.y);
            
            var randomPointLocal = new Vector2(rightEdgeX, randomY);
            var randomPointWorld = _maskCollider.transform.TransformPoint(randomPointLocal);

            return randomPointWorld;
        }

        private bool PassTheLeftEdge(Vector2 point)
        {
            var size = _maskCollider.size;
            var boundsMin = _maskCollider.offset - size * 0.5f;

            return point.x < boundsMin.x;
        }
    }
}