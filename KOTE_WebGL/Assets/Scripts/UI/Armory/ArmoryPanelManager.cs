using System;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace KOTE.UI.Armory
{
    public class ArmoryPanelManager : MonoBehaviour
    {
        public GameObject panelContainer;
        public Button playButton;
        public Image nftImage;
        public ArmoryHeaderManager headerPrefab;
        public Transform gearListTransform;
        private LinkedListNode<ArmoryTokenData> curNode;
        private LinkedList<ArmoryTokenData> nftList = new();

        private void Start()
        {
            panelContainer.SetActive(false);
            GameManager.Instance.EVENT_EXPEDITION_CONFIRMED.AddListener(OnExpeditionConfirmed);
            GameManager.Instance.EVENT_SHOW_ARMORY_PANEL.AddListener(ActivateContainer);
            GameManager.Instance.EVENT_NFT_METADATA_RECEIVED.AddListener(PopulateCharacterList);
            GameManager.Instance.EVENT_GEAR_RECEIVED.AddListener(PopulateGearList);
        }

        private void ActivateContainer(bool show)
        {
            panelContainer.SetActive(show);
            
            PopulateDummyData();
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

        private void PopulateGearList(string rawData)
        {
            GearData data = JsonConvert.DeserializeObject<GearData>(rawData);
            string[] categoryNames = Enum.GetNames(typeof(GearCategories));
            for (int i = 0; i < categoryNames.Length; i++)
            {
                if (data.categories.ContainsKey(categoryNames[i]))
                {
                    ArmoryHeaderManager curHeader = Instantiate(headerPrefab, gearListTransform);
                    curHeader.Populate(categoryNames[i]);
                }
            }
        }
        
        // MOCKUP CODE, DELETE WHEN BACKEND SENDS MESSAGES
        private void PopulateDummyData()
        {
            string[] categoryNames = Enum.GetNames(typeof(GearCategories));

            for (int i = 0; i < categoryNames.Length; i++)
            {
                ArmoryHeaderManager curHeader = Instantiate(headerPrefab, gearListTransform);
                curHeader.Populate(categoryNames[i]);
            }
        }
        // END MOCKUP CODE +++++++++++++++++++++++++++++++++++++++++

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

        private class GearItem
        {
            public TraitTypes Trait;
            public string ItemName;
            public Sprite GearImage;
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
        public Dictionary<string, GearItemData> categories;
    }
}