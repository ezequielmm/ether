using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardPanelBase : MonoBehaviour
{

    public GameObject uiCardPrefab;
    public GameObject commonCardsContainer;
    public GameObject gridCardsContainer;
    public GridLayoutGroup gridLayout;

    public bool scaleOnHover = true;
    public bool useBackgroundImage = false;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        commonCardsContainer.GetComponentInChildren<ScrollRect>().scrollSensitivity = GameSettings.PANEL_SCROLL_SPEED;
        GameManager.Instance.EVENT_HIDE_COMMON_CARD_PANEL.AddListener(HideCardSelectPanel);
        commonCardsContainer.SetActive(false);
    }

    protected void DestroyCards() 
    {
        for (int i = 0; i < gridCardsContainer.transform.childCount; i++)
        {
            Destroy(gridCardsContainer.transform.GetChild(i).gameObject);
        }
    }
    
    public void ShowCards(List<Card> cards)
    {
        DestroyCards();
        commonCardsContainer.SetActive(true);
        GenerateCards(cards);
    }

    protected void GenerateCards(List<Card> cards)
    {
        foreach (Card card in cards)
        {
            GameObject newCard = Instantiate(uiCardPrefab, gridCardsContainer.transform);
            var uiCard = newCard.GetComponent<UICardPrefabManager>();
            uiCard.useBackgroundImage = useBackgroundImage;
            uiCard.scaleCardOnHover = scaleOnHover;
            uiCard = OnGenerateCard(uiCard);
            uiCard.populate(card);
        }
    }

    public void HideCardSelectPanel()
    {
        gridLayout.cellSize = new Vector2(250, 350);
        commonCardsContainer.SetActive(false);
        DestroyCards();
    }

    public virtual UICardPrefabManager OnGenerateCard(UICardPrefabManager uiCard) 
    {
        return uiCard;
    }

}
