using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using map;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KOTE.UI.Armory
{
    public class ArmoryPanelManager : MonoBehaviour
    {
        public CharactersListManager charactersListManager;
        public GearListManager GearListManager;

        public GameObject panelContainer;
        public Button playButton;
        public CharacterPortraitManager portraitManager;
        public List<GearSlot> gearSlots;

        public List<GameObject> gearPanels;

        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;
        [SerializeField] private TextMeshProUGUI loadingText;

        private LinkedList<ArmoryTokenData> nftList = new();
        private Dictionary<string, List<GearItemData>> categoryLists = new();
        private Dictionary<Trait, VictoryItems> equippedGear = new();
        private Dictionary<int, List<GearItemData>> villagerEquippedGear = new();
        private Dictionary<int, List<GearItemData>> blessedVillagerEquippedGear = new();
        private List<ArmoryHeaderManager> gearHeaders = new();

        [SerializeField] private PostProcessingTransition postProcessingTransition;
        public const string MEMORY_REFRESH_MESSAGE = "refresh-armory";

        private Coroutine populateGearInventoryRoutine;

        private Nft SelectedCharacter => charactersListManager.SelectedCharacter;


        private void Awake()
        {
            loadingText.text = "";
            loadingText.raycastTarget = false;

            NftManager.Instance.NftsLoaded.AddListener(PopulateCharacterList);
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                OnArmoryRefresh();
            }

            Debug.Log($"[Armory] SelectedCharacter {SelectedCharacter.FormatTokenName()}");
        }
#endif

        public void OnBridgeOpen()
        {
            var data = new Dictionary<string, object>
            {
                {"eventName", "open-bridge"},
                {"data", null}
            };
            var sendJson = JsonConvert.SerializeObject(data);
            Debug.Log($"Json: {sendJson}");
            WebBridge.SendUnityMessage(sendJson, sendJson);
        }

        public void OnArmoryRefresh()
        {
            FindObjectOfType<ArmoryKnightRendererManager>(true).OnSkinLoading();

            StartCoroutine(Routine());

            IEnumerator Routine()
            {
                yield return null;
                PopulatePlayerGearInventory();
            }
        }

        private void Start()
        {
            panelContainer.SetActive(false);
            // listen for successful login to get the player's gear

        }

        public void ActivateContainer(bool show)
        {
            panelContainer.SetActive(show);
            gameObject.SetActive(show);
            charactersListManager.Show(NftManager.Instance.GetAllNfts());
        }

        private void PopulateCharacterList()
        {
            List<Nft> nfts = NftManager.Instance.GetAllNfts();

            if (charactersListManager)
            {
                charactersListManager.Show(nfts);
            }

            //PlayerSpriteManager.Instance.CachePlayerSkinsAtStartup(nfts);
            //PortraitSpriteManager.Instance.CacheAllSprites();
            nftList.Clear();

            if (nfts.Count == 0)
            {
                portraitManager.SetDefault();
                playButton.interactable = false;
                return;
            }

            foreach (Nft nft in nfts)
            {
                nftList.AddLast(new ArmoryTokenData(nft));
            }

            if (panelContainer.activeSelf)
                GameManager.Instance.NftSelected(SelectedCharacter);
        }

        public void ActiveGearPanel(bool value) =>
            gearPanels.ForEach(go => go.SetActive(value));

        public void ResetCharacterSelectionUI()
        {
            loadingText.text = "";
            leftButton.interactable = true;
            rightButton.interactable = true;
        }

        public void ResetEquippedGear()
        {
            FindObjectOfType<ArmoryKnightRendererManager>(true).OnSkinLoading();

            StartCoroutine(Routine());
            IEnumerator Routine()
            {
                yield return null;
                ClearHeaders();
            }
        }
        
        public void PopulatePlayerGearInventory()
        {
            if (populateGearInventoryRoutine == null)
                populateGearInventoryRoutine = StartCoroutine(PopulatePlayerGearInventoryRoutine());
        }

        IEnumerator PopulatePlayerGearInventoryRoutine()
        {
            GearListManager.Clear();
            ClearHeaders();
            GearData data = null;
            yield return RequestService.Instance.GetRequestCoroutine(
                WebRequesterManager.Instance.ConstructUrl(RestEndpoint.PlayerGear),
                response =>
                {
                    data = FetchData.ParseJsonWithPath<GearData>(response, "data");
                },
                err =>
                {
                    Debug.LogError($"[Armory] Error getting player gear: {err}");
                }
            );

            if (data == null)
            {
                populateGearInventoryRoutine = null;
                yield break;
            }

            yield return PopulateGearList(data.ownedGear);

            populateGearInventoryRoutine = null;
        }

        public void NftSelected(Nft nft)
        {
            ActiveGearPanel(nft.Contract != NftContract.None);
            foreach (var gearSlot in gearSlots)
            {
                gearSlot.ResetSlot();
                equippedGear.Remove(gearSlot.gearTrait);
            }
        }

        private IEnumerator PopulateGearList(List<VictoryItems> ownedGear)
        {
            var callbacksPending = ownedGear.Count;

            foreach (VictoryItems itemData in ownedGear)
            {
                FetchData.Instance.GetArmoryGearImage(itemData.trait.ParseToEnum<Trait>(), itemData.name,
                    texture =>
                    {
                        GearListManager.AddGearItem(itemData, texture);
                        callbacksPending--;
                    }
                );
                yield return null;
            }

            while (callbacksPending > 0)
                yield return null;

            UpdateGearListBasedOnToken();
            yield return null;
            yield return GenerateHeaders();
        }

        private void UpdateGearListBasedOnToken()
        {
            GearListManager.UpdateGearListBasedOnToken(SelectedCharacter);
        }

        private IEnumerator GenerateHeaders()
        {
            GearListManager.GenerateHeaders();
            yield return null;
        }

        private void ClearHeaders()
        {
            foreach (var slots in gearSlots)
            {
                slots.ResetSlot();
                OnGearItemRemoved(slots.gearTrait);
            }

            foreach (ArmoryHeaderManager header in gearHeaders)
            {
                Destroy(header.gameObject);
            }

            gearHeaders.Clear();
            categoryLists.Clear();
        }

        //private void PopulateEquippedGear()
        //{
        //    if (SelectedCharacter.Contract == NftContract.Villager &&
        //        villagerEquippedGear.ContainsKey(SelectedCharacter.TokenId))
        //    {
        //        EquipGearInSlots(villagerEquippedGear[SelectedCharacter.TokenId]);
        //    }
        //    else if (SelectedCharacter.Contract == NftContract.BlessedVillager &&
        //             blessedVillagerEquippedGear.ContainsKey(SelectedCharacter.TokenId))
        //    {
        //        EquipGearInSlots(blessedVillagerEquippedGear[SelectedCharacter.TokenId]);
        //    }
        //    else
        //    {
        //        ClearGearSlots();
        //    }
        //}

        //private void EquipGearInSlots(List<VictoryItems> gear)
        //{
        //    foreach (GearSlot slot in gearSlots)
        //    {
        //        VictoryItems curGear = gear.Find(x => x.trait.ParseToEnum<Trait>() == slot.gearTrait);
        //        if (curGear != null)
        //        {
        //            slot.SetGearInSlot(curGear);
        //            continue;
        //        }

        //        slot.ResetSlot();
        //    }
        //}

        //private void ClearGearSlots()
        //{
        //    foreach (GearSlot slot in gearSlots)
        //    {
        //        slot.ResetSlot();
        //    }
        //}

        public void OnPlayButton()
        {
            if (charactersListManager.SelectedCharacter.CanPlay == false)
            {
                Debug.LogError("This character cant play");
                return;
            }

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

            var nft = charactersListManager.SelectedCharacter;

            ExpeditionStartData startData =
                await FetchData.Instance.RequestNewExpedition(nft.ContractAddress, nft.TokenId,
                    equippedGear.Values.ToList());
            if (startData.expeditionCreated)
            {
                //Debug.LogError(":::::::::::::::::::::::::::::::::::ON START EXPEDITION::::::::::::::::::::::::::::::::::::::" + nft.Contract + " " + nft.TokenId);
                OnExpeditionConfirmed();
                return;
            }

            if (!string.IsNullOrEmpty(startData.reason))
            {
                GameManager.Instance.EVENT_SHOW_CONFIRMATION_PANEL_WITH_FULL_CONTROL.Invoke(startData.reason, () => { },
                    () => { }, new[] { "Close", "" });
            }

            playButton.interactable = SelectedCharacter.CanPlay;
        }

        private void OnExpeditionConfirmed()
        {
            // play the correct music depending on where the player is
            GameManager.Instance.EVENT_PLAY_MUSIC.Invoke(MusicTypes.Music, 1);
            GameManager.Instance.EVENT_PLAY_MUSIC.Invoke(MusicTypes.Ambient, 1);
            //GameManager.Instance.LoadScene(inGameScenes.Expedition);
            postProcessingTransition.OnTransitionInEnd.AddListener(() =>
            {
                //GameManager.Instance.LoadScene(inGameScenes.Expedition, true);
                GameManager.Instance.LoadSceneNewTest();
            });
            postProcessingTransition.StartTransition();
        }

        public void OnGearItemSelected(VictoryItems activeItem)
        {
            if (SelectedCharacter.Contract == NftContract.Villager && !activeItem.CanVillagerEquip) return;

            Trait itemTrait = activeItem.trait.ParseToEnum<Trait>();
            gearSlots.Find(x => x.gearTrait == itemTrait).SetGearInSlot(activeItem);
            equippedGear[itemTrait] = activeItem;
            GameManager.Instance.UpdateNft(Enum.Parse<Trait>(activeItem.trait), activeItem.name);
        }

        public void OnGearItemRemoved(Trait gearTrait)
        {
            equippedGear.Remove(gearTrait);
            GameManager.Instance.UpdateNft(gearTrait, "");
        }

        public void InitiateBlessedVillager()
        {
            GameManager.Instance.ShowConfirmationPanel(
                $"Are you sure you want to initiate {SelectedCharacter.FormatTokenName()}?",
                () =>
                {
                    var sendJson = JsonConvert.SerializeObject(new InitiationInfo
                    {
                        eventName = "initiate",
                        data = new InitiationData()
                        {
                            TokenId = SelectedCharacter.TokenId,
                            Contract = SelectedCharacter.ContractAddress,
                            GearIds = equippedGear.Values.Select(x => x.gearId).ToArray(),
                            Wallet = WalletManager.Instance.ActiveWallet
                        }
                    });
                    Debug.Log($"Json: {sendJson}");
                    WebBridge.SendUnityMessage(sendJson, sendJson);
                });
        }

        public class InitiationInfo
        {
            public string eventName;
            public InitiationData data;
        }

        public class InitiationData
        {
            [JsonProperty("tokenId")] public int TokenId;
            [JsonProperty("contract")]  public string Contract;
            [JsonProperty("gearIds")]  public int[] GearIds;
            [JsonProperty("wallet")] public string Wallet;
        }
    }

    public class GearData
    {
        public List<VictoryItems> ownedGear;
        // TODO this isn't working quite right, it's not correctly being parsed into a js object
        // TODO so we're getting a string back instead of a json object
        //public List<GearItemData> equippedGear;
    }
}