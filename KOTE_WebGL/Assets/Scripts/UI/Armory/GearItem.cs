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
    public RewardsLoot RewardsLoot;
    

    public void Populate(GearItemData newItemData)
    {
        ItemData = newItemData;
        Action inner = () => {
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
        };
        if (newItemData.gearImage == null)
        {
            ItemData.GetGearImage(inner);
        }
        else
            inner();
    }
    
    private string FormatTraitText()
    {
        if (ItemData.trait == "Weapon") return "";
        return " " + ItemData.trait;
    }

    public void Populate(RewardsLoot newItemData)
    {
        RewardsLoot = newItemData;
        Action inner = () => {
            Sprite ImageSprite = RewardsLoot.gearImage;
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
                    title = $"{RewardsLoot.name}"
                }
            });
        };
        
        if (newItemData.gearImage == null)
        {
            FetchData.Instance.GetTexture(newItemData.image, (texture) => {
                newItemData.gearImage = texture?.ToSprite();
                inner();
            });
        }
        else
            inner();
    }
}

[Serializable]
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

    public void GetGearImage(Action callback) 
    {
        FetchData.Instance.GetArmoryGearImage(trait.ParseToEnum<Trait>(), name, (texture) => {
            gearImage = texture?.ToSprite();
            callback?.Invoke();
        });
    }
}

public class RewardsLoot
{
    public string name;
    public string image;
    public Sprite gearImage;
}

public enum GearRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
    iOP
}