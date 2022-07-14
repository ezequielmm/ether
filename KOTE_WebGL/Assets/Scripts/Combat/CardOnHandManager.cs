using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class CardOnHandManager : MonoBehaviour
{
    [Serializable]
    public struct CardImage
    {
        public String cardName;
        public Sprite Image;
    }

    [Serializable]
    public struct Gem
    {
        public string type;
        public Sprite gem;
    }

    [Serializable]
    public struct Banner
    {
        public string rarity;
        public Sprite banner;
    }

    [Serializable]
    public struct Frame
    {
        public string pool;
        public Sprite frame;
    }

    public TextMeshPro energyTF;
    public TextMeshPro nameTF;
    public TextMeshPro rarityTF;
    public TextMeshPro descriptionTF;
    public SpriteRenderer cardImage;
    public SpriteRenderer gemSprite;
    public SpriteRenderer bannerSprite;
    public SpriteRenderer frameSprite;

    // public string id;
    // public string type;
    //  public int card_energy_cost;
    public bool card_can_be_played = true;

    public Vector3 targetPosition;
    public Vector3 targetRotation;
    public bool cardActive = false;

    [Header("Card Variation Sprites")]
    public List<CardImage> cardImages;
    public List<Gem> Gems;
    public List<Banner> banners;
    public List<Frame> frames;

    [Header("Outline effects")] 
    public ParticleSystem auraPS;
    public GameObject cardBgGO;
    public Material greenOutlineMaterial;
    public Material blueOutlineMaterial;

    private Material defaultMaterial;
    private Material outlineMaterial;

    [Header("Colors")]
    public Color greenColor;
    public Color blueColor;
    public Color redColor;

    [HideInInspector] 
    public Sequence mySequence;

    private Vector3 drawPileOrthoPosition;
    private Vector3 discardPileOrthoPosition;
    private Vector3 exhaustPileOrthoPosition;

    [Header("Movement")] 
    public ParticleSystem movePs;

    private Card thisCardValues;
    private bool activateCardAfterMove;
    private bool cardIsShowingUp;
    private bool pointerIsActive;

    private void Awake()
    {
        //Screenspace is defined in pixels. The bottom-left of the screen is (0,0); the right-top is (pixelWidth,pixelHeight). The z position is in world units from the camera.
        //Viewport space is normalized and relative to the camera. The bottom-left of the camera is (0,0); the top-right is (1,1). The z position is in world units from the camera.

        drawPileOrthoPosition = TransformUIToOrtho("DrawCardPile");
        discardPileOrthoPosition = TransformUIToOrtho("DiscardCardPile");
        exhaustPileOrthoPosition = TransformUIToOrtho("ExhaustedPilePrefab");
    }


    // Start is called before the first frame update
    void Start()
    {
        mySequence = DOTween.Sequence();

        defaultMaterial = cardBgGO.GetComponent<Renderer>().sharedMaterial;
        GameManager.Instance.EVENT_UPDATE_ENERGY.AddListener(OnUpdateEnergy);
        GameManager.Instance.EVENT_MOVE_CARD.AddListener(OnCardToMove);
        GameManager.Instance.EVENT_CARD_SHOWING_UP.AddListener(OnCardMouseShowingUp);
        GameManager.Instance.EVENT_CARD_MOUSE_EXIT.AddListener(OnCardMouseExit);
    }

    private void OnCardMouseExit(string cardId)
    {
        if (cardId != thisCardValues.id && !cardIsShowingUp)
        {
            // Debug.Log("[OnCardMouseExit]");
            ResetCardPosition();
        }
    }

    private void OnCardMouseShowingUp(string cardId, Vector3 cardPos)
    {
        if (cardId != thisCardValues.id)
        {
            // Debug.Log("Check mouse is left or right "+ TransformMouseToOrtho().x+"/"+this.transform.position.x);            

            float direction = cardPos.x > this.transform.position.x ? -0.5f : 0.5f;

            this.transform.DOMoveX(targetPosition.x + direction, 0.5f);
        }
    }

    private void OnCardToMove(CardToMoveData data)
    {
        Debug.Log("[OnCardToMove]thisCardValues.id =" + thisCardValues.id + " || data.id=" + data.id);
        if (thisCardValues.id == data.id)
        {
            System.Enum.TryParse(data.source, out CARDS_POSITIONS_TYPES origin);
            System.Enum.TryParse(data.destination, out CARDS_POSITIONS_TYPES destination);

            MoveCard(origin, destination);
        }
    }

    private void OnUpdateEnergy(int currentEnergy, int maxEnergy)
    {
        Debug.Log("[CardOnHandManager] OnUpdateEnergy = " + currentEnergy);
        if (cardActive)
        {
            UpdateCardBasedOnEnergy(currentEnergy);
        }
    }

    internal void Populate(Card card, int energy)
    {
        energyTF.SetText(card.energy.ToString());
        nameTF.SetText(card.name);
        rarityTF.SetText(card.rarity);
        descriptionTF.SetText(card.description);
        // we've got to check if the card is upgraded when picking the gem, hence the extra variable
        string cardType = card.cardType;
        if (card.isUpgraded) cardType += "+";
        gemSprite.sprite = Gems.Find(gem => gem.type == cardType).gem;
        frameSprite.sprite = frames.Find(frame => frame.pool == card.pool).frame;
        bannerSprite.sprite = banners.Find(banner => banner.rarity == card.rarity).banner;
        if (cardImages.Exists(image => image.cardName == card.name))
        {
            cardImage.sprite = cardImages.Find(image => image.cardName == card.name).Image;
        }
        /* this.id = card.id;
          card_energy_cost = card.energy;*/

        thisCardValues = card;

        UpdateCardBasedOnEnergy(energy);
    }

    public void MoveCard(CARDS_POSITIONS_TYPES originType, CARDS_POSITIONS_TYPES destinationType,
        bool activateCard = false, Vector3 pos = default(Vector3), float delay = 0)
    {
        Debug.Log("[CardOnHandManager] MoveCard = " + originType + " to " + destinationType);
        movePs.Play();

        Vector3 origin = new Vector3();
        Vector3 destination = new Vector3();

        switch (originType)
        {
            case CARDS_POSITIONS_TYPES.draw:
                origin = drawPileOrthoPosition;
                break;
            case CARDS_POSITIONS_TYPES.discard:
                origin = discardPileOrthoPosition;
                break;
            case CARDS_POSITIONS_TYPES.hand:
                origin = this.transform.position;
                break;
            case CARDS_POSITIONS_TYPES.exhaust:
                origin = exhaustPileOrthoPosition;
                break;
        }

        if (pos.magnitude > 0)
        {
            destination = pos;
        }
        else
        {
            switch (destinationType)
            {
                case CARDS_POSITIONS_TYPES.draw:
                    destination = drawPileOrthoPosition;
                    break;
                case CARDS_POSITIONS_TYPES.discard:
                    destination = discardPileOrthoPosition;
                    break;
                case CARDS_POSITIONS_TYPES.hand:
                    destination = pos;
                    break;
                case CARDS_POSITIONS_TYPES.exhaust:
                    destination = exhaustPileOrthoPosition;
                    break;
            }
        }

        cardActive = (originType == CARDS_POSITIONS_TYPES.draw && destinationType == CARDS_POSITIONS_TYPES.hand);
        //cardActive = activateCardAfterMove;

        if (delay > 0)
        {
            transform.DOMove(destination, 1f).SetDelay(delay, true).SetEase(Ease.InCirc).OnComplete(OnMoveCompleted)
                .From(origin);
            ;
            // transform.DOMoveX(destination.x, .5f).SetEase(Ease.Linear);
            // transform.DOMoveY(destination.y, .5f).SetEase(Ease.InCirc);
        }
        else
        {
            transform.DOMove(destination, 1f).From(origin).SetEase(Ease.OutCirc);
            transform.DOScale(Vector3.zero, 1f).SetEase(Ease.InElastic).OnComplete(HideAndDeactivateCard);
        }

        //transform.DOPlay();
    }

    private void OnMoveCompleted()
    {
        movePs.Stop();
    }

    private void HideAndDeactivateCard()
    {
        //Debug.Log("HideAndDeactivateCard, activateCardAfterMove="+ activateCardAfterMove);
        //cardActive = false;
        //this.gameObject.SetActive(false);

        Destroy(this.gameObject);
    }

    private void UpdateCardBasedOnEnergy(int energy)
    {
        if (thisCardValues.energy <= energy)
        {
            var main = auraPS.main;
            main.startColor = greenColor;
            outlineMaterial = greenOutlineMaterial; //TODO:apply blue if card has a special condition
            // auraPS.Play();
            card_can_be_played = true;
        }
        else
        {
            //  auraPS.Stop();
            energyTF.color = redColor;
            outlineMaterial = greenOutlineMaterial;
            card_can_be_played = false;
        }
    }

    private void OnDestroy()
    {
        // DOTween.Kill(this.transform);
        GameManager.Instance.EVENT_CARD_DESTROYED.Invoke(thisCardValues.id);
    }


    private void OnMouseEnter()
    {
        if (cardActive)
        {
            // DOTween.PlayForward(this.gameObject);
            // GameManager.Instance.EVENT_CARD_MOUSE_ENTER.Invoke(thisCardValues.cardId);


            cardBgGO.GetComponent<Renderer>().material = outlineMaterial;

            ShowUpCard();
        }
        /*else
        {
            Debug.Log("[Mouse evnter but ccard is not active]");
        }*/
    }

    private void ShowUpCard()
    {
        if (cardIsShowingUp) return;

        if (cardActive)
        {
            // mySequence.Kill();
            DOTween.Kill(this.transform);
            ResetCardPosition();

            auraPS.Play();

            cardIsShowingUp = true;

            //  Debug.Log("ShowUp");
            transform.DOScale(Vector3.one * GameSettings.HAND_CARD_SHOW_UP_Y, GameSettings.HAND_CARD_SHOW_UP_TIME);

            //pos.z = maxDepth;
            transform.DOMoveY(1.5f, 0.2f).SetRelative(true);

            transform.DORotate(Vector3.zero, GameSettings.HAND_CARD_SHOW_UP_TIME);

            GameManager.Instance.EVENT_CARD_SHOWING_UP.Invoke(thisCardValues.id, this.targetPosition);
        }
    }

    private void ResetCardPosition()
    {
        if (cardActive)
        {
            // Debug.Log("[ResetCardPosition]");
            if (auraPS.gameObject.activeSelf) auraPS.Stop();
            cardBgGO.GetComponent<Renderer>().material = defaultMaterial;
            cardIsShowingUp = false;
            transform.DOMove(targetPosition, GameSettings.HAND_CARD_RESET_POSITION_TIME);
            transform.DOScale(Vector3.one, GameSettings.HAND_CARD_RESET_POSITION_TIME);
            transform.DORotate(targetRotation, GameSettings.HAND_CARD_RESET_POSITION_TIME);
        }
    }

    private void OnMouseExit()
    {
        if (pointerIsActive) return;

        if (cardActive && cardIsShowingUp)
        {
            // Debug.Log("[OnMouseExit]");
            GameManager.Instance.EVENT_CARD_MOUSE_EXIT.Invoke(thisCardValues.id);

            ResetCardPosition();
        }
    }

    private void OnMouseDrag()
    {
        if (!card_can_be_played)
        {
            //TODO: show no energy message
            return;
        }

        float xxDelta = Mathf.Abs(this.transform.position.x - targetPosition.x);


        // Debug.Log("Distance y is " + xxDelta);
        //if (type == "attack")
        if (thisCardValues != null && thisCardValues.showPointer)
        {
            //show the pointer instead of following the mouse
            GameManager.Instance.EVENT_CARD_ACTIVATE_POINTER.Invoke(transform.position);
            pointerIsActive = true;
            return;
        }

        float zz = this.transform.position.z;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = zz;
        this.transform.position = mousePos;
    }

    private void OnMouseUp()
    {
        if (pointerIsActive)
        {
            ResetCardPosition();
            pointerIsActive = false;
        }


        if (thisCardValues.showPointer)
        {
            GameManager.Instance.EVENT_CARD_DEACTIVATE_POINTER.Invoke(thisCardValues.id);
        }
    }

    private void OnMouseUpAsButton()
    {
        if (pointerIsActive)
        {
            ResetCardPosition();
            pointerIsActive = false;
        }

        if (cardActive)
        {
            if (Vector2.Distance(this.transform.position, Vector2.zero) < 1.5f)
            {
                Debug.Log("card is on center");
                GameManager.Instance.EVENT_CARD_PLAYED.Invoke(thisCardValues.id, -1);
                cardActive = false;
            }
            else
            {
                Debug.Log("card is far from center");
                //MoveCardBackToOriginalHandPosition();
            }
        }
    }

    private void OnMouseDown()
    {
    }

    private void MoveCardBackToOriginalHandPosition()
    {
        //this.transform.DOMove(originalPosition, 0.5f);
    }

    public void ActivateCard()
    {
        // Debug.Log("Activating card");
        cardActive = true;
    }

    private Vector3 TransformUIToOrtho(string uiName)
    {
        Vector3 pos = GameObject.Find(uiName).transform.position; //(1.1, 104.5, 0.0)

        float height = 2 * Camera.main.orthographicSize; //10
        float width = height * Camera.main.aspect; //21.42

        //transform UI coordinates to orthorgraphic coordinates
        float xx = pos.x * width / Screen.width;
        xx -= width / 2; //ortho counts from the center 
        float yy = pos.y * height / Screen.height;
        yy -= height / 2;

        return new Vector3(xx, yy, this.transform.position.z);
    }

    private Vector3 TransformMouseToOrtho()
    {
        Vector3 pos = Input.mousePosition;

        float height = 2 * Camera.main.orthographicSize; //10
        float width = height * Camera.main.aspect; //21.42

        //transform UI coordinates to orthorgraphic coordinates
        float xx = pos.x * width / Screen.width;
        xx -= width / 2; //ortho counts from the center 
        float yy = pos.y * height / Screen.height;
        yy -= height / 2;

        return new Vector3(xx, yy, this.transform.position.z);
    }
}