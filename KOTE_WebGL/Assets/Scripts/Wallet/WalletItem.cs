using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WalletItem : MonoBehaviour
{
    public string WalletAddress { get; private set; }
    
    public bool IsActiveWallet => WalletManager.Instance.ActiveWallet == WalletAddress;

    [SerializeField] private TMP_Text walletAddressText;
    [SerializeField] private TMP_Text knightCountText;
    [SerializeField] private TMP_Text villagerCountText;
    [SerializeField] private TMP_Text blessedVillagerCountText;

    public void SetWalletAddress(string wallet)
    {
        walletAddressText.text = wallet;
        WalletAddress = wallet;
    }

    public void SetTokenCounts(Dictionary<NftContract, int> tokenCounts )
    {
        knightCountText.text = tokenCounts[NftContract.Knights] + " Knights";
        villagerCountText.text = tokenCounts[NftContract.Villager] + " Villagers";
        blessedVillagerCountText.text = tokenCounts[NftContract.BlessedVillager] + " Blessed Villagers";
    }

    public void Populate(string wallet, Dictionary<NftContract, int> tokenCounts )
    {
        SetWalletAddress(wallet);
        SetTokenCounts(tokenCounts );
    }

    public void RemoveWallet()
    {
        WalletManager.Instance.ForgetWallet(WalletAddress);
    }
}