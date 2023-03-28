using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CombatTurnData
{
    public string originType = string.Empty;
    public string originId = string.Empty;
    public List<Target> targets = new();
    public float delay = GameSettings.COMBAT_ANIMATION_DELAY;
    public Guid attackId = Guid.NewGuid();
    public QueueActionData action = new();

    public CombatTurnData() { }

    public CombatTurnData(string origin, List<Target> targets, float delay = GameSettings.COMBAT_ANIMATION_DELAY)
    {
        this.originId = origin;
        this.targets = targets;
        this.delay = delay;
    }

    public bool ContainsTarget(string targetID)
    {
        foreach (Target target in targets)
        {
            if (target.targetId == targetID)
            {
                return true;
            }
        }
        return false;
    }

    public Target GetTarget(string targetID)
    {
        foreach (Target target in targets)
        {
            if (target.targetId == targetID || targetID == target.targetType)
            {
                return target;
            }
        }
        return null;
    }

    [Serializable]
    public class Target
    {
        public Target() { }

        public string targetType = string.Empty;
        public string targetId = string.Empty;

        public int healthDelta;
        public int finalHealth;
        public int defenseDelta;
        public int finalDefense;
        public string effectType;
        public List<StatusData.Status> statuses = new();

        public override string ToString() 
        {
            return $"[{targetType} | {targetId} | {effectType}]";
        }
    }

    [Serializable]
    public class QueueActionData
    {
        public string name = string.Empty;
        public string hint = string.Empty;
    }

    public override string ToString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append($"[{originType} | {originId}] --> ");
        foreach (var target in targets)
        {
            sb.Append(target.ToString() + ", ");
        }
        return sb.ToString().Substring(0, sb.Length - 2);
    }
    
}