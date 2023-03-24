using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ClearTMPOnAwake : MonoBehaviour
{
    [SerializeField]
    [Tooltip("If on an object with a TMP component, it will be detected automatically on awake.")]
    private TMP_Text textObj;

    private void Awake()
    {
        GetTMP();
        ClearTMP();
    }

    private void GetTMP()
    {
        if (textObj == null)
        {
            textObj = GetComponent<TMP_Text>();
        }
    }
    private void ClearTMP()
    {
        if (textObj == null)
        {
            textObj.text = string.Empty;
        }
    }
}
