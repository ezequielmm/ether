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
    public TMP_Text healthTF;
    public TMP_Text defenseTF;
    public GameObject activeEnemy;

    private SpineAnimationsManagement spine;

    public EnemyData EnemyData { 
        set
        {
            enemyData = ProcessNewData(enemyData, value);
        }
        get
        {
            return enemyData;
        }
    }

    private EnemyData ProcessNewData(EnemyData old, EnemyData current)
    {
        if (old == null)
        {
            SetDefense(current.defense);
            SetHealth(current.hpCurrent, current.hpMax);
            return current;
        }

        bool isAttack = false;

        int hpDelta = current.hpCurrent - old.hpCurrent;
        int defenseDelta = current.defense - old.defense;

        if (defenseDelta > 0) // Defense Buffed
        {
            // Play Metallic Ring
            GameManager.Instance.EVENT_PLAY_SFX.Invoke("Defense Up");
        }
        if (hpDelta > 0) // Healed!
        {
            // Play Rising Chimes
            GameManager.Instance.EVENT_PLAY_SFX.Invoke("Heal");
        }

        //// This will add to the Event Queue. 
        //if (hpDelta < 0 || defenseDelta < 0)
        //{
        //    //isAttack = true;
        //    //var targets = new List<CombatTurnData.Target>();
        //    //// We will need an "Attack" acction to handle multiple targets
        //    //targets.Add(new CombatTurnData.Target(enemyData.id, hpDelta, current.hpCurrent, defenseDelta, current.defense));
        //    //var attack = new CombatTurnData("player", targets, 0); // player attacks target. Happens right away!
        //    //GameManager.Instance.EVENT_COMBAT_TURN_ENQUEUE.Invoke(attack);
        //}


        if (isAttack == false)
        {
            SetDefense(current.defense);
            SetHealth(current.hpCurrent, current.hpMax);
        }
        return current;
    }

    private void OnAttackRequest(CombatTurnData attack)
    {
        // TODO: Ensure that the player sets the correct enemy when attacked.
        if (attack.originId != enemyData.id) return;

        Debug.Log($"[EnemyManager] Combat Request GET!");

        bool endCalled = false;
        foreach (var target in attack.targets)
        {
            // Run Attack Animation Or Status effects
            if (target.defenseDelta != 0 || target.healthDelta != 0)
            {
                // Run Attack
                Attack();
                endCalled = true;
                RunAfterTime(0.9f, // hard coded enemy animation attack point
                    () => GameManager.Instance.EVENT_ATTACK_RESPONSE.Invoke(attack));
            }
        }
        if (!endCalled)
        {
            // If no conditions are met, close the event
            GameManager.Instance.EVENT_COMBAT_TURN_END.Invoke(attack.attackId);
        }
    }
    private void OnAttackResponse(CombatTurnData attack)
    {
        var target = attack.GetTarget(enemyData.id);
        if (target == null) return;

        Debug.Log($"[EnemyManager] Combat Response GET!");

        float waitDuration = 0;
        if (target.defenseDelta < 0 && target.healthDelta >= 0) // Hit and defence didn't fall or it did and no damage
        {
            // Play Armored Clang
            GameManager.Instance.EVENT_PLAY_SFX.Invoke("Defense Block");
        }
        else if (target.healthDelta < 0) // Damage Taken no armor
        {
            // Play Attack audio
            // Can be specific, but we'll default to "Attack"
            GameManager.Instance.EVENT_PLAY_SFX.Invoke("Attack");
            waitDuration += OnHit();
        }
        SetDefense(target.finalDefense);
        SetHealth(target.finalHealth);

        // You can add status effect changes in here as well**

        RunAfterTime(waitDuration, () => GameManager.Instance.EVENT_COMBAT_TURN_END.Invoke(attack.attackId));
    }

    private void SetDefense(int? value = null)
    {
        if (value == null)
        {
            value = enemyData.defense;
        }
        defenseTF.SetText(value.ToString());
    }

    private void Start()
    {
        GameManager.Instance.EVENT_UPDATE_ENEMY.AddListener(OnUpdateEnemy);
        GameManager.Instance.EVENT_ATTACK_REQUEST.AddListener(OnAttackRequest);
        GameManager.Instance.EVENT_ATTACK_RESPONSE.AddListener(OnAttackResponse);

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

    public void SetHealth(int? current = null, int? max = null)
    {
        if (current == null)
        {
            current = enemyData.hpCurrent;
        }
        if (max == null)
        {
            max = enemyData.hpMax;
        }
        Debug.Log($"[EnemyManager] Health: {current}/{max}");

        healthTF.SetText($"{current}/{max}");

        healthBar.maxValue = max.Value;

        if (healthBar.value != current)
        {
            hitPS.Play();
            healthBar.DOValue(current.Value, 1).OnComplete(CheckDeath);
        }
    }

    private float Attack() 
    {
        Debug.Log("+++++++++++++++[Enemy]Attack");

        float length = spine.PlayAnimationSequence("Attack");
        spine.PlayAnimationSequence("Idle");
        return length;
    }

    private float OnHit()
    {
        float length = spine.PlayAnimationSequence("Hit");
        spine.PlayAnimationSequence("Idle");
        return length;

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

    private void RunAfterTime(float time, Action toRun)
    {
        StartCoroutine(runCoroutine(time, toRun));
    }
    private IEnumerator runCoroutine(float time, Action toRun)
    {
        yield return new WaitForSeconds(time);
        toRun.Invoke();
    }
}