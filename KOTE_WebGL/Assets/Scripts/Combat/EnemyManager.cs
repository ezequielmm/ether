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

    public ParticleSystem hitPS;
    public ParticleSystem explodePS;
    public Slider healthBar;

    private bool firstAttack = true;

    private void Start()
    {
        GameManager.Instance.EVENT_UPDATE_ENEMY.AddListener(OnUpdateEnemy);
    }

    private void OnUpdateEnemy(Enemy newEnemyData)
    {
        if (newEnemyData.enemyId == enemyData.enemyId)
        {
            hitPS.Play();

            // healthBar.DOValue(newEnemyData.hpMin, 1);
            EnemyData = newEnemyData;
        }
    }

    public void SetHealth()
    {
        Debug.Log("[SetHealth]min="+enemyData.hpCurrent + "/"+enemyData.hpMax);

        if (!firstAttack) {
            Debug.Log("----------invoking attack play");
            GameManager.Instance.EVENT_PLAY_PLAYER_ATTACK.Invoke();

        } else
        {
            firstAttack = false;
        }
       
        healthBar.maxValue = enemyData.hpMax;
        healthBar.DOValue(enemyData.hpCurrent, 1).OnComplete(CheckDeath);

       
    }

    private void CheckDeath()
    {
        if (enemyData.hpCurrent < 1)
        {
            explodePS.transform.parent = null;
            explodePS.Play();
            Destroy(explodePS.gameObject, 2);
            Destroy(this.gameObject);
        }
    }
}
