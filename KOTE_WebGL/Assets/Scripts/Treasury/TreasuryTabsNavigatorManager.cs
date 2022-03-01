using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TreasuryTabsNavigatorManager : MonoBehaviour
{
    public Image NFTsImage, cardsImage, powerImage, armourImage;

    [Space(20)] public GameObject NFTsPanel;
    public GameObject cardsPanel, powerPanel, armourPanel;

    [Space(20)] public ScrollRect scrollRect;

    [Space(20)] public Color selectedColor;
    public Color nonSelectedColor;

    public void SelectTab(Image selectedImage, GameObject selectedPanel)
    {
        NFTsImage.color = nonSelectedColor;
        cardsImage.color = nonSelectedColor;
        powerImage.color = nonSelectedColor;
        armourImage.color = nonSelectedColor;

        NFTsPanel.SetActive(true);
        cardsPanel.SetActive(false);
        powerPanel.SetActive(false);
        armourPanel.SetActive(false);

        selectedImage.color = selectedColor;
        selectedPanel.SetActive(true);

        scrollRect.content = selectedPanel.transform as RectTransform;
    }

    public void SelectNFTsTab()
    {
        NFTsImage.color = selectedColor;
        cardsImage.color = nonSelectedColor;
        powerImage.color = nonSelectedColor;
        armourImage.color = nonSelectedColor;

        NFTsPanel.SetActive(true);
        cardsPanel.SetActive(false);
        powerPanel.SetActive(false);
        armourPanel.SetActive(false);

        scrollRect.content = NFTsPanel.transform as RectTransform;
    }

    public void SelectCardsTab()
    {
        NFTsImage.color = nonSelectedColor;
        cardsImage.color = selectedColor;
        powerImage.color = nonSelectedColor;
        armourImage.color = nonSelectedColor;

        NFTsPanel.SetActive(false);
        cardsPanel.SetActive(true);
        powerPanel.SetActive(false);
        armourPanel.SetActive(false);

        scrollRect.content = cardsPanel.transform as RectTransform;
    }

    public void SelectPowerTab()
    {
        NFTsImage.color = nonSelectedColor;
        cardsImage.color = nonSelectedColor;
        powerImage.color = selectedColor;
        armourImage.color = nonSelectedColor;

        NFTsPanel.SetActive(false);
        cardsPanel.SetActive(false);
        powerPanel.SetActive(true);
        armourPanel.SetActive(false);

        scrollRect.content = powerPanel.transform as RectTransform;
    }

    public void SelectArmourTab()
    {
        NFTsImage.color = nonSelectedColor;
        cardsImage.color = nonSelectedColor;
        powerImage.color = nonSelectedColor;
        armourImage.color = selectedColor;

        NFTsPanel.SetActive(false);
        cardsPanel.SetActive(false);
        powerPanel.SetActive(false);
        armourPanel.SetActive(true);

        scrollRect.content = armourPanel.transform as RectTransform;
    }
}