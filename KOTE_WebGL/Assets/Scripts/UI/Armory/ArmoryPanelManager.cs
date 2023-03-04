using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace KOTE.UI.Armory
{
    public class ArmoryPanelManager : MonoBehaviour
    {
        internal static UnityEvent<GearItemData> OnGearSelected { get; } = new();

        public GameObject panelContainer;
        public Button playButton;
        public Image nftImage;
        public ArmoryHeaderManager headerPrefab;
        public Transform gearListTransform;
        public Image[] gearSlots;
        private LinkedListNode<ArmoryTokenData> curNode;
        private LinkedList<ArmoryTokenData> nftList = new();
        private Dictionary<string, List<GearItemData>> categoryLists = new();

        // +++++++ TEMP DATA UNTIL BACKEND WORKS ++++++++++++++
        private GearData testData = new GearData
        {
            gear = new List<GearItemData>
            {
                new GearItemData
                {
                    category = "Helmet",
                    gearId = 1,
                    gearImage = null,
                    name = "Test",
                    trait = "Helmet"
                },
                new GearItemData
                {
                    category = "Pauldrons",
                    gearId = 1,
                    gearImage = null,
                    name = "Test",
                    trait = "Pauldrons"
                },
                new GearItemData
                {
                    category = "Breastplate",
                    gearId = 1,
                    gearImage = null,
                    name = "Test",
                    trait = "Breastplate"
                },
                new GearItemData
                {
                    category = "Legguards",
                    gearId = 1,
                    gearImage = null,
                    name = "Test",
                    trait = "Legguards"
                },
                new GearItemData
                {
                    category = "Boots",
                    gearId = 1,
                    gearImage = null,
                    name = "Test",
                    trait = "Boots"
                },
                new GearItemData
                {
                    category = "Weapon",
                    gearId = 1,
                    gearImage = null,
                    name = "Test",
                    trait = "Weapon"
                },
                new GearItemData
                {
                    category = "Shield",
                    gearId = 1,
                    gearImage = null,
                    name = "Test",
                    trait = "Shield"
                },
                new GearItemData
                {
                    category = "Padding",
                    gearId = 1,
                    gearImage = null,
                    name = "Test",
                    trait = "Padding"
                },
                new GearItemData
                {
                    category = "Vambraces",
                    gearId = 1,
                    gearImage = null,
                    name = "Test",
                    trait = "Vambraces"
                },
                new GearItemData
                {
                    category = "Gauntlet",
                    gearId = 1,
                    gearImage = null,
                    name = "Test",
                    trait = "Gauntlet"
                },
            }
        };
        // +++++++++++++++ END TEST DATA ++++++++++++++++++++++++
        public Sprite defaultCharacterSprite;

        private void Start()
        {
            panelContainer.SetActive(false);
            GameManager.Instance.EVENT_SHOW_ARMORY_PANEL.AddListener(ActivateContainer);
            GameManager.Instance.EVENT_GEAR_RECEIVED.AddListener(PopulateGear);
            OnGearSelected.AddListener(OnGearItemSelected);
        }

        private void ActivateContainer(bool show)
        {
            panelContainer.SetActive(show);
            GameManager.Instance.EVENT_GEAR_RECEIVED.Invoke(JsonConvert.SerializeObject(testData));
            PopulateCharacterList();
        }

        private void PopulateCharacterList()
        {
            List<Nft> nfts = NftManager.Instance.GetAllNfts();
            nftList.Clear();

            if (nfts.Count == 0)
            {
                nftImage.sprite = defaultCharacterSprite;
                curNode = null;
                playButton.interactable = false;
                return;
            }
            
            foreach (Nft nft in nfts)
            {
                nftList.AddLast(new ArmoryTokenData(nft));
            }

            playButton.interactable = true;
            curNode = nftList.First;
            GameManager.Instance.EVENT_NFT_SELECTED.Invoke(curNode.Value.MetaData);
            UpdateCharacterImage();
        }

        private void PopulateGear(string rawData)
        {
            GearData data = Unity.Plastic.Newtonsoft.Json.JsonConvert.DeserializeObject<GearData>(rawData);
            if (data == null) return;
            PopulateGearList(data);
            GenerateHeaders();
        }

        private void PopulateGearList(GearData data)
        {
            foreach (GearItemData itemData in data.gear)
            {
                if (categoryLists.ContainsKey(itemData.category))
                {
                    categoryLists[itemData.category].Add(itemData);
                    continue;
                }

                categoryLists[itemData.category] = new List<GearItemData> { itemData };
            }
        }

        private void GenerateHeaders()
        {
            string[] categories = Enum.GetNames(typeof(GearCategories));
            foreach (string category in categories)
            {
                ArmoryHeaderManager header = Instantiate(headerPrefab, gearListTransform);
                header.Populate(category, categoryLists[category]);
            }
        }

        private void UpdateCharacterImage()
        {
            nftImage.sprite = curNode.Value.NftImage;
        }

        public void OnPreviousToken()
        {
            if (curNode?.Previous == null) return;
            GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
            curNode = curNode.Previous;
            GameManager.Instance.EVENT_NFT_SELECTED.Invoke(curNode.Value.MetaData);
            UpdateCharacterImage();
        }

        public void OnNextToken()
        {
            if (curNode?.Next == null) return;
            GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
            curNode = curNode.Next;
            GameManager.Instance.EVENT_NFT_SELECTED.Invoke(curNode.Value.MetaData);
            UpdateCharacterImage();
        }

        public void OnPlayButton()
        {
            GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
            OnStartExpedition();
        }

        public void OnBackButton()
        {
            GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
            ActivateContainer(false);
        }

        private async void OnStartExpedition()
        {
            playButton.interactable = false;
            GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
            bool success = await FetchData.Instance.RequestNewExpedition("knight", curNode.Value.Id);
            if (success)
            {
                OnExpeditionConfirmed();
            }
        }

        private void OnExpeditionConfirmed()
        {
            // play the correct music depending on where the player is
            GameManager.Instance.EVENT_PLAY_MUSIC.Invoke(MusicTypes.Music, 1);
            GameManager.Instance.EVENT_PLAY_MUSIC.Invoke(MusicTypes.Ambient, 1);
            GameManager.Instance.LoadScene(inGameScenes.Expedition);
        }

        private void OnGearItemSelected(GearItemData activeItem)
        {
            GearCategories category = Utils.ParseEnum<GearCategories>(activeItem.category);
            gearSlots[(int)category].sprite = activeItem.gearImage;
        }
    }

    internal enum GearCategories
    {
        Helmet = 0,
        Pauldrons = 1,
        Breastplate = 2,
        Legguards = 3,
        Boots = 4,
        Weapon = 5,
        Shield = 6,
        Padding = 7,
        Vambraces = 8,
        Gauntlet = 9,
    }

    internal class GearData
    {
        public List<GearItemData> gear;
    }
}