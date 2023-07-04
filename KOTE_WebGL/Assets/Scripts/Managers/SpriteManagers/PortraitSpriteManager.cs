using System;
using UnityEngine;

public class PortraitSpriteManager : SingleTon<PortraitSpriteManager>
{
    public void GetKnightPortrait(Nft nft, Action<Sprite> callback) => 
        nft.GetImage(callback);
}
