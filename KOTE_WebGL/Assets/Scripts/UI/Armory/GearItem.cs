using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using KOTE.UI.Armory;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class GearItem : MonoBehaviour
{
    [SerializeField]
    public Image GearImage;

    public TooltipAtCursor tooltip;
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
        
        tooltip.SetTooltips(new List<Tooltip>
        {
            new Tooltip
            {
                title = $"{ItemData.name}{FormatTraitText()}"
            }
        });
    }
    
    private string FormatTraitText()
    {
        if (ItemData.trait == "Weapon") return "";
        return " " + ItemData.trait;
    }
}

public class GearItemData
{
    public int gearId;
    public string name;
    public string trait;
    public string category;
    public GearRarity rarity; // TODO: should be an enum
    [JsonIgnore]
    public Sprite gearImage;

    [JsonIgnore] public bool CanVillagerEquip => rarity == GearRarity.Common || rarity == GearRarity.Uncommon;

    public async UniTask GetGearImage() 
    {
        gearImage = 
            await GearIconManager.Instance.GetGearSprite(trait.ParseToEnum<Trait>(), name);
    }
}

public enum GearRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}