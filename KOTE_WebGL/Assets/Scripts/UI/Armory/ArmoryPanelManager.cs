using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using map;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KOTE.UI.Armory
{
    public class ArmoryPanelManager : MonoBehaviour
    {
        public CharactersListManager charactersListManager;

        public GameObject panelContainer;
        public Button playButton;
        public CharacterPortraitManager portraitManager;
        public TMP_Text TokenNameText;
        public TMP_Text CanPlayText;
        public ArmoryHeaderManager headerPrefab;
        public Transform gearListTransform;
        public List<GearSlot> gearSlots;

        public GameObject[] gearPanels;

        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;
        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] private GameObject loadingTextGearPanel;

        // making a reference to this since GetComponentInChildren only works on active gameObjects
        public ScrollRect gearListScroll;

        private LinkedList<ArmoryTokenData> nftList = new();
        private Dictionary<string, List<GearItemData>> categoryLists = new();
        private Dictionary<Trait, GearItemData> equippedGear = new();
        private Dictionary<int, List<GearItemData>> villagerEquippedGear = new();
        private Dictionary<int, List<GearItemData>> blessedVillagerEquippedGear = new();
        private List<ArmoryHeaderManager> gearHeaders = new();

        [SerializeField] private PostProcessingTransition postProcessingTransition;
        public const string MEMORY_REFRESH_MESSAGE = "refresh-armory";

        private Coroutine populateGearInventoryRoutine;

        private Nft SelectedCharacter => charactersListManager.SelectedCharacter;
        private Dictionary<string, Sprite> cachedSprites = new();
        
        private void Awake()
        {
            loadingText.text = "";
            loadingText.raycastTarget = false;

            NftManager.Instance.NftsLoaded.AddListener(PopulateCharacterList);

            WebBridge.OnWebMessageRecieved.AddListener(OnArmoryRefresh);
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                OnArmoryRefresh(MEMORY_REFRESH_MESSAGE);
            }
        }
#endif

        public void OnBridgeOpen()
        {
            WebBridge.SendUnityMessage("open-bridge", "open-bridge");
        }

        void OnArmoryRefresh(string data)
        {
            if (data != MEMORY_REFRESH_MESSAGE)
            {
                return;
            }

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

            gearListScroll.scrollSensitivity = GameSettings.PANEL_SCROLL_SPEED;
        }

        public void ActivateContainer(bool show)
        {
            // run this when the panel is opened, instead of when nfts load, so images are cached
            //try
            //{
            /*
                if (show)
                {
                    if (curNode != null && curNode.Value.MetaData != null)
                    {
                        GameManager.Instance.NftSelected(curNode.Value.MetaData);
                        UpdatePanelOnNftUpdate();
                    }
                }
*/
                panelContainer.SetActive(show);
                gameObject.SetActive(show);
                charactersListManager.Show(NftManager.Instance.GetAllNfts());
                
                
                //}
                //catch (Exception e)
                //{
                //    Debug.LogError(e);
                //}
        }

        private void PopulateCharacterList()
        {
            List<Nft> nfts = NftManager.Instance.GetAllNfts();
            
            if(charactersListManager){
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

        private void UpdatePanelOnNftUpdate()
        {
            Nft curMetadata = SelectedCharacter;
            TokenNameText.text = FormatTokenName(curMetadata);
            CanPlayText.text = curMetadata.CanPlay
                ? ""
                : $"Available in: {ParseTime((int)(curMetadata.PlayableAt - DateTime.UtcNow).TotalSeconds)}";
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

        public void ResetCharacterSelectionUI()
        {
            loadingText.text = "";
            leftButton.interactable = true;
            rightButton.interactable = true;
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

        public void PopulatePlayerGearInventory()
        {
            if (populateGearInventoryRoutine == null)
                populateGearInventoryRoutine = StartCoroutine(PopulatePlayerGearInventoryRoutine());
        }

        IEnumerator PopulatePlayerGearInventoryRoutine()
        {
            loadingTextGearPanel.gameObject.SetActive(true);
            ClearHeaders();
            GearData data = null;
            yield return RequestService.Instance.GetRequestCoroutine(
                WebRequesterManager.Instance.ConstructUrl(RestEndpoint.PlayerGear),
                response =>
                {
                    data = FetchData.ParseJsonWithPath<GearData>(response, "data");
                }, err => { Debug.LogError($"[Armory] Error getting player gear: {err}"); }
            );

            if (data == null)
            {
                populateGearInventoryRoutine = null;
                yield break;
            }

            yield return PopulateGearList(data.ownedGear);

            populateGearInventoryRoutine = null;
        }

        private IEnumerator PopulateGearList(List<GearItemData> ownedGear)
        {
            var callbacksPending = ownedGear.Count;

            foreach (GearItemData itemData in ownedGear)
            {
                FetchData.Instance.GetArmoryGearImage(itemData.trait.ParseToEnum<Trait>(), itemData.name,
                    texture =>
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

                        if (categoryLists.ContainsKey(itemData.category))
                        {
                            // Debug.Log($"[Armory] categoryLists contains {itemData.category}");
                            categoryLists[itemData.category].Add(itemData);
                        }
                        else
                        {
                            // Debug.Log($"[Armory] categoryLists does not contain {itemData.category}");
                            categoryLists[itemData.category] = new List<GearItemData> { itemData };
                        }

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
            loadingTextGearPanel.gameObject.SetActive(false);
        }

        private void UpdateGearListBasedOnToken()
        {
            foreach (ArmoryHeaderManager header in gearHeaders)
            {
                if (SelectedCharacter != null)
                    header.UpdateGearSelectableStatus(SelectedCharacter.Contract);
                else
                {
                    Debug.Log($"This node is null");
                }
            }
        }

        private IEnumerator GenerateHeaders()
        {
            // Debug.Log($"[Armory] GenerateHeaders : {categoryLists.Keys.Count}");
            foreach (string category in categoryLists.Keys)
            {
                ArmoryHeaderManager header = Instantiate(headerPrefab, gearListTransform);
                // Debug.Log($"[Armory] GenerateHeaders header : {header}");
                if (categoryLists.ContainsKey(category))
                {
                    header.Populate(category, categoryLists[category]);
                    gearHeaders.Add(header);
                }

                yield return null;
            }
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

        private void PopulateEquippedGear()
        {
            if (SelectedCharacter.Contract == NftContract.Villager &&
                villagerEquippedGear.ContainsKey(SelectedCharacter.TokenId))
            {
                EquipGearInSlots(villagerEquippedGear[SelectedCharacter.TokenId]);
            }
            else if (SelectedCharacter.Contract == NftContract.BlessedVillager &&
                     blessedVillagerEquippedGear.ContainsKey(SelectedCharacter.TokenId))
            {
                EquipGearInSlots(blessedVillagerEquippedGear[SelectedCharacter.TokenId]);
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
                await FetchData.Instance.RequestNewExpedition(nft.Contract, nft.TokenId,
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

        public void OnGearItemSelected(GearItemData activeItem)
        {
            if (SelectedCharacter.Contract == NftContract.Knights ||
                (SelectedCharacter.Contract == NftContract.Villager && !activeItem.CanVillagerEquip)) return;

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