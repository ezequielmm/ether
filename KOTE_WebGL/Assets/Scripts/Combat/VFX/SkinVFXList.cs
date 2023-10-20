using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Combat.VFX
{
    [CreateAssetMenu(fileName = "SkinVFX", menuName = "VFX/SkinVFX", order = 0)]
    public class SkinVFXList : ScriptableObject
    {
        [System.Serializable]
        public class VFXPair
        {
            public string skinName;
            public VFXData[] vfxData;
        }
        public VFXPair[] vfxPairs;
        private Dictionary<string, VFXPair> vfxDict;
        
        [System.Serializable]
        public struct VFXData
        {
            public string boneName;
            public GameObject vfx;
        }
        
        public VFXData[] GetVFX(string name)
        {
            vfxDict ??= vfxPairs.ToDictionary(k => k.skinName, v => v);
            if (vfxDict.TryGetValue(name, out var vfx))
                return vfx.vfxData;
            
            return null;
        }
    }
}