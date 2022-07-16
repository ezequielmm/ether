using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerManager : MonoBehaviour
{
    public SpineAnimationsManagement spineAnimationsManagement;
    public TMP_Text defenseTF;
    public TMP_Text healthTF;
    public Slider healthBar;

    public ParticleSystem hitPS;

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

            if (playerData.hpCurrent < healthBar.value)
            {
                hitPS.Play();
            }

            healthBar.DOValue(playerData.hpCurrent, 1).OnComplete(CheckDeath);

           
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
        GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.Players);
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