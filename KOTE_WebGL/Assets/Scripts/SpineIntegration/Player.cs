using System;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class Player : MonoBehaviour
{
    public SpineAnimationsManagement spineAnimationsManagement;

    private void Start()
    {
        spineAnimationsManagement = GetComponent<SpineAnimationsManagement>();
        spineAnimationsManagement.SetSkin("weapon/sword");
        spineAnimationsManagement.PlayAnimationSequence("Idle");
    }

    private void OnMouseDown()
    {
        Attack();
    }

    public void DetectMouseClick()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
        }
    }

    public void Attack()
    {
        spineAnimationsManagement.PlayAnimationSequence("Attack");
        spineAnimationsManagement.PlayAnimationSequence("Idle");
    }
}