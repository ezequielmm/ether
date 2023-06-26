using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class SpriteAssetManager : SingleTon<SpriteAssetManager>
{
    public Sprite defaultImage;
    public List<SpriteList> potionImageList;
    public SpriteList trinketImageList;
    public List<NamedSpriteList> combatBackgroundList;
    public NamedSpriteList encounterCreatureList;
    public NamedSpriteList miscImages;
    public NamedSpriteList statusIcons;
    private List<(int, int)> _potionListRanges = new List<(int, int)>();
    private Dictionary<int, Sprite> trinketMap = new Dictionary<int, Sprite>();
    
    Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    private void Start()
    {
        // the purpose of this Start() is to cache the image range for any item that has multiple lists of images

        // cache the range of potion ids to check against when a card is asked for
        foreach (SpriteList imageList in potionImageList)
        {
            _potionListRanges.Add((int.Parse(imageList.entityImages[0].name),
                int.Parse(imageList.entityImages[imageList.entityImages.Count - 1].name)));
        }

        foreach (var sprite in trinketImageList.entityImages)
        {
            // get sprite name
            var name = sprite.name;
            var id = int.TryParse(name, out var idInt) ? idInt : -1;
            if (id == -1)
            {
                Debug.LogWarning($"[SpriteAssetManager] Trinket {name} is not a valid trinket id");
                continue;
            }
            trinketMap.Add(id, sprite);
        }
    }

    public Sprite GetStatusIcon(STATUS status)
    {
        foreach (var icon in statusIcons.SpriteList) 
        {
            if (icon.name.ToLower().Trim() == status.ToString().ToLower()) 
            {
                return icon.image;
            }
        }
        if(status != STATUS.unknown) 
        {
            return GetStatusIcon(STATUS.unknown);
        }
        return null;
    }

    public Sprite GetPotionImage(int potionId)
    {
        for (int i = 0; i < potionImageList.Count; i++)
        {
            var range = _potionListRanges[i];
            if (potionId < range.Item1 || potionId > range.Item2)
            {
                continue;
            }

            List<Sprite> cardImages = potionImageList[i].entityImages;
            if (cardImages.Exists(image => int.Parse(image.name) == potionId))
            {
                return cardImages.Find(image => int.Parse(image.name) == potionId);
            }
        }

        Debug.LogWarning($"[SpriteAssetManager] No potion image for potion ID {potionId} found. You probably need to pester the backend");
        return defaultImage;
    }
    public Sprite GetTrinketImage(int trinketId)
    {
        trinketMap.TryGetValue(trinketId, out var sprite);
        if (sprite != null)
        {
            return sprite;
        }
        Debug.LogWarning($"[SpriteAssetManager] No trinket image for trinket ID {trinketId} found. You probably need to pester the backend");
        return defaultImage;
    }

    public Sprite GetMiscImage(string imageName)
    {
        if (miscImages.SpriteList.Exists(item =>
                item.name.Equals(imageName, StringComparison.CurrentCultureIgnoreCase)))
        {
            return miscImages.SpriteList
                .Find(item => item.name.Equals(imageName, StringComparison.CurrentCultureIgnoreCase)).image;
        }

        Debug.LogWarning($"no misc image with name {imageName} was found. Check that you are using a valid image name");
        return defaultImage;
    }

    public Sprite GetCombatBackground(int act, int step)
    {
        NamedSpriteList actBackgrounds = combatBackgroundList[act];
        
        for (int i = actBackgrounds.SpriteList.Count - 1; i >= 0; i--)
        {
            // if the step is less than the step for the background, continue
            if (step < int.Parse(actBackgrounds.SpriteList[i].name))
            {
                continue;
            }

            // else set that as the background and continue
           return actBackgrounds.SpriteList[i].image;
            break;
        }

        Debug.LogError($"No background found for act {act} step {step}");
        return defaultImage;
    }

    public void GetEncounterCreature(string name, Action<Sprite> onSuccess)
    {
        if (cache.ContainsKey(name))
        {
            onSuccess?.Invoke(cache[name]);
            return;
        }
        
        if (encounterCreatureList.SpriteList.Exists(item => item.name == name))
        {
            Debug.Log($"Found: {name}");
            var namedSprite = encounterCreatureList.SpriteList.Find(x => x.name == name);
            if (namedSprite.image != null)
            {
                cache.Add(name, namedSprite.image);
                onSuccess?.Invoke(namedSprite.image);
            }
            else
            {
                StartCoroutine(LoadSprite(
                    namedSprite.imageRef,
                    (sprite) =>
                    {
                        onSuccess?.Invoke(sprite);
                        cache.Add(name, sprite);
                    }));
            }
        }
        else
        {
            Debug.LogError($"No image found for encounter creature {name}");
            onSuccess?.Invoke(encounterCreatureList.SpriteList[0].image);
            cache.Add(name, encounterCreatureList.SpriteList[0].image);
        }
    }

    IEnumerator LoadSprite(AssetReference assetReference, Action<Sprite> onSuccess)
    {
        var async = assetReference.LoadAssetAsync<Sprite>();
        while (!async.IsDone)
            yield return null;

        Debug.Log($"Rresult: {async.Result}");
        onSuccess.Invoke(async.Result);
    }
}