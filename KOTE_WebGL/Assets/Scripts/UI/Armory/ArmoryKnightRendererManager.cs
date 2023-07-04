using KOTE.UI.Armory;
using UnityEngine;

public class ArmoryKnightRendererManager : MonoBehaviour
{
    public PlayerSkinManager knightManager;
    public GameObject knight, loadingText;

    private void Start()
    {
        PlayerSpriteManager.Instance.skinLoading.AddListener(OnSkinLoading);
        knightManager.skinLoaded.AddListener(OnSkinLoaded);
    }

    public void OnSkinLoading()
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