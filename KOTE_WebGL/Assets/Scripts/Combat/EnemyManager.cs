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
            GameManager.Instance.EVENT_PLAY_SFX.Invoke("Defence Block");
        }
        if (current.defense <= 0 && old.hpCurrent > current.hpCurrent) // Damage Taken no armor
        {
            // Play Attack audio
            // Can be specific, but we'll default to "Attack"
            GameManager.Instance.EVENT_PLAY_SFX.Invoke("Attack");
        }
        if (current.defense > old.defense) // Defense Buffed
        {
            // Play Metallic Ring
            GameManager.Instance.EVENT_PLAY_SFX.Invoke("Defence Up");
        }
        if (current.hpCurrent > old.hpCurrent) // Healed!
        {
            // Play Rising Chimes
            GameManager.Instance.EVENT_PLAY_SFX.Invoke("Heal");
        }
    }

    private void SetDefense()
    {
        defenseTF.SetText(enemyData.defense.ToString());
    }

    private void Start()
    {
        GameManager.Instance.EVENT_UPDATE_ENEMY.AddListener(OnUpdateEnemy);
        Debug.Log($"[Enemy Manager] Enemy ID: {enemyData.id}");
        GameManager.Instance.EVENT_PLAY_ENEMY_ATTACK.AddListener(onAttack);

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
                // TODO: Replace magic number with actual timing from the knight's animation.
                // Note: The knight's animation may be differently timed depending on the animation.
                StartCoroutine(OnHit(0.45f));
            }
            else
            {
                firstAttack = false;
            }
        }       

       
    }

    private void onAttack(int enemyId) 
    {
        //if (enemyData.enemyId == enemyId)
            Attack();
    }

    private void Attack() 
    {
        Debug.Log("+++++++++++++++[Enemy]Attack");

        spine.PlayAnimationSequence("Attack");
        spine.PlayAnimationSequence("Idle");
    }

    private IEnumerator OnHit(float hitTiming = 0)
    {
        yield return new WaitForSeconds(hitTiming);
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

    private void OnMouseEnter()
    {
        // Tooltip On
        List<Tooltip> tooltips = new List<Tooltip>() { new Tooltip() { title = "test", description = "The End Is Never The End Is Never" } };

        GameManager.Instance.EVENT_SET_TOOLTIPS.Invoke(tooltips, TooltipController.Anchor.MiddleLeft, Vector3.zero, null);
    }
    private void OnMouseExit()
    {
        // Tooltip Off
        GameManager.Instance.EVENT_CLEAR_TOOLTIPS.Invoke();
    }
}
