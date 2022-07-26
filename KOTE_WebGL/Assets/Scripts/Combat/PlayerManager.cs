using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    public SpineAnimationsManagement spineAnimationsManagement;
    public TMP_Text defenseTF;
    public TMP_Text healthTF;
    public Slider healthBar;

    private PlayerData playerData;
    public PlayerData PlayerData
    {
        set
        {
            playerData = ProcessNewData(playerData, value);
        }
        get
        {
            return playerData;
        }
    }

    private void OnAttackRequest(CombatTurnData attack) 
    {
        if(attack.origin != "player") return;

        Debug.Log($"[PlayerManager] Combat Request GET!");

        bool endCalled = false;
        foreach (var target in attack.targets) {
            // Run Attack Animation Or Status effects
            if (target.defenseDelta != 0 || target.healthDelta != 0)
            {
                // Run Attack
                Attack();
                endCalled = true;
                RunAfterTime(0.45f, // hard coded player animation attack point
                    () => GameManager.Instance.EVENT_ATTACK_RESPONSE.Invoke(attack));
            }
        }
        if (!endCalled)
        {
            // If no conditions are met, close the event
            GameManager.Instance.EVENT_COMBAT_TURN_END.Invoke();
        }
    }
    private void OnAttackResponse(CombatTurnData attack) 
    {
        var target = attack.GetTarget("player");
        if (target == null) return;

        Debug.Log($"[PlayerManager] Combat Response GET!");

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
        SetDefense();
        SetHealth();

        // You can add status effect changes in here as well**

        RunAfterTime(waitDuration, () => GameManager.Instance.EVENT_COMBAT_TURN_END.Invoke());
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

    private PlayerData ProcessNewData(PlayerData old, PlayerData current)
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

        // This will add to the Event Queue. 
        if (hpDelta < 0 || defenseDelta < 0) 
        {
            isAttack = true;
            var targets = new List<CombatTurnData.Target>();
            targets.Add(new CombatTurnData.Target("player", hpDelta, defenseDelta));
            // The player won't know who hit them
            var attack = new CombatTurnData("unknown", targets);

            GameManager.Instance.EVENT_COMBAT_TURN_ENQUEUE.Invoke(attack);
        }
        

        if (isAttack == false) 
        {
            SetDefense();
            SetHealth();
        }
        return current;
    }

    private void SetHealth(int? current = null, int? max = null)
    {
        if (current == null) 
        {
            current = playerData.hpCurrent;
        }
        if (max == null) 
        {
            max = playerData.hpMax;
        }
        Debug.Log($"[PlayerManager] Health: {current}/{max}");

        healthTF.SetText($"{current}/{max}");

        healthBar.maxValue = max.Value;

        if (healthBar.value != current)
        {
            healthBar.DOValue(current.Value, 1).OnComplete(CheckDeath);
        }
    }

    private void Start()
    {
        GameManager.Instance.EVENT_ATTACK_REQUEST.AddListener(OnAttackRequest);
        GameManager.Instance.EVENT_ATTACK_RESPONSE.AddListener(OnAttackResponse);
        GameManager.Instance.EVENT_UPDATE_PLAYER.AddListener(OnUpdatePlayer);
        GameManager.Instance.EVENT_WS_CONNECTED.AddListener(OnWSConnected);
        GameManager.Instance.EVENT_UPDATE_ENERGY.AddListener(OnUpdateEnergy);


        spineAnimationsManagement = GetComponent<SpineAnimationsManagement>();
        //spineAnimationsManagement.SetSkin("weapon/sword");
        spineAnimationsManagement.PlayAnimationSequence("Idle");

    }

    private void OnEnable()
    {
        GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.Players);
    }

    private void OnUpdateEnergy(int currentEnergy, int maxEnergy) 
    {
        if (currentEnergy == 0) 
        {
            // Out of energy audio
            GameManager.Instance.EVENT_PLAY_SFX.Invoke("Out Of Energy");
        }
    }

    private void OnWSConnected()
    {

    }

    private void OnUpdatePlayer(PlayerData newPlayerData)
    {
        PlayerData = newPlayerData;
    }

    private void SetDefense(int? value = null)
    {
        if (value == null) 
        {
            value = playerData.defense;
        }
        defenseTF.SetText(value.ToString());
    }


    private void OnMouseDown()
    {
        Attack();
    }

    public float Attack()
    {
        Debug.Log("+++++++++++++++[Player]Attack");
        float length = spineAnimationsManagement.PlayAnimationSequence("Attack");
        spineAnimationsManagement.PlayAnimationSequence("Idle");
        return length;
    }

    private float OnHit()
    {
        float length = spineAnimationsManagement.PlayAnimationSequence("Hit");
        spineAnimationsManagement.PlayAnimationSequence("Idle");
        return length;
    }

    private void OnDeath() 
    {
        // TODO: Add Death Animation
    }

    private void CheckDeath()
    {
        if (playerData.hpCurrent < 1)
        {
            /*explodePS.transform.parent = null;
            explodePS.Play();
            Destroy(explodePS.gameObject, 2);
            Destroy(this.gameObject);*/
            OnDeath();
            Debug.Log("GAME OVER");
        }
    }
}