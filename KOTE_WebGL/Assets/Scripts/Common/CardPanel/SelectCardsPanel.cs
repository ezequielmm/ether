using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Used to display and select cards from a pannel. Can also be used to compare and select an upgraded card.
/// </summary>
public class SelectCardsPanel : CardPanelBase
{
    public GameObject selectCardPrefab;
    public GameObject cardPlaceholder;
    public Button selectButton;
    public TMP_Text selectButtonText;
    public Button backButton;
    public Button hideCardOverlay;
    private int cardsToSelect;
    private int totalCardsSelected;
    private int selectedCards => selectedCardIds.Count;
    [SerializeField] private List<string> selectedCardIds = new List<string>();

    private SelectPanelOptions currentSettings;

    // we have to store this one as the button can be switched out to cancel instead
    private Action<List<String>> onFinishedSelection;
    private GameObject placeholderObject;

    // so we can move the card in the correct local space
    public RectTransform cardAreaTransform;
    private (SelectableUiCardManager cardManager, GameObject cardObject) currentSelectedCard;

    // variables to control showing the card in the center
    private int selectedCardGridIndex;

    public UnityEvent OnBackButton = new UnityEvent();


    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        hideCardOverlay.gameObject.SetActive(false);
    }
    
    public void OnShowSelectCardPanel(List<Card> selectableCards, SelectPanelOptions selectOptions,
        Action<List<string>> onFinishedSelection)
    {
        ClearSelectList();
        PopulatePanel(selectableCards, selectOptions, onFinishedSelection);
    }


    public void PopulatePanel(List<Card> selectableCards, SelectPanelOptions selectOptions,
        Action<List<string>> onFinishedSelection = null, Action<string> onSelect = null)
    {
        ClearSelectList();
        DestroyCards();
        currentSettings = selectOptions;
        foreach (Card card in selectableCards)
        {
            GameObject newCard = Instantiate(selectCardPrefab, gridCardsContainer.transform);
            SelectableUiCardManager cardManager = newCard.GetComponent<SelectableUiCardManager>();
            cardManager.Populate(card);

            if (selectOptions.FireSelectWhenCardClicked)
            {
                SetCardToFireOnClick(cardManager, onSelect);
                if (!selectOptions.NoSelectButton)
                {
                    continue;
                }
            }

            SetCardToBeToggled(cardManager, selectOptions);
        }

        placeholderObject = Instantiate(cardPlaceholder, gridCardsContainer.transform);
        placeholderObject.SetActive(false);

        cardsToSelect = selectOptions.NumberOfCardsToSelect;
        this.onFinishedSelection = onFinishedSelection;
        UpdateSelectButton();
        selectButton.gameObject.SetActive(!selectOptions.MustSelectAllCards && !selectOptions.NoSelectButton);
        backButton.gameObject.SetActive(!selectOptions.HideBackButton);
        commonCardsContainer.SetActive(true);
        
        OnPanelShow?.Invoke();
    }

    private void SetCardToBeToggled(SelectableUiCardManager cardManager, SelectPanelOptions selectOptions)
    {
        // set the toggle behavior to keep track what cards are selected
        cardManager.cardSelectorToggle.onValueChanged.AddListener((isOn) =>
        {
            if (cardManager.isSelected == false && selectedCards != cardsToSelect)
            {
                selectedCardIds.Add(cardManager.GetId());
                cardManager.isSelected = true;
                if (selectOptions.ShowCardInCenter)
                {
                    ShowCardInCenter(cardManager.gameObject, cardManager);
                }
            }
            else if (cardManager.isSelected)
            {
                if (selectOptions.ShowCardInCenter)
                {
                    ReturnCardToGrid(cardManager);
                }
                else
                {
                    selectedCardIds.Remove(cardManager.GetId());
                    cardManager.isSelected = false;
                }
            }

            if (!selectOptions.NoSelectButton)
            {
                if (selectedCards == cardsToSelect || !selectOptions.MustSelectAllCards)
                    selectButton.gameObject.SetActive(true);
                else selectButton.gameObject.SetActive(false);

                UpdateSelectButton();
            }
            
            // only show the selection frame if it's not in the center
            if (!selectOptions.ShowCardInCenter)
            {
                cardManager.DetermineToggleColor();
            }
        });
    }

    private void ShowCardInCenter(GameObject selectedCard, SelectableUiCardManager cardManager)
    {
        if (currentSelectedCard.cardManager != null && currentSelectedCard.cardObject != null) return;
        List<SelectableUiCardManager> cardManagerList =
            gridCardsContainer.GetComponentsInChildren<SelectableUiCardManager>().ToList();
        selectedCardGridIndex = cardManagerList.FindIndex(card => card.GetId() == cardManager.GetId());

        selectedCard.transform.SetParent(cardAreaTransform);
        placeholderObject.SetActive(true);
        placeholderObject.transform.SetSiblingIndex(selectedCardGridIndex);
        selectedCard.transform.SetAsLastSibling();
        selectedCard.transform.DOLocalMove(
            new Vector3(cardAreaTransform.rect.width / 2, -cardAreaTransform.rect.height / 2), 1);
        currentSelectedCard = (cardManager, selectedCard);
        hideCardOverlay.gameObject.SetActive(true);
    }

    // public override for the hide overlay
    public void ReturnCardToGrid()
    {
        ReturnCardToGrid(null);
    }

    private void ReturnCardToGrid(SelectableUiCardManager cardManager)
    {
        if (cardManager != null && cardManager.GetId() != currentSelectedCard.cardManager.GetId()) return;
        RectTransform cardTransform = currentSelectedCard.cardObject.transform as RectTransform;
        RectTransform placeholderTransform = placeholderObject.transform as RectTransform;
        cardTransform.DOAnchorPos(placeholderTransform.anchoredPosition, 1).OnComplete(() =>
        {
            // add the card back to the grid
            currentSelectedCard.cardObject.transform.SetParent(gridCardsContainer.transform);
            placeholderObject.SetActive(false);
            currentSelectedCard.cardObject.transform.SetSiblingIndex(selectedCardGridIndex);
            // update the selected card
            selectedCardIds.Remove(currentSelectedCard.cardManager.GetId());
            currentSelectedCard.cardManager.isSelected = false;
            // and clear the active selected card
            currentSelectedCard.cardManager = null;
            currentSelectedCard.cardObject = null;
            UpdateSelectButton();
            hideCardOverlay.gameObject.SetActive(false);
        });
    }

    private void SetCardToFireOnClick(SelectableUiCardManager cardManager, Action<string> selectAction)
    {
        cardManager.cardSelectorToggle.onValueChanged.AddListener((isOn) =>
        {

            GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
            selectAction?.Invoke(cardManager.GetId());
        });
    }

    public void BackButtonPressed() 
    {
        OnBackButton.Invoke();
        HidePanel();
    }

    public void HidePanel()
    {
        ClearSelectList();
        HideCardSelectPanel();
    }

    public void ClearSelectList()
    {
        selectedCardIds.Clear();
        totalCardsSelected = 0;
    }

    private void UpdateSelectButton()
    {
        if (selectedCardIds.Count > 0)
        {
            selectButtonText.text = "Select";
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() =>
            {
                totalCardsSelected += selectedCards;
                if (totalCardsSelected == cardsToSelect)
                {
                    HideCardSelectPanel();
                    onFinishedSelection(selectedCardIds);
                    ClearSelectList();
                    DeleteSelectedCard();
                    selectButtonText.text = "Cancel";
                    selectButton.onClick.RemoveAllListeners();
                }
                else if (totalCardsSelected > cardsToSelect)
                {
                    HidePanel();
                    DeleteSelectedCard();
                    UpdateSelectButton();
                    // TODO: Show in an UI
                    Debug.LogError("Too many cards selected!");
                }
            });
        }
        else if (selectedCardIds.Count == 0)
        {
            selectButtonText.text = "Cancel";
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => { HidePanel(); });
        }
    }

    private void DeleteSelectedCard()
    {
        Destroy(currentSelectedCard.cardObject);
        placeholderObject.SetActive(false);
        currentSelectedCard.cardManager = null;
        currentSelectedCard.cardObject = null;
    }
}

// class to package different settings for the select menu
public class SelectPanelOptions
{
    public bool MustSelectAllCards;
    public bool HideBackButton;
    public bool NoSelectButton;
    public bool FireSelectWhenCardClicked;
    public int NumberOfCardsToSelect;
    public bool ShowCardInCenter;
}