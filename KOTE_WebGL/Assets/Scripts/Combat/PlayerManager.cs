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
            playerData = value;
            SetDefense();
            SetHealth();
        }
        get
        {
            return playerData;
        }
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


        spineAnimationsManagement = GetComponent<SpineAnimationsManagement>();
        //spineAnimationsManagement.SetSkin("weapon/sword");
        spineAnimationsManagement.PlayAnimationSequence("Idle");

    }

    private void OnEnable()
    {
        GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.Players);
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