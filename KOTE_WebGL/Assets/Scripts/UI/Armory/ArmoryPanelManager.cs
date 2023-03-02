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

        private void Start()
        {
            panelContainer.SetActive(false);
            GameManager.Instance.EVENT_EXPEDITION_CONFIRMED.AddListener(OnExpeditionConfirmed);
            GameManager.Instance.EVENT_SHOW_ARMORY_PANEL.AddListener(ActivateContainer);
            GameManager.Instance.EVENT_NFT_METADATA_RECEIVED.AddListener(PopulateCharacterList);
            GameManager.Instance.EVENT_GEAR_RECEIVED.AddListener(PopulateGear);
            OnGearSelected.AddListener(OnGearItemSelected);
        }

        private void ActivateContainer(bool show)
        {
            panelContainer.SetActive(show);
            GameManager.Instance.EVENT_GEAR_RECEIVED.Invoke(JsonConvert.SerializeObject(testData));
        }

        private void PopulateCharacterList(NftData heldNftData)
        {
            nftList.Clear();

            if (heldNftData.assets.Length == 0)
            {
                nftImage.sprite = NftImageManager.Instance.defaultImage;
                curNode = null;
                playButton.interactable = false;
                return;
            }

            foreach (NftMetaData nftMetaData in heldNftData.assets)
            {
                nftList.AddLast(new ArmoryTokenData(nftMetaData));
            }

            playButton.interactable = true;
            curNode = nftList.First;
            curNode.Value.tokenImageReceived.AddListener(UpdateCharacterImage);
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
            curNode.Value.tokenImageReceived.RemoveListener(UpdateCharacterImage);
            curNode = curNode.Previous;
            curNode.Value.tokenImageReceived.AddListener(UpdateCharacterImage);
            UpdateCharacterImage();
        }

        public void OnNextToken()
        {
            if (curNode?.Next == null) return;
            GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
            curNode.Value.tokenImageReceived.RemoveListener(UpdateCharacterImage);
            curNode = curNode.Next;
            curNode.Value.tokenImageReceived.AddListener(UpdateCharacterImage);
            nftImage.sprite = curNode.Value.NftImage;
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

        private void OnStartExpedition()
        {
            playButton.interactable = false;
            GameManager.Instance.EVENT_NFT_SELECTED.Invoke(curNode.Value.MetaData);
            //for the moment this is hardcoded
            SendData.Instance.SendStartExpedition("knight", curNode.Value.Id);
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