using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace KOTE.UI.Armory
{
    public class GearIconManager : SingleTon<GearIconManager>
    {
        public Sprite defaultImage;
        // cache for already downloaded images
        private Dictionary<Trait, Dictionary<string, Sprite>> iconCache = new();

        public Sprite GetGearSprite(Trait gearType, string itemName)
        {
            if (IsIconCached(gearType, itemName))
            {
                return iconCache[gearType][itemName];
            }

            //TODO request correct image
            return defaultImage;
        }

        private bool IsIconCached(Trait gearType, string itemName)
        {
            return iconCache.ContainsKey(gearType) && iconCache[gearType].ContainsKey(itemName);
        }

        private void CacheIcon(Trait traitType, string itemName, Sprite iconSprite)
        {
            if (iconCache.ContainsKey(traitType))
            {
                iconCache[traitType][itemName] = iconSprite;
                return;
            }

            iconCache[traitType] = new Dictionary<string, Sprite>();
            iconCache[traitType][itemName] = iconSprite;
        }

        internal async UniTask RequestGearIcons(GearData gearList)
        {
            foreach (GearItemData item in gearList.data.ownedGear)
            {
                Trait itemTrait = Utils.ParseEnum<Trait>(item.trait);
                if(IsIconCached(itemTrait, item.name)) continue;
                Sprite curSprite = await GetIcon($"{item.trait}/{item.name}");
                CacheIcon(itemTrait, item.name, curSprite);
            }
        }
        
        private async UniTask<Sprite> GetIcon(string itemName) 
        {
            Texture2D texture = await FetchData.Instance.GetArmoryGearImage(itemName);
            return texture.ToSprite();
        }
    }
}