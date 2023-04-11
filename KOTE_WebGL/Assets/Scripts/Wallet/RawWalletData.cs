using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class RawWalletData
{
    [JsonProperty("tokens")] public List<ContractData> Contracts = new();
}

[Serializable]
public class ContractData
{
    public string contract_address;
    public string characterClass;
    public int token_count;
    public List<TokenData> tokens = new();

    [JsonIgnore]
    public NftContract ContractType
    {
        get
        {
            if (tokens?.Count <= 0) return NftContract.None;
            switch (characterClass)
            {
                case "BlessedVillager":
                case "blessed-villager":
                    return NftContract.BlessedVillager;
                case "Villager":
                case "villager":
                    return NftContract.Villager;
                case "KnightsOfTheEther":
                case "Knights":
                case "knight":
                    return NftContract.Knights;
                default:
                    return NftContract.None;
            }
        }
    }
}

[Serializable]
public class TokenData
{
    public string token_id;
    public string name;
    public bool can_play;
    public Nft metadata;
}