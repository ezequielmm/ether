using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EnemyManager : MonoBehaviour
{
    private Enemy enemyData;

    public Enemy EnemyData { 
        set
        {
            enemyData = value;
            SetHealth();
        }
        get
        {
            return enemyData;
        }
                            }

    public ParticleSystem HitPS;
    public Slider healthBar;

    private void Start()
    {
        GameManager.Instance.EVENT_UPDATE_ENEMY.AddListener(OnUpdateEnemy);
    }

    private void OnUpdateEnemy(Enemy newEnemyData)
    {
        if (newEnemyData.enemyId == enemyData.enemyId)
        {
            HitPS.Play();

            // healthBar.DOValue(newEnemyData.hpMin, 1);
            EnemyData = newEnemyData;
        }
    }

    public void SetHealth()
    {
        Debug.Log("[SetHealth]min="+enemyData.hpMin+"/"+enemyData.hpMax);
        healthBar.maxValue = enemyData.hpMax;
        healthBar.DOValue(enemyData.hpMin, 1);
    }
}
