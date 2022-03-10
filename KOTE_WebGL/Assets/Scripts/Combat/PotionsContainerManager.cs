using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionsContainerManager : MonoBehaviour
{
    public List<Potion> potions;
    public GameObject potionPrefab;

    private void Start()
    {
        GameManager.Instance.EVENT_POTION_USED.AddListener(OnPotionUsed);

        CreatePotions();
    }

    public void CreatePotions()
    {
        for (int i = 0; i < 3; i++)
        {
            Potion potion = Instantiate(potionPrefab, transform).GetComponent<Potion>();
            potions.Add(potion);
        }
    }

    public void OnPotionUsed(Potion potionUsed)
    {
        //validate potion in the server
    }
}