using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscardedCardPileManager : MonoBehaviour
{
    public GameObject informationContent;
    public GameObject cardPrefab;

    // Start is called before the first frame update
    void Start()
    {
        SetInformationRows();
    }

    public void SetInformationRows()
    {
        for (int i = 0; i < 20; i++)
        {
            GameObject currentRow = Instantiate(cardPrefab, informationContent.transform);
        }
    }
}
