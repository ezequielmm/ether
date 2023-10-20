using System.Collections.Generic;
using UnityEngine;

namespace Combat.VFX
{
    public class VFXParticle : MonoBehaviour
    {
        [SerializeField] private List<ParticleSystem> particles;
        
        public void Play()
        {
            particles.ForEach(p => p.Play());
        }
    }
}