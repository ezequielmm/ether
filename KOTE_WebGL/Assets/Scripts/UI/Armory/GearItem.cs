using Cysharp.Threading.Tasks;
using KOTE.UI.Armory;
using UnityEngine;
using UnityEngine.UI;

public class GearItem : MonoBehaviour
{
    [SerializeField]
    public Image GearImage;
    [HideInInspector]
    public GearItemData ItemData;

    public async virtual void Populate(GearItemData newItemData)
    {
        ItemData = newItemData;
        if (newItemData.gearImage == null)
        {
            await ItemData.GetGearImage();
        }
        Sprite ImageSprite = ItemData.gearImage;
        GearImage.sprite = ImageSprite;
    }
}

public class GearItemData
{
    public int gearId;
    public string name;
    public string trait;
    public string category;
    public Sprite gearImage;

    public async UniTask GetGearImage() 
    {
        gearImage = 
            await GearIconManager.Instance.GetGearSprite(Utils.ParseEnum<Trait>(trait), name);
    }
}