using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
    [JsonProperty("attributes")]
    [JsonConverter(typeof(TraitDictionaryConverter))]
    public Dictionary<Trait, string> Traits;
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
        Dictionary<Trait, string> result = new();
        if (reader.TokenType == JsonToken.Null)
        {
            return result;
        }
        JArray jArray = JArray.Load(reader);
        foreach (JObject jObject in jArray) 
        {
            string TraitName = jObject.SelectToken("trait_type").ToObject<string>();
            if (Enum.TryParse(TraitName, true, out Trait trait)) 
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
        return typeof(Dictionary<Trait, string>) == objectType;
    }
}
