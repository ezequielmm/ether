using System;
using System.Collections;
using UnityEngine;

namespace Combat.VFX
{
    [CreateAssetMenu(fileName = "VFX", menuName = "VFX/VFX")]
    public class VisualEffect : ScriptableObject
    {
        public string animationName;
        public Material material;
        public VFXParticle particlesPrefab;
        private VFXParticle particlesInstance;
        public bool stopWhenAnimationEnds = true;

        public VFXLogic logic;
        
        public void Play(MonoBehaviour monoBehaviour, Animator anim, MeshRenderer meshRenderer, Action callbackAtEnd = null)
        {
            if (logic && !logic.RunLogic(monoBehaviour))
                return;
            
            var previousMat = meshRenderer.material;
            
            /*
            if (material)
            {
                meshRenderer.material = new Material(material);
                meshRenderer.material.mainTexture = previousMat.mainTexture;
            }
            */
            
            if (particlesPrefab)
            {
                if (!particlesInstance) particlesInstance = Instantiate(particlesPrefab, meshRenderer.transform);
                particlesInstance.transform.localPosition = Vector3.zero;
                particlesInstance.Play();
            }

            if (!string.IsNullOrEmpty(animationName))
            {
                anim.Play(animationName);
                monoBehaviour.StartCoroutine(EffectFinish(anim, () =>
                {
                    meshRenderer.material = previousMat;
                    callbackAtEnd?.Invoke();
                }));
            }
        }

        private IEnumerator EffectFinish(Animator anim, Action afterAnimation)
        {
            var t = 0f;
            var duration = anim.GetCurrentAnimatorStateInfo(0).length;
            
            while (t < duration || !stopWhenAnimationEnds)
            {
                if (t < duration)
                    t += Time.deltaTime;
                yield return null;
            }
            
            afterAnimation?.Invoke();
            anim.Play("Default");
        }

        public void StopVFX() => stopWhenAnimationEnds = true;
    }
}
