using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
    [CreateAssetMenu(fileName = "EnemiesConfig", menuName = "ScriptableObjects/Enemies", order = 0)]
    public class EnemiesConfig : ScriptableObject
    {
        public List<string> enemiesNames;


        public string GetEnemy(string enemyName)
        {
            enemyName = enemyName.ToLower().Replace(" ", "").Trim();
            if (!enemiesNames.Contains(enemyName))
                enemyName = "sporemonger";
            Debug.Log($"enemyType: {enemyName}");
            
            return enemyName;
        }
    }
}