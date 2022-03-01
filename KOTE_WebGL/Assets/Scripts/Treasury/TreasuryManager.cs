using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TreasuryManager : MonoBehaviour
{
    public TreasuryTabsNavigatorManager treasuryTabsNavigatorManager;

    [Space(20)] public GameObject characterList;
    public GameObject tabContentPrefab;

    private void Start()
    {
        Button firstCharacterButton = characterList.transform.GetChild(0)?.GetComponent<Button>();
        if (firstCharacterButton != null) firstCharacterButton.onClick?.Invoke();
    }

    public void OnCharacterButtonStatic()
    {
        treasuryTabsNavigatorManager.SelectNFTsTab();

        SetTestingObjects();
    }

    //just setting random objects for testing
    private void SetTestingObjects()
    {
        //setting random amount of objects for testing purposes
        foreach (Transform child in treasuryTabsNavigatorManager.NFTsPanel.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in treasuryTabsNavigatorManager.cardsPanel.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in treasuryTabsNavigatorManager.powerPanel.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in treasuryTabsNavigatorManager.armourPanel.transform)
        {
            Destroy(child.gameObject);
        }

        int objectsAmount = Random.Range(7, 20);

        for (int i = 0; i < objectsAmount; i++)
        {
            Debug.Log($"{i}");
            GameObject localObject = Instantiate(tabContentPrefab, treasuryTabsNavigatorManager.NFTsPanel.transform);
            localObject.transform.GetChild(0).GetComponent<TMP_Text>().text = i.ToString();
        }

        objectsAmount = Random.Range(7, 20);

        for (int i = 0; i < objectsAmount; i++)
        {
            GameObject localObject = Instantiate(tabContentPrefab, treasuryTabsNavigatorManager.cardsPanel.transform);
            localObject.transform.GetChild(0).GetComponent<TMP_Text>().text = i.ToString();
        }

        objectsAmount = Random.Range(7, 20);

        for (int i = 0; i < objectsAmount; i++)
        {
            GameObject localObject = Instantiate(tabContentPrefab, treasuryTabsNavigatorManager.powerPanel.transform);
            localObject.transform.GetChild(0).GetComponent<TMP_Text>().text = i.ToString();
        }

        objectsAmount = Random.Range(7, 20);

        for (int i = 0; i < objectsAmount; i++)
        {
            GameObject localObject = Instantiate(tabContentPrefab, treasuryTabsNavigatorManager.armourPanel.transform);
            localObject.transform.GetChild(0).GetComponent<TMP_Text>().text = i.ToString();
        }
    }
}