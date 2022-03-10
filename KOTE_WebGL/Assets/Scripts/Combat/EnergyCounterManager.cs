using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnergyCounterManager : MonoBehaviour
{
    public TMP_Text energyText;
    public int maxEnergy;

    private void Start()
    {
        maxEnergy = 10;
        SetEnergyText(5);
    }

    public void SetEnergyText(int energy)
    {
        energyText.text = $"{energy}/{maxEnergy}";
    }
}