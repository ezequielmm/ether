﻿using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace KOTE.UI.Armory
{
    public class GearListManager : MonoBehaviour
    {
        [SerializeField] private GameObject loadingText;
        
        private Dictionary<string, Sprite> cachedSprites = new();
        private List<Category> categoryLists = new();
        private Category currentCategory => categoryLists[currentCategoryIndex];

        [SerializeField] private ArmoryHeaderManager header;

        [SerializeField] private int pageLength = 6;
        [SerializeField] private TMP_Text pagesCount;
        
        private int currentItemIndex = 0;
        private int currentCategoryIndex = 0;
        
        class Category
        {
            public string name;
            public List<GearItemData> items;
        }
        
        public void Clear()
        {
            loadingText.SetActive(true);
            // Remove instantiated items
        }

        public void AddGearItem(GearItemData itemData, Texture2D texture)
        {
            var sprite = default(Sprite);
            if (cachedSprites.ContainsKey(itemData.name)) {
                sprite = cachedSprites[itemData.name];
            }
            else {
                sprite = texture?.ToSprite();
                cachedSprites.Add(itemData.name, sprite);
            }
                        
            itemData.gearImage = sprite;

            if (itemData.gearImage == null)
                Debug.LogError(
                    $"Image is null for {itemData.name} - {itemData.category} - {itemData.trait}");

            var category = categoryLists.FirstOrDefault(e => e.name == itemData.category);
            if (categoryLists.Any(e => e.name == itemData.category))
                category.items.Add(itemData);
            else
                categoryLists.Add(new ()
                {
                    name = itemData.category,
                    items = new List<GearItemData> { itemData }   
                });
        }
        
        public void UpdateGearListBasedOnToken(Nft SelectedCharacter)
        {
            if (SelectedCharacter != null)
                header.UpdateGearSelectableStatus(SelectedCharacter.Contract);
            else
                Debug.Log($"This node is null");
        }

        public void GenerateHeaders()
        {
            ShowPage(0);
            loadingText.SetActive(false);
        }

        [ContextMenu("NextItemsPage")]
        public void NextItemsPage()
        {
            if (currentItemIndex + pageLength >= currentCategory.items.Count)
                currentItemIndex = 0;
            else
                currentItemIndex += pageLength;
            
            ShowPage(currentItemIndex);
        }

        [ContextMenu("PreviousItemsPage")]
        public void PreviousItemsPage()
        {
            if (currentItemIndex == 0 && currentCategory.items.Count > pageLength)
                currentItemIndex = pageLength * (currentCategory.items.Count / pageLength);
            else
                currentItemIndex -= pageLength;
            if (currentItemIndex < 0)
                currentItemIndex = 0;
            
            ShowPage(currentItemIndex);
        }
        
        private void ShowPage(int startIndex)
        {
            int page = currentItemIndex / pageLength + 1;
            pagesCount.text = $"{page}/{currentCategory.items.Count / pageLength + (pageLength != currentCategory.items.Count ? 1 : 0)}";
            
            header.ClearList();
            header.Populate(currentCategory.name, currentCategory.items.Skip(startIndex).Take(pageLength).ToList());
            header.OnToggle(true);
        }

        public void NextCategory()
        {
            // Advance categoryIndex loopeable
            currentCategoryIndex = (currentCategoryIndex + 1) % categoryLists.Count;
            currentItemIndex = 0;

            ShowPage(0);
        }

        public void PrevCategory()
        {
            // Decrease categoryIndex loopeable
            currentCategoryIndex = (currentCategoryIndex - 1 + categoryLists.Count) % categoryLists.Count;
            currentItemIndex = 0;

            ShowPage(0);
        }
    }
}