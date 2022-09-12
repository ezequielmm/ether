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

    // public string id;
    // public string type;
    //  public int card_energy_cost;
    public bool card_can_be_played = true;

    public Vector3 targetPosition;
    public Vector3 targetRotation;
    [SerializeField]
    private bool _cardActive = false;
    public bool cardActive { get => _cardActive;
        set 
        {
            _cardActive = value;
            UpdateCardBasedOnEnergy(currentPlayerEnergy);
        } 
    }

   /* [Header("Card Variation Sprites")]
    public List<Gem> Gems;
    public List<Banner> banners;
    public List<Frame> frames;
    [FormerlySerializedAs("images")] public List<Sprite> cardImages;*/
    
    [Header("Outline effects")] public ParticleSystem auraPS;

    public Material greenOutlineMaterial;
    public Material blueOutlineMaterial;

    private Material defaultMaterial;
    private Material outlineMaterial;

    [Header("Colors")] public Color greenColor;
    public Color blueColor;
    public Color redColor;

    [HideInInspector]
    public List<Tooltip> tooltips;
    [HideInInspector] public Sequence mySequence;

    private Vector3 drawPileOrthoPosition;
    private Vector3 discardPileOrthoPosition;
    private Vector3 exhaustPileOrthoPosition;

    [Header("Movement")] public ParticleSystem movePs;

    public Card thisCardValues;
    private bool activateCardAfterMove;
    private bool cardIsShowingUp;
    private bool pointerIsActive;
    private bool cardIsDisplaced;
    private bool discardAfterMove;

    private int currentPlayerEnergy;

    private bool awaitMouseUp;

    private bool inTransit;
    new Collider2D collider;


    private bool overPlayer = false;
    private PlayerData playerData = null;
    private GameObject lastOver;

    private void Awake()
    {
        //Screenspace is defined in pixels. The bottom-left of the screen is (0,0); the right-top is (pixelWidth,pixelHeight). The z position is in world units from the camera.
        //Viewport space is normalized and relative to the camera. The bottom-left of the camera is (0,0); the top-right is (1,1). The z position is in world units from the camera.

        tooltips = new List<Tooltip>();

        drawPileOrthoPosition = TransformUIToOrtho("DrawCardPile");
        discardPileOrthoPosition = TransformUIToOrtho("DiscardCardPile");
        exhaustPileOrthoPosition = TransformUIToOrtho("ExhaustedPilePrefab");
        inTransit = false;
    }


    // Start is called before the first frame update
    void Start()
    {
        collider = GetComponent<Collider2D>();
        mySequence = DOTween.Sequence();
        GameManager.Instance.EVENT_UPDATE_ENERGY.AddListener(OnUpdateEnergy);
        GameManager.Instance.EVENT_MOVE_CARD.AddListener(OnCardToMove);
        GameManager.Instance.EVENT_CARD_SHOWING_UP.AddListener(OnCardMouseShowingUp);
        GameManager.Instance.EVENT_CARD_MOUSE_EXIT.AddListener(OnCardMouseExit);
        GameManager.Instance.EVENT_CARD_CREATE.AddListener(OnCreateCard);
        var death = gameObject.AddComponent<DisableOnDeath>();
        death.UnParent = true;
    }

    private void OnCreateCard(string cardID)
    {
        if (cardID == thisCardValues.id)
        {
            /*cardcontent.SetActive(true);
            cardActive = true;
            card_can_be_played = true;*/

            MoveCard(CARDS_POSITIONS_TYPES.draw, CARDS_POSITIONS_TYPES.hand, true);
        }
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

    static float delay;

    private void OnCardToMove(CardToMoveData data, float delayIndex)
    {
        if (thisCardValues.id == data.id)
        {
            System.Enum.TryParse(data.source, out CARDS_POSITIONS_TYPES origin);
            System.Enum.TryParse(data.destination, out CARDS_POSITIONS_TYPES destination);
            if (origin == CARDS_POSITIONS_TYPES.discard)
            {
                delay += 0.1f;
            }

            if (!(origin == CARDS_POSITIONS_TYPES.draw && destination == CARDS_POSITIONS_TYPES.hand))
            {
                delayIndex = 0;
            }
            float internalDelay = 0;
            if (delayIndex > 0) 
            {
                internalDelay += 1;
            }

            if (inTransit)
            {
                delay += 1.1f;
            }

            if (delay > 0 || delayIndex > 0)
            {
                StartCoroutine(MoveAfterTime(delay + (delayIndex * GameSettings.CARD_DRAW_SHOW_TIME) + internalDelay, origin, destination));
            }
            else
            {
                MoveCard(origin, destination);
            }
        }
    }

    private IEnumerator MoveAfterTime(float delay, CARDS_POSITIONS_TYPES origin, CARDS_POSITIONS_TYPES destination)
    {
        yield return new WaitForSeconds(delay);
        MoveCard(origin, destination);
    }

    private void OnUpdateEnergy(int currentEnergy, int maxEnergy)
    {
        currentPlayerEnergy = currentEnergy;
        // Debug.Log("[CardOnHandManager] OnUpdateEnergy = "+currentEnergy);
        if (cardActive)
        {
            UpdateCardBasedOnEnergy(currentEnergy);
        }
    }

    internal void Populate(Card card, int energy)
    {
        //Debug.Log(card);
        //cardidTF.SetText(card.id);
        energyTF.SetText(card.energy.ToString());
        nameTF.SetText(card.name);
        rarityTF.SetText(card.rarity);
        descriptionTF.SetText(card.description);
        // we've got to check if the card is upgraded when picking the gem, hence the extra variable
        string cardType = card.cardType;
        // if (card.isUpgraded) cardType += "+";
        CardAssetManager cardAssetManager = CardAssetManager.Instance;
        gemSprite.sprite = cardAssetManager.GetGem(card.cardType);
        frameSprite.sprite = cardAssetManager.GetFrame(card.pool);
        bannerSprite.sprite = cardAssetManager.GetBanner(card.rarity);
        cardImage.sprite = cardAssetManager.GetCardImage(card.cardId);
        /* this.id = card.id;
          card_energy_cost = card.energy;*/
        thisCardValues = card;

        currentPlayerEnergy = energy;

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


        UpdateCardBasedOnEnergy(energy);
    }

    public bool MoveCardIfClose(CARDS_POSITIONS_TYPES originType, CARDS_POSITIONS_TYPES destinationType)
    {
        Vector3 origin = new Vector3();
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

        if (Mathf.Abs((this.transform.position - origin).magnitude) <= 0.1f)
        {
            MoveCard(originType, destinationType);
            return true;
        }
        else
        {
           // Debug.Log($"[CardOnHandManager] Card {thisCardValues.id} is not from {originType} and will not be moved to {destinationType}.");
            return false;
        }
    }

    public void MoveCard(CARDS_POSITIONS_TYPES originType, CARDS_POSITIONS_TYPES destinationType,
        bool activateCard = false, Vector3 pos = default(Vector3), float delay = 0)
    {
      //  Debug.Log("[CardOnHandManager] MoveCard = " + originType + " to " + destinationType + "........card id: " +                  thisCardValues.id);
        movePs.Play();

        Debug.Log("[CardOnHandManager] Card Is Now Moving");

        Vector3 origin = new Vector3();
        Vector3 destination = new Vector3();

        switch (originType)
        {
            case CARDS_POSITIONS_TYPES.draw:
                origin = drawPileOrthoPosition;
                transform.localScale = Vector3.zero;
                break;
            case CARDS_POSITIONS_TYPES.discard:
                origin = discardPileOrthoPosition;
                transform.localScale = Vector3.one * 0.2f;
                break;
            case CARDS_POSITIONS_TYPES.hand:
                origin = this.transform.position;
                break;
            case CARDS_POSITIONS_TYPES.exhaust:
                origin = exhaustPileOrthoPosition;
                transform.localScale = Vector3.one * 0.2f;
                break;
        }

        activateCardAfterMove = false;

        if (pos.magnitude > 0)
        {
            destination = pos;
            activateCardAfterMove = true;
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
                    if (originType == CARDS_POSITIONS_TYPES.hand)
                    {
                        discardAfterMove = true;
                    }

                    break;
                case CARDS_POSITIONS_TYPES.hand:
                    destination = pos;
                    destination.z = GameSettings.HAND_CARD_SHOW_UP_Z;
                    activateCardAfterMove = true;
                    break;
                case CARDS_POSITIONS_TYPES.exhaust:
                    destination = exhaustPileOrthoPosition;
                    break;
            }
        }

        if (originType != destinationType)
        {
            switch (destinationType)
            {
                case CARDS_POSITIONS_TYPES.draw:
                    GameManager.Instance.EVENT_CARD_SHUFFLE.Invoke();
                    break;
                case CARDS_POSITIONS_TYPES.discard:
                    GameManager.Instance.EVENT_CARD_DISCARD.Invoke();
                    break;
                case CARDS_POSITIONS_TYPES.hand:
                    GameManager.Instance.EVENT_CARD_DRAW.Invoke();
                    break;
                case CARDS_POSITIONS_TYPES.exhaust:
                    GameManager.Instance.EVENT_CARD_EXHAUST.Invoke();
                    break;
            }
        }

        //cardActive = (originType == CARDS_POSITIONS_TYPES.draw && destinationType == CARDS_POSITIONS_TYPES.hand);
        //cardActive = activateCardAfterMove;
        cardActive = false;
        cardcontent.SetActive(true);

        if (delay > 0)
        {
            inTransit = true;
            transform.DOMove(destination, 1f).SetDelay(delay, true).SetEase(Ease.InCirc).From(origin);
            if (originType == CARDS_POSITIONS_TYPES.draw && destinationType == CARDS_POSITIONS_TYPES.hand)
            {
                transform.localScale = Vector3.zero;
                transform.DOScale(Vector3.one, 1f).SetDelay(delay, true).SetEase(Ease.OutElastic)
                    .OnComplete(OnMoveCompleted);
            }
            else
            {
                transform.DOScale(Vector3.zero, 1f).SetDelay(delay, true).SetEase(Ease.InElastic)
                    .OnComplete(HideAndDeactivateCard);
            }
        }
        else
        {
            inTransit = true;
            transform.DOMove(destination, 1f).From(origin).SetEase(Ease.OutCirc);
            if (originType == CARDS_POSITIONS_TYPES.draw && destinationType == CARDS_POSITIONS_TYPES.hand)
            {
                Debug.Log("[CardOnHandManager] Draw new card to hand.");
                transform.localScale = Vector3.zero;
                transform.DORotate(Vector3.zero, 1f);
                transform.DOScale(Vector3.one * 1.5f, 1f).SetEase(Ease.OutElastic).OnComplete(OnMoveCompleted);
            }
            else
            {
                transform.DOScale(Vector3.zero, 1f).SetEase(Ease.InElastic).OnComplete(HideAndDeactivateCard);
            }
        }
    }

    IEnumerator ResetAfterTime(float seconds) 
    {
        yield return new WaitForSeconds(seconds);
        inTransit = false;
        cardActive = true;
        ResetCardPosition();
    }

    public void TryResetPosition() 
    {
        if (inTransit)
        {
            StartCoroutine(TryResetAfterTime(0.25f));
        }
        else 
        {
            ResetCardPosition();
        }
    }

    IEnumerator TryResetAfterTime(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        TryResetPosition();
    }


    private void OnMoveCompleted()
    {
        inTransit = false;
        cardActive = activateCardAfterMove;
        movePs.Stop();
        if (discardAfterMove)
        {
            discardAfterMove = false;
            GameManager.Instance.EVENT_CARD_DISABLED.Invoke(thisCardValues.id);
        }


        if (cardActive && ((Vector2)transform.position).magnitude < 0.5f) 
            // if in the center of the screen
        {
            inTransit = true;
            cardActive = false;
            StartCoroutine(ResetAfterTime(GameSettings.CARD_DRAW_SHOW_TIME));
        }
        if (cardActive)
        {
            UpdateCardBasedOnEnergy(currentPlayerEnergy);
        }
    }

    private void HideAndDeactivateCard()
    {
        movePs.Stop();

        inTransit = false;

        if (discardAfterMove)
        {
            discardAfterMove = false;
            GameManager.Instance.EVENT_CARD_DISABLED.Invoke(thisCardValues.id);
        }

        if (!activateCardAfterMove)
        {
            DisableCardContent(false);
        }
        else
        {
            cardActive = true;
        }
    }

    private void UpdateCardBasedOnEnergy(int energy)
    {
        if (thisCardValues.energy <= energy)
        {
            var main = auraPS.main;
            main.startColor = greenColor;
            outlineMaterial = greenOutlineMaterial; //TODO:apply blue if card has a special condition
            energyTF.color = Color.black;
            card_can_be_played = true;
            //Debug.Log($"[CardOnHandManager] [{thisCardValues.name}] Card is now playable {energy}/{thisCardValues.energy}");
        }
        else
        {
            energyTF.color = redColor;
            outlineMaterial = greenOutlineMaterial;
            card_can_be_played = false;
            //Debug.Log($"[CardOnHandManager] [{thisCardValues.name}] Card is no longer playable {energy}/{thisCardValues.energy}");
        }
    }

    public void DisableCardContent(bool notify = false)
    {
        // DOTween.Kill(this.transform);
        this.cardcontent.SetActive(false);
        if (notify) GameManager.Instance.EVENT_CARD_DISABLED.Invoke(thisCardValues.id);
    }

    public void EnableCardContent()
    {
        // DOTween.Kill(this.transform);
        this.cardcontent.SetActive(true);
        ActivateCard();
        cardActive = true;
    }


    private void OnMouseEnter()
    {
        if (cardActive && !Input.GetMouseButton(0))
        {
            // DOTween.PlayForward(this.gameObject);
            // GameManager.Instance.EVENT_CARD_MOUSE_ENTER.Invoke(thisCardValues.cardId);

            ShowUpCard();
        }
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonUp(0)) 
        {
            ShowUpCard();
        }
    }

    private void ShowUpCard()
    {
        if (cardIsShowingUp) return;

        if (cardActive)
        {
            ResetCardPosition();
            DOTween.Kill(this.transform);

            if(card_can_be_played)auraPS.Play();

            cardIsShowingUp = true;

            //  Debug.Log("ShowUp");
            transform.DOScale(Vector3.one * GameSettings.HAND_CARD_SHOW_UP_SCALE, GameSettings.HAND_CARD_SHOW_UP_TIME);


            Vector3 showUpPosition = new Vector3(targetPosition.x, GameSettings.HAND_CARD_SHOW_UP_Y, GameSettings.HAND_CARD_SHOW_UP_Z);
            transform.DOMove(showUpPosition, GameSettings.HAND_CARD_SHOW_UP_TIME);


            transform.DORotate(Vector3.zero, GameSettings.HAND_CARD_SHOW_UP_TIME).OnComplete(() =>
            {
                if (transform.position.x <= 0)
                {
                    Vector3 topRightOfCard = new Vector3(transform.position.x + (collider.bounds.size.x / 2) + 0.2f,
                        transform.position.y + (collider.bounds.size.y / 2), 0);
                    GameManager.Instance.EVENT_SET_TOOLTIPS.Invoke(tooltips, TooltipController.Anchor.TopLeft, topRightOfCard, null);
                }
                else 
                {
                    Vector3 topLeftOfCard = new Vector3(transform.position.x - ((collider.bounds.size.x / 2) + 0.2f),
                            transform.position.y + (collider.bounds.size.y / 2), 0);
                    GameManager.Instance.EVENT_SET_TOOLTIPS.Invoke(tooltips, TooltipController.Anchor.TopRight, topLeftOfCard, null);
                }
                
            });

            GameManager.Instance.EVENT_CARD_SHOWING_UP.Invoke(thisCardValues.id, this.targetPosition);
        }
        else
        {
            auraPS.Stop();
        }
    }

    private void ResetCardPosition()
    {
        if (cardActive)
        {
            // Debug.Log("[ResetCardPosition]");
            if (auraPS.gameObject.activeSelf) auraPS.Stop();

            DOTween.Kill(this.transform);

            cardIsShowingUp = false;
            cardIsDisplaced = false;
            transform.DOMove(targetPosition, GameSettings.HAND_CARD_RESET_POSITION_TIME);
            transform.DOScale(Vector3.one, GameSettings.HAND_CARD_RESET_POSITION_TIME);
            transform.DORotate(targetRotation, GameSettings.HAND_CARD_RESET_POSITION_TIME);
        }
    }

    private void OnMouseExit()
    {
        GameManager.Instance.EVENT_CLEAR_TOOLTIPS.Invoke();

        if (pointerIsActive) return;

        if (cardActive && cardIsShowingUp)
        {
            // Debug.Log("[OnMouseExit]");
            GameManager.Instance.EVENT_CARD_MOUSE_EXIT.Invoke(thisCardValues.id);

            if (cardIsDisplaced)
            {
                // Play Cancellation sound
                GameManager.Instance.EVENT_PLAY_SFX.Invoke("Card Cancel");
            }

            if (!Input.GetMouseButton(0))
                ResetCardPosition();
            else
                awaitMouseUp = true;
        }
    }

    private void Update()
    {
        if (awaitMouseUp && !Input.GetMouseButton(0))
        {
            awaitMouseUp = false;
            ResetCardPosition();
        }

        if (delay > 0)
        {
            delay -= Time.deltaTime;
            if (delay < 0) delay = 0;
        }
    }

    private void OnMouseDown()
    {
        if (!card_can_be_played)
        {
            GameManager.Instance.EVENT_CARD_NO_ENERGY.Invoke();
            return;
        }
    }

    private void OnMouseDrag()
    {
        if (!card_can_be_played)
        {
            //GameManager.Instance.EVENT_CARD_NO_ENERGY.Invoke();
            return;
        }

        if (!cardActive) //card_can_be_played
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

            Vector3 showUpPosition = new Vector3(0, GameSettings.HAND_CARD_SHOW_UP_Y, GameSettings.HAND_CARD_SHOW_UP_Z);
            transform.DOMove(showUpPosition, GameSettings.HAND_CARD_SHOW_UP_TIME);

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
            if(transform.position.y > GameSettings.HAND_CARD_SHOW_UP_Y && card_can_be_played)//if (overPlayer)
            {
                Debug.Log("card is on center");
                // Get Player ID
                GameManager.Instance.EVENT_CARD_PLAYED.Invoke(thisCardValues.id, "-1");
                cardActive = false;
            }
            else
            {
                Debug.Log("card is far from center");
                cardIsDisplaced = true;
                //MoveCardBackToOriginalHandPosition();
            }
        }
    }

    private void PlayerEnter(GameObject obj) 
    {
        lastOver = obj;
        if (obj.CompareTag("Player") && card_can_be_played && transform.position.y > GameSettings.HAND_CARD_SHOW_UP_Y)
        {
            overPlayer = true;
            playerData = obj.GetComponentInChildren<PlayerManager>().PlayerData;
        }
    }

    private void PlayerExit(GameObject obj) 
    {
        if (obj.CompareTag("Player"))
        {
            overPlayer = false;
            playerData = null;
        }
        lastOver = null;
    }
    
    private void OnTriggerStay2D(Collider2D collision)
    {
        bool isOver = false;
        if (collision != null && collision.gameObject.CompareTag("Player"))
        {
            var other = collision.gameObject.GetComponentInParent<PlayerManager>();
            if (other != null)
                isOver = true;
        }
        if (overPlayer != isOver)
        {
            if (isOver)
            {
                PlayerEnter(collision.gameObject);
            }
            else
            {
                if (lastOver != null)
                {
                    PlayerExit(lastOver);
                }
            }
        }
        else if (isOver && collision.gameObject != lastOver)
        {
            if (lastOver != null)
            {
                PlayerExit(lastOver);
            }
            PlayerEnter(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        OnTriggerStay2D(null);
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