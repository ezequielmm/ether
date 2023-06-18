using System;
using System.Collections.Generic;
using System.Linq;
using map;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace KOTE.UI.Armory
{
    public class ArmoryPanelManager : MonoBehaviour
    {
        internal static UnityEvent<GearItemData> OnGearSelected { get; } = new();
        internal static UnityEvent<Trait> OnSlotCleared { get; } = new();

        public GameObject panelContainer;
        public Button playButton;
        public CharacterPortraitManager portraitManager;
        public TMP_Text TokenNameText;
        public TMP_Text CanPlayText;
        public ArmoryHeaderManager headerPrefab;
        public Transform gearListTransform;
        public List<GearSlot> gearSlots;

        public GameObject[] gearPanels;

        // making a reference to this since GetComponentInChildren only works on active gameObjects
        public ScrollRect gearListScroll;

        private LinkedListNode<ArmoryTokenData> curNode;
        private LinkedList<ArmoryTokenData> nftList = new();
        private Dictionary<string, List<GearItemData>> categoryLists = new();
        private Dictionary<Trait, GearItemData> equippedGear = new();
        private Dictionary<int, List<GearItemData>> villagerEquippedGear = new();
        private Dictionary<int, List<GearItemData>> blessedVillagerEquippedGear = new();
        private List<ArmoryHeaderManager> gearHeaders = new();

        [SerializeField] private PostProcessingTransition postProcessingTransition;
        
        private void Awake()
        {
            GameManager.Instance.EVENT_AUTHENTICATED.AddListener(PopulatePlayerGearInventory);
            NftManager.Instance.NftsLoaded.AddListener(PopulateCharacterList);
        }

        private void Start()
        {
            panelContainer.SetActive(false);
            GameManager.Instance.EVENT_SHOW_ARMORY_PANEL.AddListener(ActivateContainer);
            // listen for successful login to get the player's gear
            OnGearSelected.AddListener(OnGearItemSelected);
            OnSlotCleared.AddListener(OnGearItemRemoved);
            gearListScroll.scrollSensitivity = GameSettings.PANEL_SCROLL_SPEED;
        }

        private void ActivateContainer(bool show)
        {
            // run this whe the panel is opened, instead of when nfts load, so images are cached
            try
            {
                GameManager.Instance.EVENT_NFT_SELECTED.Invoke(curNode.Value.MetaData);
                UpdatePanelOnNftUpdate();
                panelContainer.SetActive(show);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void PopulateCharacterList()
        {
            List<Nft> nfts = NftManager.Instance.GetAllNfts();
            PlayerSpriteManager.Instance.CachePlayerSkinsAtStartup(nfts);
            PortraitSpriteManager.Instance.CacheAllSprites();
            nftList.Clear();

            if (nfts.Count == 0)
            {
                portraitManager.SetDefault();
                curNode = null;
                playButton.interactable = false;
                return;
            }

            foreach (Nft nft in nfts)
            {
                nftList.AddLast(new ArmoryTokenData(nft));
            }

            curNode = nftList.First;
            GameManager.Instance.EVENT_NFT_SELECTED.Invoke(curNode.Value.MetaData);
        }

        private void UpdatePanelOnNftUpdate()
        {
            Nft curMetadata = curNode.Value.MetaData;
            TokenNameText.text = FormatTokenName(curMetadata);
            CanPlayText.text = curMetadata.CanPlay ? "" : $"Available in: {ParseTime((int)(curMetadata.PlayableAt - DateTime.UtcNow).TotalSeconds)}";
            CanPlayText.transform.parent.gameObject.SetActive(!curMetadata.CanPlay);
            portraitManager.SetPortrait(curMetadata);
            foreach (GameObject panel in gearPanels)
            {
                panel.SetActive(!curMetadata.isKnight);
            }

            playButton.interactable = curMetadata.CanPlay;
            if (curMetadata.isKnight) ClearGearSlots();
            else PopulateEquippedGear();

            UpdateGearListBasedOnToken();
        }

        private string FormatTokenName(Nft tokenData)
        {
            string contractName = "";
            switch (tokenData.Contract)
            {
                case NftContract.Knights:
                    contractName = tokenData.Contract.ToString().TrimEnd('s');
                    break;
                case NftContract.Villager:
                    contractName = tokenData.Contract.ToString();
                    break;
                case NftContract.BlessedVillager:
                    contractName = "Blessed Villager";
                    break;
                case NftContract.NonTokenVillager:
                    return "Basic Villager";
            }

            return contractName + " #" + tokenData.TokenId;
        }

        private async void PopulatePlayerGearInventory()
        {
            GearData data = await FetchData.Instance.GetGearInventory();
            if (data == null) return;
            await GearIconManager.Instance.RequestGearIcons(data);
            PopulateGearList(data.ownedGear);

            GenerateHeaders();
        }

        private async void PopulateGearList(List<GearItemData> ownedGear)
        {
            foreach (GearItemData itemData in ownedGear)
            {
                itemData.gearImage =
                    await GearIconManager.Instance.GetGearSprite(itemData.trait.ParseToEnum<Trait>(), itemData.name);
                if (categoryLists.ContainsKey(itemData.category))
                {
                    categoryLists[itemData.category].Add(itemData);
                    continue;
                }

                categoryLists[itemData.category] = new List<GearItemData> { itemData };
            }

            UpdateGearListBasedOnToken();
        }

        private void UpdateGearListBasedOnToken()
        {
            foreach (ArmoryHeaderManager header in gearHeaders)
            {
                header.UpdateGearSelectableStatus(curNode.Value.MetaData.Contract);
            }
        }

        private void GenerateHeaders()
        {
            foreach (string category in categoryLists.Keys)
            {
                ArmoryHeaderManager header = Instantiate(headerPrefab, gearListTransform);
                if (categoryLists.ContainsKey(category))
                {
                    header.Populate(category, categoryLists[category]);
                    gearHeaders.Add(header);
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
                GearItemData curGear = gear.Find(x => x.trait.ParseToEnum<Trait>() == slot.gearTrait);
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
            if (curNode?.Next == null)
            {
                Debug.LogError($"[OnNextToken] no cur node or next node is null");
                return;
            }
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

        public void OnTroveButton()
        {
            Application.OpenURL("https://trove.treasure.lol/games/kote");
        }

        private async void OnStartExpedition()
        {
            playButton.interactable = false;
            GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
            ExpeditionStartData startData =
                await FetchData.Instance.RequestNewExpedition(curNode.Value.MetaData.Contract, curNode.Value.Id,
                    equippedGear.Values.ToList());
            if (startData.expeditionCreated)
            {
                OnExpeditionConfirmed();
                return;
            }

            if (!string.IsNullOrEmpty(startData.reason))
            {
                GameManager.Instance.EVENT_SHOW_CONFIRMATION_PANEL_WITH_FULL_CONTROL.Invoke(startData.reason, () => { },
                    () => { }, new[] { "Close", "" });
            }

            playButton.interactable = curNode.Value.MetaData.CanPlay;
        }

        private void OnExpeditionConfirmed()
        {
            // play the correct music depending on where the player is
            GameManager.Instance.EVENT_PLAY_MUSIC.Invoke(MusicTypes.Music, 1);
            GameManager.Instance.EVENT_PLAY_MUSIC.Invoke(MusicTypes.Ambient, 1);
            //GameManager.Instance.LoadScene(inGameScenes.Expedition);
            postProcessingTransition.OnTransitionInEnd.AddListener(() => GameManager.Instance.LoadScene(inGameScenes.Expedition, true));
            postProcessingTransition.StartTransition();
        }

        private void OnGearItemSelected(GearItemData activeItem)
        {
            if (curNode.Value.MetaData.Contract == NftContract.Knights ||
                (curNode.Value.MetaData.Contract == NftContract.Villager && !activeItem.CanVillagerEquip)) return;

            Trait itemTrait = activeItem.trait.ParseToEnum<Trait>();
            gearSlots.Find(x => x.gearTrait == itemTrait).SetGearInSlot(activeItem);
            equippedGear[itemTrait] = activeItem;
            GameManager.Instance.EVENT_UPDATE_NFT.Invoke(Enum.Parse<Trait>(activeItem.trait), activeItem.name);
        }

        private void OnGearItemRemoved(Trait gearTrait)
        {
            equippedGear.Remove(gearTrait);
            GameManager.Instance.EVENT_UPDATE_NFT.Invoke(gearTrait, "");
        }
        
        public string ParseTime(int totalSeconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(totalSeconds);

            // Si el tiempo es menor a una hora, formatear como minutos y segundos.
            if (time.TotalHours < 1)
            {
                return $"{time.Minutes:D2}:{time.Seconds:D2}";
            }
            // Si el tiempo es menor a un día pero mayor o igual a una hora, formatear como horas y minutos.
            else if (time.TotalDays < 1)
            {
                return $"{time.Hours}Hr {time.Minutes:D2}m";
            }
            // Si el tiempo es mayor o igual a un día, formatear como días, horas y minutos.
            else
            {
                return $"{time.Days}Ds {time.Hours}Hr";
            }
        }
    }


    public class GearData
    {
        public List<GearItemData> ownedGear;
        // TODO this isn't working quite right, it's not correctly being parsed into a js object
        // TODO so we're getting a string back instead of a json object
        //public List<GearItemData> equippedGear;
    }
}