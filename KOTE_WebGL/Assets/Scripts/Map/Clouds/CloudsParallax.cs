using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace map.Clouds
{
    public class CloudsParallax : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _startPlayDelay = 2f;
        [SerializeField] private int _cloudsPoolAmount = 8;
        [SerializeField] private int _cloudsSimultaneous = 3;
        [SerializeField] private float _minRandomSpeed;
        [SerializeField] private float _maxRandomSpeed;
        [SerializeField] private float _minRandomNewCloudDelay;
        [SerializeField] private float _maxRandomNewCloudDelay;
        [Space]
        [SerializeField] private BoxCollider2D _maskCollider;
        [SerializeField] private Cloud _cloudPrefab;

        [SerializeField] private Sprite[] _cloudSprites;

        private List<int> testList;

        private readonly Queue<Cloud> _cloudsPool = new();
        private Queue<int> _spriteIndexQueue = new();

        private Coroutine initializeCloudsRoutine;
        
        private void Awake()
        {
            GameManager.Instance.EVENT_ALL_MAP_NODES_UPDATE.AddListener((a) =>
            {
                StartCoroutine(Initialize());
            });
        }
        
        private IEnumerator Initialize()
        {
            // clear all clouds, just in case
            foreach (var cloud in _cloudsPool) {
                Destroy(cloud.gameObject);
            }
            _cloudsPool.Clear();
            
            GenerateClouds();
            yield return new WaitForSeconds(_startPlayDelay);
            
            for (int i = 0; i < _cloudsSimultaneous; i++)
                LaunchCloud();
        }

        private void GenerateClouds()
        {
            for (int i = 0; i < _cloudsPoolAmount; i++)
            {
                var cloud = Instantiate(_cloudPrefab, transform);
                cloud.Init(_maskCollider);
                cloud.Hide();

                cloud.OnEndLifeTime += CloudEndLifeTime;

                _cloudsPool.Enqueue(cloud);
            }
        }

        private void LaunchCloud()
        {
            // Pick a Cloud from queue
            var cloud = _cloudsPool.Dequeue();
            var randomSprite = _cloudSprites[GetRandomSpriteIndex()];
            var randomSpeed = Random.Range(_minRandomSpeed, _maxRandomSpeed);
            
            cloud.Run(randomSprite, randomSpeed);
        }

        private int GetRandomSpriteIndex()
        {
            if (_spriteIndexQueue.Count <= 0)
                GenerateIndexPool();
            return _spriteIndexQueue.Dequeue();
        }

        private void GenerateIndexPool()
        {
            for (int i = 0; i < _cloudSprites.Length; i++)
                _spriteIndexQueue.Enqueue(i);
            _spriteIndexQueue = new Queue<int>(_spriteIndexQueue.OrderBy(x => Random.value > .5f));
        }

        private void CloudEndLifeTime(Cloud cloud)
        {
            // Return the cloud to the queue
            cloud.Hide();
            _cloudsPool.Enqueue(cloud);

            StartCoroutine(Delay());
            IEnumerator Delay()
            {
                yield return new WaitForSeconds(Random.Range(_minRandomNewCloudDelay, _maxRandomNewCloudDelay));
                LaunchCloud();
            }
        }
    }
}