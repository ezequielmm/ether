using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class RawWalletData
{
    public List<ContractData> tokens;
}

public class ContractData
{
    public string contract_address;
    public int token_count;
    public List<TokenData> tokens;
    [JsonIgnore] public NftContract ContractType => Utils.ParseEnum<NftContract>(tokens[0]?.name);
}

[Serializable]
public class TokenData
{
    public string token_id;
    public string name;
    public Nft metadata;
}