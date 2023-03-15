using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class RawWalletData
{
    [JsonProperty("tokens")]
    public List<ContractData> Contracts = new();
}

[Serializable]
public class ContractData
{
    public string contract_address;
    public int token_count;
    public List<TokenData> tokens = new();

    [JsonIgnore]
    public NftContract ContractType => (tokens[0] == null)
        ? NftContract.None
        : tokens[0].name.ParseToEnum<NftContract>();
}

[Serializable]
public class TokenData
{
    public string token_id;
    public string name;
    public bool can_play;
    public Nft metadata;
}