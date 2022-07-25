using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

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

        // Run Attack Animation Or Status effects
        if (attack.defenseDelta != 0 || attack.healthDelta != 0) 
        {
            // Run Attack
            Attack();
            RunAfterTime(0.45f, // hard coded player animation attack point
                () => GameManager.Instance.EVENT_ATTACK_RESPONSE.Invoke(attack));
        }
    }
    private void OnAttackResponse(CombatTurnData attack) 
    {
        if (attack.target != "player") return;

        float waitDuration = 0;
        if (attack.defenseDelta < 0 && attack.healthDelta >= 0) // Hit and defence didn't fall or it did and no damage
        {
            // Play Armored Clang
            GameManager.Instance.EVENT_PLAY_SFX.Invoke("Defense Block");
        } 
        else if (attack.healthDelta < 0) // Damage Taken no armor
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

    private IEnumerator RunAfterTime(float time, Action toRun) 
    {
        yield return new WaitForSeconds(time);
        toRun.Invoke();
    }

    private PlayerData ProcessNewData(PlayerData old, PlayerData current)
    {
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
            var attack = new CombatTurnData()
            {
                origin = "Unknown",
                target = "player",
                healthDelta = hpDelta,
                defenseDelta = defenseDelta
            };
            GameManager.Instance.EVENT_COMBAT_TURN_ENQUEUE.Invoke(attack);
        }
        

        if (isAttack == false) 
        {
            SetDefense();
            SetHealth();
        }
        return current;
    }

    private void SetHealth()
    {
        Debug.Log("[SetHealth]min=" + playerData.hpCurrent + "/" + playerData.hpMax);

        healthTF.SetText(playerData.hpCurrent + "/" + playerData.hpMax);

        healthBar.maxValue = playerData.hpMax;

        if (healthBar.value != playerData.hpCurrent)
        {
            healthBar.DOValue(playerData.hpCurrent, 1).OnComplete(CheckDeath);
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

    private void SetDefense()
    {
        defenseTF.SetText(playerData.defense.ToString());
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