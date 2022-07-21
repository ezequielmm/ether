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
    public GameObject activeEnemy;

    private SpineAnimationsManagement spine;

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

    private void SetDefense()
    {
        defenseTF.SetText(enemyData.defense.ToString());
    }

    private void Start()
    {
        GameManager.Instance.EVENT_UPDATE_ENEMY.AddListener(OnUpdateEnemy);

        // Grab first spine animation management script we find. This is a default. We'll set this when spawning the enemy usually.
        if (activeEnemy == null)
        {
            activeEnemy = GetComponentInChildren<SpineAnimationsManagement>()?.gameObject;
            if (activeEnemy == null) 
            {
                Debug.Log($"[Enemy Manager] Could not find enemy animation");
            }
        }
        spine = activeEnemy.GetComponent<SpineAnimationsManagement>();
        spine.PlayAnimationSequence("Idle");
    }

    private void OnUpdateEnemy(EnemyData newEnemyData)
    {
        if (newEnemyData.enemyId == enemyData.enemyId)
        {
            // healthBar.DOValue(newEnemyData.hpMin, 1);
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
                OnHit();
            }
            else
            {
                firstAttack = false;
            }
        }       

       
    }

    private void OnHit()
    {
        spine.PlayAnimationSequence("Hit");
        spine.PlayAnimationSequence("Idle");

    }

    private void CheckDeath()
    {
        if (enemyData.hpCurrent < 1)
        {
            explodePS.transform.parent = null;
            explodePS.Play();
            Destroy(explodePS.gameObject, 2);
            spine.PlayAnimationSequence("Death");
            Destroy(this.gameObject,2);
        }
    }
}
