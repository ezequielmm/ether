using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KOTE.UI.Armory
{
    public class ArmoryPanelManager : MonoBehaviour
    {
        public GameObject panelContainer;
        public Button playButton;
        public Image nftImage;
        private LinkedListNode<ArmoryTokenData> curNode;
        private LinkedList<ArmoryTokenData> nftList = new();

        private void Start()
        {
            panelContainer.SetActive(false);
            GameManager.Instance.EVENT_EXPEDITION_CONFIRMED.AddListener(OnExpeditionConfirmed);
            GameManager.Instance.EVENT_ARMORYPANEL_ACTIVATION_REQUEST.AddListener(ActivateContainer);
            GameManager.Instance.EVENT_NFT_METADATA_RECEIVED.AddListener(PopulateCharacterList);
        }

        private void ActivateContainer(bool show)
        {
            panelContainer.SetActive(show);
        }

        private void PopulateCharacterList(NftData heldNftData)
        {
            foreach (NftMetaData nftMetaData in heldNftData.assets)
            {
                nftList.AddLast(new ArmoryTokenData(nftMetaData));
            }

            curNode = nftList.First;
            curNode.Value.tokenImageReceived.AddListener(UpdateCharacterImage);
            UpdateCharacterImage();
        }

        private void UpdateCharacterImage()
        {
            nftImage.sprite = curNode.Value.NftImage;
        }

        public void OnPreviousToken()
        {
            if (curNode.Previous == null) return;
            curNode.Value.tokenImageReceived.RemoveListener(UpdateCharacterImage);
            curNode = curNode.Previous;
            curNode.Value.tokenImageReceived.AddListener(UpdateCharacterImage);
            UpdateCharacterImage();
        }

        public void OnNextToken()
        {
            if (curNode.Next == null) return;
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
            GameManager.Instance.webRequester.RequestStartExpedition("knight",
                curNode.Value.Id); //for the moment this is hardcoded
        }

        private void OnExpeditionConfirmed()
        {
            // play the correct music depending on where the player is
            GameManager.Instance.EVENT_PLAY_MUSIC.Invoke(MusicTypes.Music, 1);
            GameManager.Instance.EVENT_PLAY_MUSIC.Invoke(MusicTypes.Ambient, 1);
            GameManager.Instance.LoadScene(inGameScenes.Expedition);
        }
    }
}