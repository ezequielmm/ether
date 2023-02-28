using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionManager : MonoBehaviour
{
    public Button startExpeditionButton;
    public GameObject characterSelectionContainer;

    [Space] [Header("Class Select")] public List<GameObject> characterBorders;

    [Space] [Header("NFT Select")] public GameObject nftSelectionLayout;
    public GameObject nftSelectItemPrefab;

    private GameObject currentClass;
    private SelectableNftManager selectedNft;

    private void Start()
    {
        //GameManager.Instance.webRequester.RequestCharacterList();// we are not requesting the list until we have more than one type so for the moment only knight

        GameManager.Instance.EVENT_WALLET_DISCONNECTED.AddListener(OnWalletDisconnected);
        GameManager.Instance.EVENT_REQUEST_NFT_METADATA.AddListener(OnMetadataRequest);
        GameManager.Instance.EVENT_CHARACTERSELECTIONPANEL_ACTIVATION_REQUEST.AddListener(
            ActivateInnerCharacterSelectionPanel);
        GameManager.Instance.EVENT_EXPEDITION_CONFIRMED.AddListener(OnExpeditionConfirmed);
        GameManager.Instance.EVENT_NFT_METADATA_RECEIVED.AddListener(PopulateNftPanel);

        startExpeditionButton.interactable = false;
    }

    private void ActivateInnerCharacterSelectionPanel(bool activate)
    {
        characterSelectionContainer.SetActive(activate);
    }

    private void OnWalletDisconnected()
    {
        ClearNfts();
    }

    // when a new wallet is received clear the panel
    private void OnMetadataRequest(int[] ids)
    {
        ClearNfts();
    }

    private void ClearNfts()
    {
        for (int i = 0; i < nftSelectionLayout.transform.childCount; i++)
        {
            Destroy(nftSelectionLayout.transform.GetChild(i).gameObject);
        }
    }

    private void PopulateNftPanel(NftData heldNftData)
    {
        foreach (NftMetaData metaData in heldNftData.assets)
        {
            GameObject localObject = Instantiate(nftSelectItemPrefab, nftSelectionLayout.transform);
            SelectableNftManager currentNft = localObject.GetComponent<SelectableNftManager>();
            currentNft.Populate(metaData, (isOn) =>
            {
                if (!currentNft.isSelected)
                {
                    if (selectedNft == null)
                    {
                        selectedNft = currentNft;
                        startExpeditionButton.interactable = true;
                        currentNft.isSelected = true;
                    }
                    else if (selectedNft != currentNft)
                    {
                        // clear the previous selected nft
                        selectedNft.isSelected = false;
                        selectedNft.DetermineToggleColor();
                        // and set the current nft as the new one
                        selectedNft = currentNft;
                        startExpeditionButton.interactable = true;
                        currentNft.isSelected = true;
                    }
                }
                else if (currentNft.isSelected && selectedNft == currentNft)
                {
                    selectedNft = null;
                    startExpeditionButton.interactable = false;
                    currentNft.isSelected = false;
                }

                currentNft.DetermineToggleColor();
            });
        }
    }

    public void OnStartExpedition()
    {
        startExpeditionButton.enabled = false;
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        GameManager.Instance.EVENT_NFT_SELECTED.Invoke(selectedNft.internalPrefab.metaData);
        WebRequesterManager.Instance.RequestStartExpedition("knight", selectedNft.internalPrefab.metaData.token_id); //for the moment this is hardcoded
    }

    private void OnExpeditionConfirmed()
    {
        // play the correct music depending on where the player is
        GameManager.Instance.EVENT_PLAY_MUSIC.Invoke(MusicTypes.Music, 1);
        GameManager.Instance.EVENT_PLAY_MUSIC.Invoke(MusicTypes.Ambient, 1);
        GameManager.Instance.LoadScene(inGameScenes.Expedition);
    }

    public void OnArmoryButton()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        GameManager.Instance.EVENT_SHOW_ARMORY_PANEL.Invoke(true);
        ActivateInnerCharacterSelectionPanel(false);
    }

    public void OnBackButton()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        ActivateInnerCharacterSelectionPanel(false);
    }

    // Leaving this so we still have it when we need to do class selections

    #region ClassSelect

    public void OnCharacterSelected(GameObject currentClassBorder)
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        startExpeditionButton.interactable = true;

        foreach (GameObject classBorder in characterBorders)
        {
            classBorder.SetActive(classBorder == currentClassBorder);
        }
    }

    public void SetSelectedCharacter(GameObject selectedClass)
    {
        currentClass = selectedClass;
    }

    #endregion
}