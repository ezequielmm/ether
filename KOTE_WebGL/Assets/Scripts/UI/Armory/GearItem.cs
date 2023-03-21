using System;
using Cysharp.Threading.Tasks;
using KOTE.UI.Armory;
using Newtonsoft.Json;
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
        if (GearImage == null)
        {
            Debug.LogError($"[GearItem] GearImage on gameobject [{gameObject.name}] is null.");
            return;
        }
        GearImage.sprite = ImageSprite;
    }
}

public class GearItemData
{
    public int gearId;
    public string name;
    public string trait;
    public string category;
    public string rarity; // TODO: should be an enum
    [JsonIgnore]
    public Sprite gearImage;

    public async UniTask GetGearImage() 
    {
        gearImage = 
            await GearIconManager.Instance.GetGearSprite(trait.ParseToEnum<Trait>(), name);
    }
}