using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace KOTE.Expedition.Combat.Cards
{
    internal enum CARD_PARTICLE_TYPES
    {
        Move,
        Aura
    }

    public class CardVisualsManager : MonoBehaviour
    {
        public CardManager cardManager;

        public GameObject cardcontent;

        public TextMeshPro cardidTF;
        public TextMeshPro energyTF;
        public TextMeshPro nameTF;
        public TextMeshPro rarityTF;
        public TextMeshPro descriptionTF;
        public SpriteRenderer cardImage;
        public SpriteRenderer gemSprite;
        public SpriteRenderer bannerSprite;
        public SpriteRenderer frameSprite;

        [Header("Outline effects")] public ParticleSystem auraPS;

        public Material greenOutlineMaterial;
        public Material blueOutlineMaterial;

        private Material defaultMaterial;
        private Material outlineMaterial;

        [Header("Colors")] public Color greenColor;
        public Color blueColor;
        public Color redColor;

        [HideInInspector] public List<Tooltip> tooltips;

        [Header("Movement")] public ParticleSystem movePs;

        public Card cardData;

        private int currentPlayerEnergy;


        private void Awake()
        {
            //Screenspace is defined in pixels. The bottom-left of the screen is (0,0); the right-top is (pixelWidth,pixelHeight). The z position is in world units from the camera.
            //Viewport space is normalized and relative to the camera. The bottom-left of the camera is (0,0); the top-right is (1,1). The z position is in world units from the camera.

            tooltips = new List<Tooltip>();
        }


        // Start is called before the first frame update
        void Start()
        {
            GameManager.Instance.EVENT_UPDATE_ENERGY.AddListener(OnUpdateEnergy);
            GameManager.Instance.EVENT_CARD_UPDATE_TEXT.AddListener(UpdateCardText);
        }

        internal void PlayCardParticles(CARD_PARTICLE_TYPES type)
        {
            switch (type)
            {
                case CARD_PARTICLE_TYPES.Move:
                    movePs.Play();
                    break;
                case CARD_PARTICLE_TYPES.Aura:
                    auraPS.Play();
                    break;
            }
        }

        internal void StopCardParticles(CARD_PARTICLE_TYPES type)
        {
            switch (type)
            {
                case CARD_PARTICLE_TYPES.Move:
                    movePs.Stop();
                    break;
                case CARD_PARTICLE_TYPES.Aura:
                    auraPS.Stop();
                    break;
            }
        }

        internal void SetTooltips(Vector3 position, TooltipController.Anchor anchor)
        {
            GameManager.Instance.EVENT_SET_TOOLTIPS.Invoke(tooltips, anchor,
                position, null);
        }

        public void Populate(Card card, int energy)
        {
            cardData = card;
            //Debug.Log(card);

            // we've got to check if the card is upgraded when picking the gem, hence the extra variable
            CardAssetManager cardAssetManager = CardAssetManager.Instance;
            gemSprite.sprite = cardAssetManager.GetGem(card.cardType, card.isUpgraded);
            if (card.cardType == "curse" || card.cardType == "status")
            {
                frameSprite.sprite = cardAssetManager.GetFrame(card.cardType);
            }
            else
            {
                frameSprite.sprite = cardAssetManager.GetFrame(card.pool);
            }

            bannerSprite.sprite = cardAssetManager.GetBanner(card.rarity);
            cardImage.sprite = cardAssetManager.GetCardImage(card.cardId);

            currentPlayerEnergy = energy;
            UpdateCardText(card);
        }

        private void UpdateCardText(Card card)
        {
            if (card.id == cardData.id)
            {
                cardData = card;
                string cardEnergy = Mathf.Max(card.energy, 0).ToString();
                if (card.energy < 0)
                {
                    cardEnergy = "X";
                }

                energyTF.SetText(cardEnergy);
                nameTF.SetText(card.name);
                rarityTF.SetText(card.rarity);
                descriptionTF.SetText(card.description);

                if (card.properties.statuses != null)
                {
                    tooltips.Clear();
                    foreach (var status in card.properties.statuses)
                    {
                        if (!string.IsNullOrEmpty(status.tooltip.title))
                        {
                            tooltips.Add(status.tooltip);
                        }
                        else
                        {
                            var description = status.args.description ?? "TODO // Add Description";
                            tooltips.Add(new Tooltip()
                            {
                                title = Utils.PrettyText(status.name),
                                description = description
                            });
                        }
                    }
                }

                if (card.keywords != null && card.keywords.Contains("unplayable"))
                {
                    cardManager.hasUnplayableKeyword = true;
                }

                UpdateCardBasedOnEnergy();
            }
        }

        private void OnUpdateEnergy(int currentEnergy, int maxEnergy)
        {
            currentPlayerEnergy = currentEnergy;
            // Debug.Log("[CardOnHandManager] OnUpdateEnergy = "+currentEnergy);
            if (cardManager.cardActive)
            {
                UpdateCardBasedOnEnergy();
            }
        }

        internal void UpdateCardEnergyText(int energy)
        {
            string cardEnergy = Mathf.Max(energy, 0).ToString();
            if (energy < 0)
            {
                cardEnergy = "X";
            }

            energyTF.text = cardEnergy;
            cardData.energy = energy;
            UpdateCardBasedOnEnergy();
        }

        internal void UpdateCardBasedOnEnergy()
        {
            if (cardManager.hasUnplayableKeyword)
            {
                energyTF.text = "-";
                outlineMaterial = greenOutlineMaterial;
                cardManager.card_can_be_played = false;
            }
            else if (cardData.energy <= currentPlayerEnergy)
            {
                var main = auraPS.main;
                main.startColor = greenColor;
                outlineMaterial = greenOutlineMaterial; //TODO:apply blue if card has a special condition
                energyTF.color = Color.black;
                cardManager.card_can_be_played = true;
                //Debug.Log($"[CardOnHandManager] [{thisCardValues.name}] Card is now playable {energy}/{thisCardValues.energy}");
            }
            else
            {
                energyTF.color = redColor;
                outlineMaterial = greenOutlineMaterial;
                cardManager.card_can_be_played = false;
                //Debug.Log($"[CardOnHandManager] [{thisCardValues.name}] Card is no longer playable {energy}/{thisCardValues.energy}");
            }
        }

        internal void DisableCardContent(bool notify = false)
        {
            // DOTween.Kill(this.transform);
            this.cardcontent.SetActive(false);
            if (notify) GameManager.Instance.EVENT_CARD_DISABLED.Invoke(cardData.id);
        }

        internal void EnableCardContent()
        {
            // DOTween.Kill(this.transform);
            this.cardcontent.SetActive(true);
            ActivateCard();
            cardManager.cardActive = true;
        }

        public void ActivateCard()
        {
            // Debug.Log("Activating card");
            cardManager.cardActive = true;
        }
    }
}