using System;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class Player : MonoBehaviour
{
    public SpineAnimationsManagement spineAnimationsManagement;

    private void Start()
    {
        GameManager.Instance.EVENT_PLAY_PLAYER_ATTACK.AddListener(Attack);

        spineAnimationsManagement = GetComponent<SpineAnimationsManagement>();
        //spineAnimationsManagement.SetSkin("weapon/sword");
        spineAnimationsManagement.PlayAnimationSequence("Idle");

       
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
}