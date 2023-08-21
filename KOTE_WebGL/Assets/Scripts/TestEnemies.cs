using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DefaultNamespace
{
    public class TestEnemies : MonoBehaviour
    {
        [SerializeField] private EnemiesManager _enemiesManager;

        [SerializeField] private string[] enemiesToTry;
        
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
    }
}