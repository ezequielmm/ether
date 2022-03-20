using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    private void Start()
    {
        GameManager.Instance.EVENT_TARGET_POINTER_SELECTED.AddListener(DoCardEffect);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DetectTargetable();
        }
    }

    public void DetectTargetable()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition),
            Vector3.forward, Mathf.Infinity);

        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            GameManager.Instance.EVENT_START_POINTER.Invoke(gameObject);
        }
    }

    public void DoCardEffect(GameObject invoker)
    {
        if (invoker != gameObject) return;
        Debug.Log($"Card attack enemy");
    }
}