using System;
using System.Collections.Generic;
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
        public Sprite defaultCharacterSprite;
        public Image nftImage;
        public ArmoryHeaderManager headerPrefab;
        public Transform gearListTransform;
        public Image[] gearSlots;
        public GameObject[] gearPanels;
        private LinkedListNode<ArmoryTokenData> curNode;
        private LinkedList<ArmoryTokenData> nftList = new();
        private Dictionary<string, List<GearItemData>> categoryLists = new();
        
        private void Awake()
        {
            GameManager.Instance.EVENT_AUTHENTICATED.AddListener(PopulatePlayerGearInventory);
        }

        private void Start()
        {
            panelContainer.SetActive(false);
            GameManager.Instance.EVENT_SHOW_ARMORY_PANEL.AddListener(ActivateContainer);
            // listen for successful login to get the player's gear
            OnGearSelected.AddListener(OnGearItemSelected);
        }

        private void ActivateContainer(bool show)
        {
            panelContainer.SetActive(show);
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
            UpdatePanelOnNftUpdate();
        }

        private void PopulateGearSlots()
        {
        }

        private async void PopulatePlayerGearInventory()
        {
            GearData data = await FetchData.Instance.GetGearInventory();
            if (data == null) return;
            await GearIconManager.Instance.RequestGearIcons(data);
            PopulateGearList(data);
            GenerateHeaders();
        }

        private void PopulateGearList(GearData data)
        {
            foreach (GearItemData itemData in data.data.ownedGear)
            {
                itemData.gearImage =
                    GearIconManager.Instance.GetGearSprite(Utils.ParseEnum<Trait>(itemData.trait), itemData.name);
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

        private void UpdatePanelOnNftUpdate()
        {
            nftImage.sprite = curNode.Value.NftImage;
            foreach (GameObject panel in gearPanels)
            {
                panel.SetActive(curNode.Value.MetaData.Contract != NftContract.KnightsOfTheEther);
            }
        }

        public void OnPreviousToken()
        {
            if (curNode?.Previous == null) return;
            GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
            curNode = curNode.Previous;
            GameManager.Instance.EVENT_NFT_SELECTED.Invoke(curNode.Value.MetaData);
            UpdatePanelOnNftUpdate();
        }

        public void OnNextToken()
        {
            if (curNode?.Next == null) return;
            GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
            curNode = curNode.Next;
            GameManager.Instance.EVENT_NFT_SELECTED.Invoke(curNode.Value.MetaData);
            UpdatePanelOnNftUpdate();
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
        Legguard = 3,
        Boots = 4,
        Weapon = 5,
        Shield = 6,
        Padding = 7,
        Vambraces = 8,
        Gauntlets = 9,
    }

    public class GearData
    {
        public Data data;

        public class Data
        {
            public List<GearItemData> ownedGear;
        }
    }
}