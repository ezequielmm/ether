using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class PlayerManager : MonoBehaviour, ITooltipSetter
{
    public SpineAnimationsManagement spineAnimationsManagement;
    public DefenseController defenseController;
    public TMP_Text healthTF;
    public TMP_Text nameTextField;
    public Slider healthBar;

    new private Collider2D collider;
    private StatusManager statusManager;
    private Action RunWithEvent;
    private bool CalledEvent;
    private List<Guid> runningEvents = new List<Guid>();

    Bounds playerBounds;

    private PlayerData playerData;

    public PlayerData PlayerData
    {
        set { playerData = ProcessNewData(playerData, value); }
        get { return playerData; }
    }

    private void Start()
    {
        GameManager.Instance.EVENT_ATTACK_REQUEST.AddListener(OnAttackRequest);
        GameManager.Instance.EVENT_ATTACK_RESPONSE.AddListener(OnAttackResponse);
        GameManager.Instance.EVENT_UPDATE_PLAYER.AddListener(OnUpdatePlayer);
        GameManager.Instance.EVENT_UPDATE_ENERGY.AddListener(OnUpdateEnergy);
        GameManager.Instance.EVENT_ENCOUNTER_DAMAGE.AddListener(OnEncounterDamage);
        GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.Players);

        GameManager.Instance.EVENT_UPDATE_NAME_AND_FIEF.AddListener(SetNameAndFief);
        GameManager.Instance.EVENT_PLAYER_STATUS_UPDATE.AddListener(SetNameFromUpdate);

        collider = GetComponent<Collider2D>();
        playerBounds = collider.bounds;
        collider.enabled = false;

        GameManager.Instance.EVENT_ACTIVATE_POINTER.AddListener(ActivateCollider);
        GameManager.Instance.EVENT_DEACTIVATE_POINTER.AddListener(DeactivateCollider);

        nameTextField.text = FindObjectOfType<TopBarManager>()?.nameText?.text ?? string.Empty;

        if (statusManager == null)
            statusManager = GetComponentInChildren<StatusManager>();

        if (spineAnimationsManagement == null)
            spineAnimationsManagement = GetComponent<SpineAnimationsManagement>();
        spineAnimationsManagement.ANIMATION_EVENT.AddListener(OnAnimationEvent);

        //spineAnimationsManagement.SetSkin("weapon/sword");
        if(PlayerPrefs.GetInt("enable_injured_idle") == 1)
        {
            GameSettings.SHOW_PLAYER_INJURED_IDLE = true;
        }
        spineAnimationsManagement.PlayAnimationSequence("Idle");
    }

    public void SetNameAndFief(string name, int fief)
    {
        SetName(name);
    }
    public void SetNameFromUpdate(PlayerStateData playerState)
    {
        SetName(playerState.data.playerState.playerName);
    }


    private void ActivateCollider(PointerData _)
    {
        if (collider != null)
            collider.enabled = true;
    }

    private void DeactivateCollider(string _)
    {
        if (collider != null)
            collider.enabled = false;
    }

    private void OnAttackRequest(CombatTurnData attack)
    {
        if (attack.originType != "player") return;
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
                runningEvents.Add(attack.attackId);
                // Run Attack
                var f = Attack();
                if (f > afterEvent) afterEvent = f;
                endCalled = true;
                RunAfterEvent(() =>
                {
                    GameManager.Instance.EVENT_ATTACK_RESPONSE.Invoke(attack);
                    runningEvents.Remove(attack.attackId);
                });
            }
            else if (target.effectType == nameof(ATTACK_EFFECT_TYPES.defense)) // Defense Up
            {
                runningEvents.Add(attack.attackId);
                var f = PlayAnimation("Cast");
                if (f > afterEvent) afterEvent = f;
                endCalled = true;
                RunAfterEvent(() =>
                {
                    GameManager.Instance.EVENT_ATTACK_RESPONSE.Invoke(attack);
                    runningEvents.Remove(attack.attackId);
                });
            }
            else if (target.effectType == nameof(ATTACK_EFFECT_TYPES.heal)) // Health Up
            {
                runningEvents.Add(attack.attackId);
                var f = PlayAnimation("Cast");
                if (f > afterEvent) afterEvent = f;
                endCalled = true;
                RunAfterEvent(() =>
                {
                    GameManager.Instance.EVENT_ATTACK_RESPONSE.Invoke(attack);
                    runningEvents.Remove(attack.attackId);
                });
            }
        }

        if (!endCalled)
        {
            // If no conditions met, pass onto the target and play cast
            runningEvents.Add(attack.attackId);
            var f = PlayAnimation("Cast");
            if (f > afterEvent) afterEvent = f;
            endCalled = true;
            RunAfterEvent(() =>
            {
                GameManager.Instance.EVENT_ATTACK_RESPONSE.Invoke(attack);
                runningEvents.Remove(attack.attackId);
            });
        }
        else if (afterEvent > 0)
        {
            RunAfterTime(afterEvent, () =>
            {
                if (RunWithEvent != null && !CalledEvent && runningEvents.Contains(attack.attackId))
                {
                    Debug.LogWarning($"[{gameObject.name}] Animation is missing a 'attack' or 'release' event!");
                    RunWithEvent.Invoke();
                    runningEvents.Remove(attack.attackId);
                }
            });
        }
    }

    private void OnAttackResponse(CombatTurnData attack)
    {
        var target = attack.GetTarget("player");
        if (attack.originType == "player") PlaySound(attack);
        if (target == null) return;

        Debug.Log($"[PlayerManager] Combat Response GET!");

        // Negitive Deltas
        float waitDuration = 0;
        if (target.defenseDelta < 0 || target.healthDelta < 0)
        {
            GameManager.Instance.EVENT_DAMAGE.Invoke(target);
        }

        if (target.healthDelta < 0) // Damage Taken no armor
        {
            // Play Attack audio
            // Can be specific, but we'll default to "Attack"
            waitDuration += OnHit();
        }

        if (target.healthDelta > 0) // Healed!
        {
            // Play Rising Chimes
            GameManager.Instance.EVENT_HEAL.Invoke( /*PlayerData.id*/ "player", target.healthDelta);
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

        RunAfterTime(waitDuration, () =>
        {
            GameManager.Instance.EVENT_COMBAT_TURN_END.Invoke(attack.attackId);
            //CheckDeath(target.finalHealth); 
        });
    }

    private void PlaySound(CombatTurnData attack)
    {
        foreach (var target in attack.targets)
        {
            if (target.defenseDelta < 0 || target.healthDelta < 0)
            {
                Debug.LogWarning("Commented out sending an event from a PlaySound method??");
               // GameManager.Instance.EVENT_DAMAGE.Invoke(target);
            }

            if (target.defenseDelta < 0 &&
                target.healthDelta >= 0) // Hit and defence didn't fall or it did and no damage
            {
                // Play Armored Clang
                GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.Knight, "Block");
            }
            else if (target.healthDelta < 0) // Damage Taken no armor
            {
                // Play Attack audio
                // Can be specific, but we'll default to "Attack"
                GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.Knight, "Attack");
            }

            // Positive Deltas
            if (target.defenseDelta > 0) // Defense Buffed
            {
                // Play Metallic Ring
                GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.Knight, "Buff");
            }

            if (target.healthDelta > 0) // Healed!
            {
                // Play Rising Chimes
                GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.Knight, "Buff");
            }
        }
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
        SetName(current.playerName);

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
            healthBar.DOValue(current.Value, 1).OnComplete(() => { CheckDeath(current.Value); });
        }

        // update the player data so we can check health values in combat for animations
        if (playerData != null)
        {
            playerData.hpCurrent = current.Value;
            playerData.hpMax = max.Value;
        }
    }

    private void OnUpdateEnergy(int currentEnergy, int maxEnergy)
    {
        if (currentEnergy == 0)
        {
            // Out of energy audio
        }
    }

    private void OnUpdatePlayer(PlayerData newPlayerData)
    {
        PlayerData = newPlayerData;
    }

    private void OnEncounterDamage(int damageTaken)
    {
        OnHit();
        // the math here is due to only receiving the damage dealt, but the backend applies it to the player state
        SetHealth(playerData.hpCurrent - damageTaken);
    }

    private void SetName(string name)
    {
        Debug.Log($"NAME SET {name}");
        nameTextField.text = name;
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
        OnIdle();
        return length;
    }

    public float Attack()
    {
        Debug.Log("+++++++++++++++[Player]Attack");
        float length = spineAnimationsManagement.PlayAnimationSequence("Attack");
        OnIdle();
        return length;
    }

    private float OnHit()
    {
        float length = spineAnimationsManagement.PlayAnimationSequence("Hit");
        OnIdle();
        return length;
    }

    private float OnDeath()
    {
        float length = spineAnimationsManagement.PlayAnimationSequence("Death");
        return length;
    }

    private void OnIdle()
    {
        if (playerData.hpCurrent < (playerData.hpMax / 4)  && GameSettings.SHOW_PLAYER_INJURED_IDLE)
        {
            spineAnimationsManagement.PlayAnimationSequence("InjuredIdle");
            return;
        }

        spineAnimationsManagement.PlayAnimationSequence("Idle");
    }


    private void CheckDeath(int current)
    {
        if (current <= 0)
        {
            // Tell game that a player is dying
            GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(PlayerState), nameof(PlayerState.dying));
            GameManager.Instance.EVENT_COMBAT_FORCE_CLEAR.Invoke();

            // Play animation
            RunAfterTime(OnDeath(), () =>
            {
                // Tell game that a player is dead
                GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(PlayerState), nameof(PlayerState.dead));
            });
        }
    }

    public void SetTooltip(List<Tooltip> tooltips)
    {
        collider.enabled = true;
        playerBounds = collider.bounds;
        Vector3 anchorPoint = new Vector3(playerBounds.center.x + playerBounds.extents.x,
            playerBounds.center.y, 0);
        collider.enabled = false;
        // Tooltip On
        GameManager.Instance.EVENT_SET_TOOLTIPS.Invoke(tooltips, TooltipController.Anchor.MiddleLeft, anchorPoint,
            null);
    }
}