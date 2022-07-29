using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CombatTurnData
{
    public string origin;
    public List<Target> targets;
    public float delay;
    public string attackId;

    public CombatTurnData()
    {
        attackId = new Guid().ToString();
    }
    public CombatTurnData(string origin, List<Target> targets, float delay = GameSettings.COMBAT_ANIMATION_DELAY)
    {
        this.origin = origin;
        this.targets = targets;
        this.delay = delay;
        attackId = new Guid().ToString();
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
            if (target.targetID == targetID)
            {
                return target;
            }
        }
        return null;
    }

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

        public string targetID;

        public int healthDelta;
        public int finalHealth;
        public int defenseDelta;
        public int finalDefense;
    }

    public override string ToString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append($"[{origin}] --> [");
        foreach (var target in targets)
        {
            sb.Append(target.ToString() + ", ");
        }
        return sb.ToString().Substring(0, sb.Length - 2) + "]";
    }
}