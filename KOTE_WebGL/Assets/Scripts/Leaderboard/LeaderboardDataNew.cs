using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DefaultNamespace.Leaderboard.New
{
    public class Achievement
    {
        [JsonProperty("name")]
        public string name;

        [JsonProperty("score")]
        public int score;
    }

    public class DataItem
    {
        [JsonProperty("score")]
        public int score;

        [JsonProperty("finalScore")]
        public FinalScore finalScore;

        [JsonProperty("endedAt")]
        public DateTime endedAt;

        [JsonProperty("address")]
        public string address;

        [JsonProperty("totalTime")]
        public int totalTime;
    }

    public class FinalScore
    {
        [JsonProperty("outcome")]
        public string outcome;

        [JsonProperty("expeditionType")]
        public string expeditionType;

        [JsonProperty("totalScore")]
        public int totalScore;

        [JsonProperty("achievements")]
        public List<Achievement> achievements;

        [JsonProperty("notifyNoLoot")]
        public bool notifyNoLoot;

        [JsonProperty("lootbox")]
        public List<object> lootbox;
    }

    public class Pagination
    {
        [JsonProperty("currentPage")]
        public int currentPage;

        [JsonProperty("pageSize")]
        public int pageSize;

        [JsonProperty("totalItems")]
        public int totalItems;

        [JsonProperty("totalPages")]
        public int totalPages;
    }

    public class Root
    {
        [JsonProperty("data")]
        public DataItem[] data;

        [JsonProperty("pagination")]
        public Pagination pagination;
    }
}