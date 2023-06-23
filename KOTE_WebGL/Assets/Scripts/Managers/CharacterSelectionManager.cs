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

        WalletManager.Instance.DisconnectingWallet.AddListener(ClearOutNfts);
        WalletManager.Instance.NewWalletConfirmed.AddListener(ClearOutNfts);
        GameManager.Instance.EVENT_CHARACTERSELECTIONPANEL_ACTIVATION_REQUEST.AddListener(
            ActivateInnerCharacterSelectionPanel);
        NftManager.Instance.NftsLoaded.AddListener(PopulateNftPanel);

        startExpeditionButton.interactable = false;
    }

    private void ActivateInnerCharacterSelectionPanel(bool activate)
    {
        characterSelectionContainer.SetActive(activate);
    }

    private void ClearOutNfts(string walletAddress)
    {
        ClearNfts();
    }

    private void ClearOutNfts(RawWalletData walletData)
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

    private void PopulateNftPanel()
    {
        List<Nft> nfts = NftManager.Instance.GetAllNfts();
        foreach (Nft nft in nfts)
        {
            GameObject localObject = Instantiate(nftSelectItemPrefab, nftSelectionLayout.transform);
            SelectableNftManager currentNft = localObject.GetComponent<SelectableNftManager>();
            currentNft.Populate(nft, (isOn) =>
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


    public void OnArmoryButton()
    {
        Debug.Log($"[{this.GetType().Name}] OnArmoryButton");
        return;
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        GameManager.Instance.ShowArmoryPanel(true);
        ActivateInnerCharacterSelectionPanel(false);
    }

    public void OnBackButton()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        ActivateInnerCharacterSelectionPanel(false);
    }

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
}