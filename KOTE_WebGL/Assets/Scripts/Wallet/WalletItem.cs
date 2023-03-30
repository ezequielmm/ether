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

    public void SetTokenCounts(int knightCount, int villagerCount, int bVillagerCount)
    {
        knightCountText.text = knightCount + " Knights";
        villagerCountText.text = villagerCount + " Villagers";
        blessedVillagerCountText.text = bVillagerCount + " Blessed Villagers";
    }

    public void Populate(string wallet, int knights, int villagers, int blessedVillagers)
    {
        SetWalletAddress(wallet);
        SetTokenCounts(knights, villagers, blessedVillagers);
    }

    public void RemoveWallet()
    {
        WalletManager.Instance.ForgetWallet(WalletAddress);
    }
}