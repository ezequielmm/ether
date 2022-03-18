using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using Animation = Spine.Animation;

public class Dragon : MonoBehaviour
{
    public SpineAnimationsManagement spineAnimationsManagement;

    private void Start()
    {
        spineAnimationsManagement = GetComponent<SpineAnimationsManagement>();
        spineAnimationsManagement.PlayAnimationSequence("Flying");
    }
}