using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoyalHouseManager : MonoBehaviour
{
    [Serializable]
    public class ArmoryItem
    {
        public string name;
        public string description;
        public int encumbrance;
    }

    [Serializable]
    public class BlessingItem
    {
        public string name;
        public string description;
        public int fief;
    }

    public GameObject confirmationPanel;

    public GameObject armoryItemPrefab;
    public GameObject blessingItemPrefab;

    public void ActivateInnerSelectionsConfirmPanel(bool activate)
    {
        confirmationPanel.SetActive(activate);
    }
}