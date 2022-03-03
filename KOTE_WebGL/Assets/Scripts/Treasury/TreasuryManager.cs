using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TreasuryManager : MonoBehaviour
{
    public TreasuryTabsNavigatorManager treasuryTabsNavigatorManager;
    public List<Transform> panelTransforms;

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
        foreach (Transform panelTransform in panelTransforms)
        {
            DestroyChildren(panelTransform);
        }

        foreach (Transform panelTransform in panelTransforms)
        {
            SetObjects(panelTransform);
        }
    }

    public void DestroyChildren(Transform transform)
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void SetObjects(Transform transform)
    {
        int objectsAmount = Random.Range(7, 20);

        for (int i = 0; i < objectsAmount; i++)
        {
            GameObject localObject = Instantiate(tabContentPrefab, transform);
            localObject.transform.GetChild(0).GetComponent<TMP_Text>().text = i.ToString();
        }
    }
}