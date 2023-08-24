using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Nft
{
    [JsonProperty("token_id")]
    public int TokenId;
    public NftContract Contract;
    [JsonProperty("image_url")]
    public string ImageUrl;
    [JsonProperty("name")]
    public string Name;
    [JsonProperty("description")]
    public string Description;

    private Dictionary<Trait, string> trait;
    public Dictionary<Trait, string> Traits
    {
        set => trait = value;
        get =>
            trait ?? (trait = attributes.ToDictionary(k => (Trait)Enum.Parse(typeof(Trait),
                k.Key.ToString() switch
                {
                    "Breastplates" => "Breastplate",
                    "Helmets" => "Helmet",
                    "Weapons" => "Weapon",
                    "Gauntlets" => "Gauntlet",
                    "Paddings" => "Padding",
                    "Crests" => "Crest",
                    "Shields" => "Shield",
                    "Sigils" => "Sigil",
                    "Vambraces" => "Vambrace",
                    "Legguards" => "Legguard",
                    "Upper_Paddings" => "Upper_Padding",
                    "Lower_Paddings" => "Lower_Padding",
                    _ => k.Key.ToString()
                }), v => v.Value));
    }

    [JsonProperty("attributes")]
    [JsonConverter(typeof(TraitDictionaryConverter))]
    public Dictionary<TraitsParse, string> attributes;
    [JsonProperty("can_play")]
    public bool CanPlay;
    public string adaptedImageURI;

    [JsonIgnore]
    public Sprite Image = null;
    
    public bool isKnight => Contract == NftContract.Knights;
    public DateTime PlayableAt => DateTime.Today + TimeSpan.FromHours(24);

    public void GetImage(Action<Sprite> callback) 
    {
        if (/*Image != null ||*/ string.IsNullOrEmpty(adaptedImageURI)) 
        {
            callback?.Invoke(null);
            return;
        }

        FetchData.Instance.GetTexture(adaptedImageURI, (texture) => {
            Image = texture ? texture.ToSprite() : null;
            callback?.Invoke(Image);
        });
    }
}

public class TraitDictionaryConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        JToken t = JToken.FromObject(value);
        t.WriteTo(writer);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        Dictionary<TraitsParse, string> result = new();
        if (reader.TokenType == JsonToken.Null)
        {
            return result;
        }
        JArray jArray = JArray.Load(reader);
        foreach (JObject jObject in jArray) 
        {
            string TraitName = jObject.SelectToken("trait_type").ToObject<string>();
            if (Enum.TryParse(TraitName, true, out TraitsParse trait)) 
            {
                string TraitValue = jObject.SelectToken("value").ToObject<string>();
                result.Add(trait, TraitValue);
            }
        }
        return result;
    }

    public override bool CanRead => true;
    public override bool CanConvert(Type objectType)
    {
        return typeof(Dictionary<TraitsParse, string>) == objectType;
    }
}
