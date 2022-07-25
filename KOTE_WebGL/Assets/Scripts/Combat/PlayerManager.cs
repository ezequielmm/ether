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

    private void OnAttack(CombatTurnData attack) 
    {
        if (attack.target != "player") return;


    }

    private PlayerData ProcessNewData(PlayerData old, PlayerData current)
    {
        bool isAttack = false;

        /*
        if (old.defense > current.defense && (current.defense > 0 ||
            (current.defense == 0 && old.hpCurrent == current.hpCurrent))) // Hit and defence didn't fall or it did and no damage
        {
            // Play Armored Clang
            GameManager.Instance.EVENT_PLAY_SFX.Invoke("Defense Block");
        }
        if (current.defense <= 0 && old.hpCurrent > current.hpCurrent) // Damage Taken no armor
        {
            // Play Attack audio
            // Can be specific, but we'll default to "Attack"
            GameManager.Instance.EVENT_PLAY_SFX.Invoke("Attack");
        }
        */

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

            if (healthBar.value > playerData.hpCurrent) // damage taken
            {
                // This should be called by the backend at some point
                GameManager.Instance.EVENT_PLAY_ENEMY_ATTACK.Invoke(-1);
                // TODO: Replace magic number with actual timing from the enemy's animation.
                // Note: The enemy's animation may be differently timed depending on the animation.
                StartCoroutine(OnHit(0.9f));
            }
        }
    }

    private void Start()
    {
        GameManager.Instance.EVENT_PLAY_PLAYER_ATTACK.AddListener(Attack);
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

    public void Attack()
    {
        Debug.Log("+++++++++++++++[Player]Attack");
        spineAnimationsManagement.PlayAnimationSequence("Attack");
        spineAnimationsManagement.PlayAnimationSequence("Idle");
    }

    private IEnumerator OnHit(float hitTiming = 0)
    {
        yield return new WaitForSeconds(hitTiming);
        spineAnimationsManagement.PlayAnimationSequence("Hit");
        spineAnimationsManagement.PlayAnimationSequence("Idle");
    }

    private void CheckDeath()
    {
        if (playerData.hpCurrent < 1)
        {
            /*explodePS.transform.parent = null;
            explodePS.Play();
            Destroy(explodePS.gameObject, 2);
            Destroy(this.gameObject);*/

            Debug.Log("GAME OVER");
        }
    }
}