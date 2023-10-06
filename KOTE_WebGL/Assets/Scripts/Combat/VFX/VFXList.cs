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
            public string name;
            public ScriptableObject vfx;
        }
        public VFXPair[] vfxPairs;
        private Dictionary<string, ScriptableObject> vfxDict;
        
        public VisualEffect GetVFX(string name)
        {
            vfxDict ??= vfxPairs.ToDictionary(k => k.name, v => v.vfx);
            if (vfxDict.TryGetValue(name, out var vfx))
                return vfx as VisualEffect;
            
            return null;
        }
    }
}