using System;
using UnityEngine;

namespace Combat.VFX
{
    [CreateAssetMenu(fileName = "GrowVFX", menuName = "VFX/Logic/GrowVFX")]
    public class GrowVFX : VFXLogic
    {
        private Vector2 originalScale;
        
        public override bool RunLogic(MonoBehaviour monoBehaviour)
        {
            if (originalScale == default)
                originalScale = monoBehaviour.transform.localScale;
            if (Math.Abs(originalScale.x - monoBehaviour.transform.localScale.x) < 0.1f)
                monoBehaviour.transform.localScale *= 1.2f;

            return false;
        }
    }
}