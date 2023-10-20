using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Combat.VFX;
using TMPro;
using UnityEngine;

namespace DefaultNamespace
{
    public class TestEnemies : MonoBehaviour
    {
        [SerializeField] private EnemiesManager _enemiesManager;

        [SerializeField] private string[] enemiesToTry;
        [SerializeField] private VFX vfxToTest;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private VFXList vfxes;

        private void Start()
        {
            _enemiesManager.OnEnemiesUpdate(new EnemiesData()
            {
                data = enemiesToTry.ToDictionary(k => k, v => new EnemyData()
                {
                    name = v,
                    id = Guid.NewGuid().ToString(),
                    size = "medium",
                    type = v,
                    hpCurrent = 10,
                    hpMax = 10,
                }).Values.ToList()
            });
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
                TestVFXes();
            if (Input.GetKeyDown(KeyCode.S))
                TestVFX();
        }

        [ContextMenu("Test")]
        private void TestVFX()
        {
            var enemyManager = _enemiesManager.enemies[0].GetComponent<EnemyManager>();
            var vfx = enemyManager.vfxList.GetVFX(vfxToTest);
            vfx?.Play(
                enemyManager.spine,
                enemyManager.spine.TryGetComponent<Animator>(out var animator) ? animator : enemyManager.spine.gameObject.AddComponent<Animator>(),
                enemyManager.spine.GetComponent<MeshRenderer>());
        }
        
        [ContextMenu("TestVfxes")]
        private void TestVFXes()
        {
            var enemyManager = _enemiesManager.enemies[0].GetComponent<EnemyManager>();

            StartCoroutine(Test());
            IEnumerator Test()
            {
                foreach (var vfxPair in vfxes.vfxPairs)
                {
                    var vfx = enemyManager.vfxList.GetVFX(vfxPair.name);
                    vfx.Play(
                        enemyManager,
                        enemyManager.spine.TryGetComponent<Animator>(out var animator) ? animator : enemyManager.spine.gameObject.AddComponent<Animator>(),
                        enemyManager.spine.GetComponent<MeshRenderer>());
                    text.text = $"VFX: {vfxPair.name}";
                    yield return new WaitForSeconds(2.5f);
                }

                text.text = "";
            }
        }
    }
}