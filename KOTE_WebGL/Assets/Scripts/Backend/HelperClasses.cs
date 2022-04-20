using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// These classes are used to deserialize JSON objects coming from/to webrequests or webscokects messages
/// </summary>
/// 
[Serializable]
public class ExpeditionMapData
{
   public NodeDataHelper[] data;
}
[Serializable]
public class NodeDataHelper
{
    public int act;
    public int step;
    public int id;
    public string type;
    public int[] exits;
    public int[] enter;
}

[Serializable]
public class MapStructure
{
    public List<Act> acts = new List<Act>();
}
[Serializable]
public class Act
{
    public List<Step> steps = new List<Step>();
}
[Serializable]
public class Step
{
    public List<NodeDataHelper> nodesData = new List<NodeDataHelper>();
}


[Serializable]
public class ExpeditionStatusData
{
    public Data data;
    public bool GetHasExpedition()
    {
        return this.data.hasExpedition == "true";
    }

    [Serializable]
    public class Data
    {
        public string hasExpedition;
    }
}

public class ExpeditionRequestData
{
    public Data data;
    public bool GetExpeditionStarted()
    {
        return this.data.expeditionCreated == "true";
    }

    [Serializable]
    public class Data
    {
        public string expeditionCreated;
    }
}

[Serializable]
public class RandomNameData
{
    public Data data;

    [Serializable]
    public class Data
    {
        public string username;
    }
}

[Serializable]
public class RegisterData
{
    public Data data;

    [Serializable]
    public class Data
    {
        public string token;
        public string name;
    }
}

[Serializable]
public class LoginData
{
    public Data data;

    [Serializable]
    public class Data
    {
        public string token;
    }
}

[Serializable]
public class ProfileData
{
    public Data data;

    [Serializable]
    public class Data
    {
        public string id;
        public string name;
        public string email;
        public List<string> wallets;
        public int coins;
        public int fief;
        public int experience;
        public int level;
        public int act;
        public ActMap act_map;

        [Serializable]
        public class ActMap
        {
            public string id;
            public string current_node;
        }
    }
}

[Serializable]
public class LogoutData
{
    public Data data;

    [Serializable]
    public class Data
    {
        public string message;
    }
}

[Serializable]
public class PlayerStateData 
{
    public Data data;

    [Serializable]
    public class Data
    {
        public string player_name;
        public string className;
        public int hp_max;
        public int hp_current;
        public int gold;
        public Potions potion;

        [Serializable]
        public class Potions 
        {
            public string index;
            public string potion_name;
        }

        public string[] trinkets;
        public string[] deck;
    }
}