using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class PlayerManager : MonoBehaviour
{
    public SpineAnimationsManagement spineAnimationsManagement;
    public DefenseController defenseController;
    public TMP_Text healthTF;
    public Slider healthBar;

    new private Collider2D collider;
    private StatusManager statusManager;
    private Action RunWithEvent;
    private bool CalledEvent;

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

    private void Start()
    {
        GameManager.Instance.EVENT_ATTACK_REQUEST.AddListener(OnAttackRequest);
        GameManager.Instance.EVENT_ATTACK_RESPONSE.AddListener(OnAttackResponse);
        GameManager.Instance.EVENT_UPDATE_PLAYER.AddListener(OnUpdatePlayer);
        GameManager.Instance.EVENT_WS_CONNECTED.AddListener(OnWSConnected);
        GameManager.Instance.EVENT_UPDATE_ENERGY.AddListener(OnUpdateEnergy);
        GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.Players);


        collider = GetComponent<Collider2D>();

        if (statusManager == null)
            statusManager = GetComponentInChildren<StatusManager>();

        if (spineAnimationsManagement == null)
            spineAnimationsManagement = GetComponent<SpineAnimationsManagement>();
        spineAnimationsManagement.ANIMATION_EVENT.AddListener(OnAnimationEvent);

        //spineAnimationsManagement.SetSkin("weapon/sword");
        spineAnimationsManagement.PlayAnimationSequence("Idle");

    }

    private void OnAttackRequest(CombatTurnData attack) 
    {
        if(attack.originType != "player") return;
        RunAfterEvent(null);

        Debug.Log($"[PlayerManager] Combat Request GET!");

        bool endCalled = false;
        float afterEvent = 0;
        RunAfterTime(0.1f, () => { CalledEvent = false; });
        foreach (CombatTurnData.Target target in attack.targets)
        {
            // Run Attack Animation Or Status effects
            if (target.effectType == nameof(ATTACK_EFFECT_TYPES.damage))
            {
                // Run Attack
                var f = Attack();
                if (f > afterEvent) afterEvent = f;
                endCalled = true;
                RunAfterEvent(() => GameManager.Instance.EVENT_ATTACK_RESPONSE.Invoke(attack));
            }
            else if (target.effectType == nameof(ATTACK_EFFECT_TYPES.defense)) // Defense Up
            {
                var f = PlayAnimation("Cast");
                if (f > afterEvent) afterEvent = f;
                endCalled = true;
                RunAfterEvent(() => GameManager.Instance.EVENT_ATTACK_RESPONSE.Invoke(attack));
            }
            else if (target.effectType == nameof(ATTACK_EFFECT_TYPES.heal)) // Health Up
            {
                var f = PlayAnimation("Cast");
                if (f > afterEvent) afterEvent = f;
                endCalled = true;
                RunAfterEvent(() => GameManager.Instance.EVENT_ATTACK_RESPONSE.Invoke(attack));
            }
        }
        if (!endCalled)
        { // If no conditions met, pass onto the target and play cast
            var f = PlayAnimation("Cast");
            if (f > afterEvent) afterEvent = f;
            endCalled = true;
            RunAfterEvent(() => GameManager.Instance.EVENT_ATTACK_RESPONSE.Invoke(attack));
        }
        else if (afterEvent > 0)
        {
            RunAfterTime(afterEvent, () =>
            {
                if (RunWithEvent != null && !CalledEvent)
                {
                    Debug.LogWarning($"[{gameObject.name}] Animation is missing a 'attack' or 'release' event!");
                    RunWithEvent.Invoke();
                }
            });
        }
    }
    private void OnAttackResponse(CombatTurnData attack) 
    {
        var target = attack.GetTarget("player");
        if (target == null) return;

        Debug.Log($"[PlayerManager] Combat Response GET!");

        // Negitive Deltas
        float waitDuration = 0;
        if (target.defenseDelta < 0 || target.healthDelta < 0)
        {
            GameManager.Instance.EVENT_DAMAGE.Invoke(target);
        }

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

        // Positive Deltas
        if (target.defenseDelta > 0) // Defense Buffed
        {
            // Play Metallic Ring
            GameManager.Instance.EVENT_PLAY_SFX.Invoke("Defense Up");
        }
        if (target.healthDelta > 0) // Healed!
        {
            // Play Rising Chimes
            GameManager.Instance.EVENT_PLAY_SFX.Invoke("Heal");
            GameManager.Instance.EVENT_HEAL.Invoke(/*PlayerData.id*/ "player", target.healthDelta);
            waitDuration += 1;
        }

        // Update the UI
        if (target.defenseDelta != 0)
        {
            SetDefense(target.finalDefense);
        }
        if (target.healthDelta != 0)
        {
            SetHealth(target.finalHealth);
        }

        // Add status changes
        if (target.statuses != null)
        {
            statusManager.UpdateStatus(target.statuses);
        }

        RunAfterTime(waitDuration, () => {
            GameManager.Instance.EVENT_COMBAT_TURN_END.Invoke(attack.attackId); 
            //CheckDeath(target.finalHealth); 
        });
    }
    private void RunAfterEvent(Action toRun) 
    {
        RunWithEvent = toRun;
    }

    private void OnAnimationEvent(string eventName) 
    {
        if (eventName.Equals("attack") || eventName.Equals("release")) 
        {
            CalledEvent = true;
            RunWithEvent.Invoke();
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

    private PlayerData ProcessNewData(PlayerData old, PlayerData current)
    {
        if (old == null) 
        {
            SetDefense(current.defense);
            SetHealth(current.hpCurrent, current.hpMax);
            return current;
        }

        int hpDelta = current.hpCurrent - old.hpCurrent;
        int defenseDelta = current.defense - old.defense;

        if (defenseDelta < 0) 
        {
            // Natural Defense Fall (eg: New Turn)
        }

        SetDefense(current.defense);
        SetHealth(current.hpCurrent, current.hpMax);

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
            healthBar.DOValue(current.Value, 1).OnComplete( () => { CheckDeath(current.Value); });
        }
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
        defenseController.Defense = value.Value;
    }

    public float PlayAnimation(string animationSequence) 
    {
        float length = spineAnimationsManagement.PlayAnimationSequence(animationSequence);
        spineAnimationsManagement.PlayAnimationSequence("Idle");
        return length;
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

    private float OnDeath() 
    {
        float length = spineAnimationsManagement.PlayAnimationSequence("Death");
        return length;
    }


    private void CheckDeath(int current)
    {
        if (current <= 0)
        {
            // Tell game that a player is dying
            GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(PlayerState), nameof(PlayerState.dying));

            // Play animation
            RunAfterTime(OnDeath(), () => {
                // Tell game that a player is dead
                GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(PlayerState), nameof(PlayerState.dead));
            });
        }
    }

    private List<Tooltip> GetTooltipInfo()
    {
        List<Tooltip> list = new List<Tooltip>();

        foreach (IntentIcon icon in GetComponentsInChildren<IntentIcon>())
        {
            list.Add(icon.GetTooltip());
        }

        foreach (StatusIcon icon in GetComponentsInChildren<StatusIcon>())
        {
            list.Add(icon.GetTooltip());
        }

        return list;
    }

    private void OnMouseEnter()
    {
        Vector3 anchorPoint = new Vector3(collider.bounds.center.x + collider.bounds.extents.x,
            collider.bounds.center.y, 0);
        // Tooltip On
        GameManager.Instance.EVENT_SET_TOOLTIPS.Invoke(GetTooltipInfo(), TooltipController.Anchor.MiddleLeft, anchorPoint, null);
    }
    private void OnMouseExit()
    {
        // Tooltip Off
        GameManager.Instance.EVENT_CLEAR_TOOLTIPS.Invoke();
    }
}