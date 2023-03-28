using UnityEngine;

public class ArmoryKnightRendererManager : MonoBehaviour
{
    public PlayerSkinManager knightManager;
    public GameObject knight, loadingText;

    private void Start()
    {
        GameManager.Instance.EVENT_NFT_SELECTED.AddListener(OnSkinLoading);
        knightManager.skinLoaded.AddListener(OnSkinLoaded);
    }

    private void OnSkinLoading(Nft data)
    {
        knight.SetActive(false);
        loadingText.SetActive(true);
    }

    private void OnSkinLoaded()
    {
        knight.SetActive(true);
        loadingText.SetActive(false);
    }
}