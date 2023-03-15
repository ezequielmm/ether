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
            foreach (GearItemData item in gearList.ownedGear)
            {
                Trait itemTrait = item.trait.ParseToEnum<Trait>();
                if(IsIconCached(itemTrait, item.name)) continue;
                Sprite curSprite = await GetIcon($"{item.trait}/{item.name}");
                CacheIcon(itemTrait, item.name, curSprite);
            }
        }
        
        internal async UniTask RequestGearIcons(Nft metadata)
        {
            foreach (KeyValuePair<Trait, string> trait in metadata.Traits)
            {
                if(IsIconCached(trait.Key, trait.Value)) continue;
                Sprite curSprite = await GetIcon($"{trait.Key}/{trait.Value}");
                CacheIcon(trait.Key, trait.Value, curSprite);
            }
        }
        
        private async UniTask<Sprite> GetIcon(string itemName) 
        {
            Texture2D texture = await FetchData.Instance.GetArmoryGearImage(itemName);
            if (texture == null)
            {
                Debug.LogWarning($"Gear icon not found for {itemName}");
                return defaultImage;
            }
            return texture.ToSprite();
        }
    }
}