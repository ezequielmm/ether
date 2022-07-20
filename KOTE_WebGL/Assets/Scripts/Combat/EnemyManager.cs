using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class EnemyManager : MonoBehaviour
{
    private EnemyData enemyData;
    public ParticleSystem hitPS;
    public ParticleSystem explodePS;
    public Slider healthBar;
    private bool firstAttack = true;
    public TMP_Text healthTF;
    public TMP_Text defenseTF;

    public EnemyData EnemyData { 
        set
        {
            enemyData = value;
            SetDefense();
            SetHealth();
        }
        get
        {
            return enemyData;
        }
    }

    private void ProcessNewData(EnemyData old, EnemyData current) 
    {
        if (old == null || current == null)
        {
            return;
        }
        if (old.defense > current.defense && (current.defense > 0 ||
            (current.defense == 0 && old.hpCurrent == current.hpCurrent))) // Hit and defence didn't fall or it did and no damage
        {
            // Play Armored Clang
            GameManager.Instance.EVENT_PLAY_SFX.Invoke("Armored Clang");
        }
        if (current.defense <= 0 && old.hpCurrent > current.hpCurrent) // Damage Taken no armor
        {
            // Play Attack audio
            // Can be specific, but we'll default to "Attack"
            GameManager.Instance.EVENT_PLAY_SFX.Invoke("Attack");
        }
    }

    private void SetDefense()
    {
        defenseTF.SetText(enemyData.defense.ToString());
    }

    private void Start()
    {
        GameManager.Instance.EVENT_UPDATE_ENEMY.AddListener(OnUpdateEnemy);
    }

    private void OnUpdateEnemy(EnemyData newEnemyData)
    {
        if (newEnemyData.enemyId == enemyData.enemyId)
        {
            // healthBar.DOValue(newEnemyData.hpMin, 1);
            ProcessNewData(EnemyData, newEnemyData);
            EnemyData = newEnemyData;
        }
    }

    public void SetHealth()
    {
        Debug.Log("[SetHealth]min="+enemyData.hpCurrent + "/"+enemyData.hpMax);

        healthTF.SetText(enemyData.hpCurrent + "/" + enemyData.hpMax);

        healthBar.maxValue = enemyData.hpMax;

        if (healthBar.value != enemyData.hpCurrent)
        {
            hitPS.Play();
            healthBar.DOValue(enemyData.hpCurrent, 1).OnComplete(CheckDeath);

            if (!firstAttack)
            {
                Debug.Log("----------invoking attack play");
                GameManager.Instance.EVENT_PLAY_PLAYER_ATTACK.Invoke();

            }
            else
            {
                firstAttack = false;
            }
        }       

       
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
