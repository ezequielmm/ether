using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField]
    private GameObject combatContainer;

    private void Start()
    {
        combatContainer.SetActive(false);
        GameManager.Instance.EVENT_NODE_DATA_UPDATE.AddListener(OnNodeDataUpdated);//TODO: this will change as not all nodes will be combat
    }

    private void OnNodeDataUpdated(string arg0)
    {
        combatContainer.SetActive(true);
    }
}