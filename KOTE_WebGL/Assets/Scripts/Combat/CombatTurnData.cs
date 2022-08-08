using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CombatTurnData
{
    public string originType = string.Empty;
    public string originId = string.Empty;
    public List<Target> targets;
    public float delay;
    public Guid attackId;

    public CombatTurnData()
    {
        attackId = new Guid();
        delay = GameSettings.COMBAT_ANIMATION_DELAY;
    }
    public CombatTurnData(string origin, List<Target> targets, float delay = GameSettings.COMBAT_ANIMATION_DELAY)
    {
        this.originId = origin;
        this.targets = targets;
        this.delay = delay;
        attackId = new Guid();
    }

    public bool ContainsTarget(string targetID)
    {
        foreach (Target target in targets)
        {
            if (target.targetID == targetID)
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
            if (target.targetID == targetID || targetID == target.targetType)
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
        public Target(string target, int healthDelta, int finalHealth, int defenseDelta, int finalDefense)
        {
            this.targetID = target;
            this.healthDelta = healthDelta;
            this.finalHealth = finalHealth;
            this.defenseDelta = defenseDelta;
            this.finalDefense = finalDefense;
        }

        public string targetType = string.Empty;
        public string targetID = string.Empty;

        public int healthDelta;
        public int finalHealth;
        public int defenseDelta;
        public int finalDefense;
        public List<StatusData.Status> statuses;

        public override string ToString() 
        {
            return $"[{targetType} | {targetID}]";
        }
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