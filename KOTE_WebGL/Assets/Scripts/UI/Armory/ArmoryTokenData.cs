using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

[assembly: InternalsVisibleTo("PlayModeTests")]

namespace KOTE.UI.Armory
{
    internal class ArmoryTokenData
    {
        public int Id => MetaData.TokenId;
        public Nft MetaData { get; }
        public Sprite NftImage => MetaData.Image;

        public ArmoryTokenData(Nft tokenMetadata)
        {
            MetaData = tokenMetadata;
        }
    }
}