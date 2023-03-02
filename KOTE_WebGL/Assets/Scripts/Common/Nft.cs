using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nft
{
    public int TokenId { get; private set; }
    public string Wallet { get; private set; }
    public NftContract Contract { get; private set; }
    public string ImageUrl { get; private set; }
    public string ImageThumbnailUrl { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Dictionary<Trait, string> Traits { get; private set; }

    private Nft() { }

    public Nft GetNft(int TokenId, NftContract Contract)
    {

    }
}