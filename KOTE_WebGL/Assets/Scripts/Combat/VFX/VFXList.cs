using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Combat.VFX
{
    [CreateAssetMenu(fileName = "VFXList", menuName = "VFX/VFXList", order = 0)]
    public class VFXList : ScriptableObject
    {
        [System.Serializable]
        public struct VFXPair
        {
            public VFX name;
            public ScriptableObject vfx;
        }
        public VFXPair[] vfxPairs;
        private Dictionary<VFX, ScriptableObject> vfxDict;
        
        public VisualEffect GetVFX(VFX name)
        {
            vfxDict ??= vfxPairs.ToDictionary(k => k.name, v => v.vfx);
            if (vfxDict.TryGetValue(name, out var vfx))
                return vfx as VisualEffect;
            
            return null;
        }
    }
    
    public enum VFX
    {
        Buff,
        Debuff,
        Defend,
        Defend_Shake,
        Counter,
        Self_immolation,
        Infect,
        Breach_attack,
        Breach_hit,
        Grow,
        TransformMossy,
        Mitosis,
        Scorch,
        Ethereal,
        None
    }
}