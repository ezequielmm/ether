using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class MemoryCleaner : MonoBehaviour
    {
        private void Start()
        {
            StartCoroutine(LoadScene());
        }
        
        IEnumerator LoadScene()
        {
            CleanMemory();
            yield return null;
            yield return new WaitForSeconds(1f);
            GameManager.Instance.LoadScene(inGameScenes.Expedition, true);
        }
        
        private void CleanMemory()
        {
            var nftTextures = PlayerSpriteManager.Instance.GetAllTraitSprites();
            List<Texture2D> textures = new List<Texture2D>();
            foreach (var traitSprite in nftTextures) {
                textures.Add(traitSprite.Sprite.texture);
            }
            
            FetchData.Instance.CleanMemory(textures.ToArray());
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

    }
}