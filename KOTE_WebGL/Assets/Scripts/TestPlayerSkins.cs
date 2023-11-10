using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class TestPlayerSkins : MonoBehaviour
    {
        public string helmet;
        public string breastplate;
        public string cloak;
        public string weapon;
        public string shield;
        public string boots;
        public string gauntlets;
        public string pauldrons;
        public string vambrace;
        public string legguard;
        public string padding;

        [Space]
        [SerializeField] private GameObject playerSpriteManager;
        [SerializeField] private SpineAnimationsManagement playerAnimations;
        
        private void Start()
        {
            ClientEnvironmentManager.Instance.StartEnvironmentManger();
            TrySkin();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                TrySkin();
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                PlayAnimations();
            }
        }

        [ContextMenu("TrySkin")]
        public void TrySkin()
        {
            var nftData = new Nft()
            {
                CanPlay = true,
                Contract = NftContract.Knights,
            };

            nftData.trait = new Dictionary<Trait, string>();
            if (!string.IsNullOrEmpty(helmet))
                nftData.Traits.Add(Trait.Helmet, helmet);
            if (!string.IsNullOrEmpty(breastplate))
                nftData.Traits.Add(Trait.Breastplate, breastplate);
            if (!string.IsNullOrEmpty(cloak))
                nftData.Traits.Add(Trait.Cloak, cloak);
            if (!string.IsNullOrEmpty(weapon))
                nftData.Traits.Add(Trait.Weapon, weapon);
            if (!string.IsNullOrEmpty(shield))
                nftData.Traits.Add(Trait.Shield, shield);
            if (!string.IsNullOrEmpty(boots))
                nftData.Traits.Add(Trait.Boots, boots);
            if (!string.IsNullOrEmpty(gauntlets))
                nftData.Traits.Add(Trait.Gauntlet, gauntlets);
            if (!string.IsNullOrEmpty(pauldrons))
                nftData.Traits.Add(Trait.Pauldrons, pauldrons);
            if (!string.IsNullOrEmpty(vambrace))
                nftData.Traits.Add(Trait.Vambrace, vambrace);
            if (!string.IsNullOrEmpty(legguard))
                nftData.Traits.Add(Trait.Legguard, legguard);
            if (!string.IsNullOrEmpty(padding))
                nftData.Traits.Add(Trait.Padding, padding);
            
            FindObjectOfType<PlayerSpriteManager>().DestroyInstance();
            Instantiate(playerSpriteManager);
            FindObjectOfType<PlayerSpriteManager>().BuildPlayer(nftData);
        }

        public void PlayAnimations()
        {
            var animationsList = new[]
            {
                "attack",
                "attack",
                "cast",
                "injured_idle",
                "death"
            };

            foreach (var anim in animationsList)
            {
                playerAnimations.PlayAnimationSequence(anim);
            }
        
        }
    }
}