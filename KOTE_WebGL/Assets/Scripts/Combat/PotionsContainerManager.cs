using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PotionsContainerManager : MonoBehaviour
{
    public List<PotionManager> potions;
    public GameObject potionPrefab;
    public GameObject potionLayout;
    public Image warningBackground;

    public GameObject potionOptionPanel;
    public Button drinkButton;
    public Button discardButton;

    private int potionMax = 3;
    private float potionWidth = 0;

    private void Start()
    {
        potionOptionPanel.SetActive(false);
        GameManager.Instance.EVENT_PLAYER_STATUS_UPDATE.AddListener(OnPlayerStateUpdate);
        GameManager.Instance.EVENT_POTION_SHOW_POTION_MENU.AddListener(OnShowPotionOptions);
        GameManager.Instance.EVENT_POTION_POTIONS_FULL.AddListener(OnPotionsFull);
    }

    private void OnPlayerStateUpdate(PlayerStateData playerState)
    {
        float potionWidth = 0;

        ClearPotions();
        CreateHeldPotions(playerState.data.playerState.potions);
        CreateEmptyPotions();

        ResizeWarningBackground(potionWidth);
    }

    private void ClearPotions()
    {
        foreach (PotionManager potion in potions)
        {
            Destroy(potion.gameObject);
        }
        potions.Clear();
    }

    private void CreateHeldPotions(List<HeldPotion> heldPotions)
    {
        foreach (HeldPotion potion in heldPotions)
        {
            PotionManager potionManager =
                Instantiate(potionPrefab, potionLayout.transform).GetComponent<PotionManager>();
            potionManager.Populate(potion);
            potions.Add(potionManager);
            potionWidth = potionManager.GetComponent<RectTransform>().rect.width;
        }
    }

    public void CreateEmptyPotions()
    {
        for (int i = potions.Count; i < potionMax; i++)
        {
            PotionManager potion = Instantiate(potionPrefab, potionLayout.transform).GetComponent<PotionManager>();
            potions.Add(potion);
            potion.Populate(null);
        }
    }

    private void OnShowPotionOptions(PotionManager potion)
    {
        potionOptionPanel.SetActive(true);
        drinkButton.onClick.AddListener(() =>
        {
            if (potion.GetPotionTarget() == "enemy")
            {
                // this turns on the pointer from the potion
                potion.pointerActive = true;
                potionOptionPanel.SetActive(false);
                return;
            }
            
            GameManager.Instance.EVENT_POTION_USED.Invoke(potion.GetPotionId(), null);
            potionOptionPanel.SetActive(false);
        });
        discardButton.onClick.AddListener(() =>
        {
            GameManager.Instance.EVENT_POTION_DISCARDED.Invoke(potion.GetPotionId());
            potionOptionPanel.SetActive(false);
        });
    }
    
    private void ResizeWarningBackground(float potionWidth)
    {
        for (int i = 0; i < potions.Count; i++)
        {
            Vector2 width = warningBackground.rectTransform.offsetMax;
            width.x += potionWidth;
            warningBackground.rectTransform.offsetMax = width;
        }
    }

    private void OnPotionsFull()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Join(warningBackground.DOFade(0.5f, 1).SetLoops(4, LoopType.Yoyo));
        sequence.Play();
    }
}