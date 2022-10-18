using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardPanelBase : MonoBehaviour
{
    public GameObject commonCardsContainer;

    public GameObject gridCardsContainer;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        commonCardsContainer.SetActive(false);
        GameManager.Instance.EVENT_HIDE_COMMON_CARD_PANEL.AddListener(OnHideSelectCardPanel);
    }

    private void OnHideSelectCardPanel()
    {
        commonCardsContainer.SetActive(false);
        DestroyCards();
    }

    protected void DestroyCards()
    {
        for (int i = 0; i < gridCardsContainer.transform.childCount; i++)
        {
            Destroy(gridCardsContainer.transform.GetChild(i).gameObject);
        }
    }
}