using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Spine;
using Spine.Unity;
using UnityEditor.VersionControl;
using UnityEngine;
using Animation = Spine.Animation;
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

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DetectMouseClick();
        }
    }

    public void DetectMouseClick()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            Attack();
        }
    }

    public void Attack()
    {
        spineAnimationsManagement.PlayAnimationSequence("Attack");
        spineAnimationsManagement.PlayAnimationSequence("Idle");
    }
}