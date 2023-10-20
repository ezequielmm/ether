using UnityEngine;

namespace Combat.VFX
{
    public abstract class VFXLogic : ScriptableObject
    {
        public abstract bool RunLogic(MonoBehaviour monoBehaviour);
    }
}