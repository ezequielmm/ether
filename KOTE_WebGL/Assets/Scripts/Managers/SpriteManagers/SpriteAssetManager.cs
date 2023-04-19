using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private List<(int, int)> _trinketListRanges = new List<(int, int)>();

    private void Start()
    {
        // the purpose of this Start() is to cache the image range for any item that has multiple lists of images

        // cache the range of potion ids to check against when a card is asked for
        foreach (SpriteList imageList in potionImageList)
        {
            _potionListRanges.Add((int.Parse(imageList.entityImages[0].name),
                int.Parse(imageList.entityImages[imageList.entityImages.Count - 1].name)));
        }

        _trinketListRanges.Add((int.Parse(trinketImageList.entityImages[0].name),
            int.Parse(trinketImageList.entityImages[trinketImageList.entityImages.Count - 1].name)));
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

        Debug.LogWarning($"No potion image for potion ID {potionId} found. You probably need to pester the backend");
        return defaultImage;
    }
    public Sprite GetTrinketImage(int trinketId)
    {
        for (int i = 0; i < trinketImageList.Count; i++)
        {
            var range = _trinketListRanges[i];
            if (trinketId < range.Item1 || trinketId > range.Item2)
            {
                continue;
            }

            List<Sprite> cardImages = trinketImageList[i].entityImages;
            if (cardImages.Exists(image => int.Parse(image.name) == trinketId))
            {
                return cardImages.Find(image => int.Parse(image.name) == trinketId);
            }
        }

        Debug.LogWarning($"No trinket image for trinket ID {trinketId} found. You probably need to pester the backend");
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

    public Sprite GetEncounterCreature(string name)
    {
        if (encounterCreatureList.SpriteList.Exists(item => item.name == name))
        {
            return encounterCreatureList.SpriteList.Find(x => x.name == name).image;
        }

        Debug.LogError($"No image found for encounter creature {name}");
        return encounterCreatureList.SpriteList[0].image;
    }
}