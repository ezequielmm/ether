using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExhaustedCardPileManager : MonoBehaviour
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
        for (int i = 0; i < 30; i++)
        {
            GameObject currentRow = Instantiate(cardPrefab, informationContent.transform);
        }
    }
}
