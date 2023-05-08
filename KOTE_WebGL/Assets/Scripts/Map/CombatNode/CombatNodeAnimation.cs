using UnityEngine;

namespace map.CombatNode
{
    public class CombatNodeAnimation : MonoBehaviour
    {
        // Animation Event
        public void PlayBladeSound1()
        {
            GameManager.Instance.EVENT_PLAY_SFX?.Invoke(SoundTypes.MapElements, "Blade Clash 1");
        }
        
        // Animation Event
        public void PlayBladeSound2()
        {
            GameManager.Instance.EVENT_PLAY_SFX?.Invoke(SoundTypes.MapElements, "Blade Clash 2");
        }
    }
}