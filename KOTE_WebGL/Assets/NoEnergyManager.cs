using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System;

public class NoEnergyManager : MonoBehaviour
{
    public TextMeshProUGUI noEnergyLabel;

    void Start()
    {
        GameManager.Instance.EVENT_CARD_NO_ENERGY.AddListener(OnCardNoEnergy);
        DeactivateLabels();
    }


    void OnCardNoEnergy()
    {
        noEnergyLabel.gameObject.SetActive(true);
        noEnergyLabel.DOFade(1, .5f).From(0).SetLoops(2, LoopType.Yoyo).OnComplete(OnComplete);
        
    }

    void OnComplete()
    {
        //DeactivateLabels();
    }

    private void DeactivateLabels()
    {
        noEnergyLabel.gameObject.SetActive(false);
        
    }
}
