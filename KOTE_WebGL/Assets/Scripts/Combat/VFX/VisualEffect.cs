using System.Collections;
using System.Linq;
using UnityEngine;

namespace Combat.VFX
{
    [CreateAssetMenu(fileName = "VFX", menuName = "VFX/VFX")]
    public class VisualEffect : ScriptableObject
    {
        public string animationName;
        public Material material;
        public ParticleSystem particlesPrefab;
        private ParticleSystem particlesInstance;
        public bool stopWhenAnimationEnds = true;

        public void Play(MonoBehaviour monoBehaviour, Animator anim, MeshRenderer meshRenderer)
        {
            var previousMat = meshRenderer.material;
            meshRenderer.material = new Material(material);
            meshRenderer.material.mainTexture = previousMat.mainTexture;
            
            anim.Play(animationName);
            
            if (particlesPrefab)
            {
                if (!particlesInstance) particlesInstance = Instantiate(particlesPrefab, meshRenderer.transform);
                particlesInstance.transform.localPosition = Vector3.zero;
                particlesInstance.Play();
            }
            
            monoBehaviour.StartCoroutine(EffectFinish(anim, meshRenderer, previousMat));
        }
        
        public void Play(MonoBehaviour monoBehaviour, Animator anim, Renderer meshRenderer, Material targetMaterial)
        {
            var previousMat = meshRenderer.materials.First(e => e == targetMaterial);
            meshRenderer.material = new Material(material);
            meshRenderer.material.mainTexture = previousMat.mainTexture;
            
            anim.Play(animationName);
            
            if (particlesPrefab)
            {
                if (!particlesInstance) particlesInstance = Instantiate(particlesPrefab, meshRenderer.transform);
                particlesInstance.transform.localPosition = Vector3.zero;
                particlesInstance.Play();
            }
            
            monoBehaviour.StartCoroutine(EffectFinish(anim, meshRenderer, previousMat));
        }

        private IEnumerator EffectFinish(Animator anim, Renderer meshRenderer, Material previousMat)
        {
            var t = 0f;
            var duration = anim.GetCurrentAnimatorStateInfo(0).length;
            
            while (t < duration)
            {
                t += Time.deltaTime;
                yield return null;
            }
            if (!stopWhenAnimationEnds) yield return null;
            
            meshRenderer.material = previousMat;
            anim.Play("Default");
        }

        public void StopVFX() => stopWhenAnimationEnds = true;
    }
}
