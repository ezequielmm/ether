using System;
using System.Collections.Generic;
using System.Linq;
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
        public GearSlot[] gearSlots;
        public GameObject[] gearPanels;

        private LinkedListNode<ArmoryTokenData> curNode;
        private LinkedList<ArmoryTokenData> nftList = new();
        private Dictionary<string, List<GearItemData>> categoryLists = new();
        private Dictionary<GearCategories, GearItemData> equippedGear = new();
        private Dictionary<int, List<GearItemData>> villagerEquippedGear = new();
        private Dictionary<int, List<GearItemData>> blessedVillagerEquippedGear = new();

        private void Awake()
        {
            GameManager.Instance.EVENT_REQUEST_LOGIN_SUCESSFUL.AddListener(OnLogin);
            NftManager.Instance.NftsLoaded.AddListener(PopulateCharacterList);
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
        }

        private void PopulateCharacterList()
        {
            List<Nft> nfts = NftManager.Instance.GetAllNfts();

            // ++++++++++++++++++++++TEST VILLAGER DATA++++++++++++++++++++++++++++++++++++++++++++++++++
            Nft testVillager = new Nft
            {
                Contract = NftContract.Villager,
                Description = "NOPE THIS IS A TEST",
                ImageUrl = "https://i.seadn.io/gcs/files/5685fc1142009fd4233a2b076ef762f9.jpg",
                Name = "Villager #10",
                TokenId = 10,
                Traits = new Dictionary<Trait, string>()
            };
            testVillager.Traits[Trait.Padding] = "Brown";
            testVillager.Traits[Trait.Helmet] = "None";
            testVillager.Traits[Trait.Shield] = "Rusty Shield";
            testVillager.Traits[Trait.Weapon] = "Rusty Sword";

            nfts.Add(testVillager);
            // +++++++++++++++++ END TEST VILLAGER+++++++++++++++++++++++++++++++++++++++++
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

        private async void UpdatePanelOnNftUpdate()
        {
            nftImage.sprite = await curNode.Value.MetaData.GetImage();
            foreach (GameObject panel in gearPanels)
            {
                panel.SetActive(!curNode.Value.MetaData.isKnight);
            }

            if (curNode.Value.MetaData.isKnight) ClearGearSlots();
            else PopulateEquippedGear();
        }

        private void OnLogin(string data, int data2)
        {
            PopulatePlayerGearInventory();
        }

        private async void PopulatePlayerGearInventory()
        {
            GearData data = await FetchData.Instance.GetGearInventory();
            if (data == null) return;
            await GearIconManager.Instance.RequestGearIcons(data);
            villagerEquippedGear[10] = data.equippedGear;
            PopulateGearInventory(data.ownedGear);

            GenerateHeaders();
        }

        private void PopulateGearInventory(List<GearItemData> ownedGear)
        {
            foreach (GearItemData itemData in ownedGear)
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
                if (categoryLists.ContainsKey(category))
                {
                    header.Populate(category, categoryLists[category]);
                }
            }
        }

        private void PopulateEquippedGear()
        {
            if (curNode.Value.MetaData.Contract == NftContract.Villager &&
                villagerEquippedGear.ContainsKey(curNode.Value.Id))
            {
                EquipGearInSlots(villagerEquippedGear[curNode.Value.Id]);
            }
            else if (curNode.Value.MetaData.Contract == NftContract.BlessedVillager &&
                     blessedVillagerEquippedGear.ContainsKey(curNode.Value.Id))
            {
                EquipGearInSlots(blessedVillagerEquippedGear[curNode.Value.Id]);
            }
            else
            {
                ClearGearSlots();
            }
        }

        private void EquipGearInSlots(List<GearItemData> gear)
        {
            foreach (GearSlot slot in gearSlots)
            {
                GearItemData curGear = gear.Find(x => Utils.ParseEnum<Trait>(x.trait) == slot.gearTrait);
                if (curGear != null)
                {
                    slot.SetGearInSlot(curGear);
                    continue;
                }

                slot.ResetSlot();
            }
        }

        private void ClearGearSlots()
        {
            foreach (GearSlot slot in gearSlots)
            {
                slot.ResetSlot();
            }
        }

        private void SetEquippedGear()
        {
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
            EquippedGearData selectedGear = new EquippedGearData
            {
                data = new EquippedGearData.Data { equippedGear = equippedGear.Values.ToList() }
            };
            bool success =
                await FetchData.Instance.RequestNewExpedition("knight", curNode.Value.Id, selectedGear);
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
            if (curNode.Value.MetaData.Contract == NftContract.KnightsOfTheEther) return;
            GearCategories category = Utils.ParseEnum<GearCategories>(activeItem.category);
            gearSlots[(int)category].SetGearInSlot(activeItem);
            equippedGear[Utils.ParseEnum<GearCategories>(activeItem.category)] = activeItem;
            GameManager.Instance.EVENT_UPDATE_NFT.Invoke(Enum.Parse<Trait>(activeItem.trait), activeItem.name);
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

    public class EquippedGearData
    {
        public Data data;

        public class Data
        {
            public List<GearItemData> equippedGear;
        }
    }

    public class GearData
    {
        public List<GearItemData> ownedGear;
        public List<GearItemData> equippedGear;
    }
}