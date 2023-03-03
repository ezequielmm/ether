using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using KOTE.Expedition.Combat.Cards;
using KOTE.Expedition.Combat.Cards.Piles;
using UnityEngine;

public class CardAnimationQueue : MonoBehaviour
{
    // CardPilesManager and CardMovementManager are one module, but are split for cleanlieness
    public CardPilesManager handManager;
    private Queue<Sequence> movementQueue = new Queue<Sequence>();
    private Sequence activeSequence;

    private void Start()
    {
        GameManager.Instance.EVENT_MOVE_CARDS.AddListener(OnMoveCards);
    }

    private void OnMoveCards(List<(CardToMoveData, float)> cardMoveData)
    {
        Sequence sequence = DOTween.Sequence();
        foreach ((CardToMoveData, float) cardMove in cardMoveData)
        {
            if (handManager.MasterCardList.ContainsKey(cardMove.Item1.id))
            {
                Sequence movementSequence = handManager.MasterCardList[cardMove.Item1.id]
                    .GetComponent<CardManager>()
                    .OnCardToMove(cardMove.Item1, cardMove.Item2);

                sequence.Insert(0, movementSequence);
                handManager.UpdatePilesOnMove(cardMove.Item1.id, cardMove.Item1.source, cardMove.Item1.destination);
            }
            else
            {
                Debug.LogWarning(
                    $"[CardMovementManager] Trying to move card but no card with id {cardMove.Item1.id} exists!");
            }
        }

        Debug.Log("Card Move added to movement queue");
        AddSequenceToQueue(sequence);
    }

    private void PlayNext()
    {
        if (movementQueue.Count == 0)
        {
            activeSequence = null;
            // delay a little to let things settle
            StartCoroutine(WaitToRearrange());
        }
        else
        {
            activeSequence = movementQueue.Dequeue();
            activeSequence.Play();
        }
    }

    private void AddSequenceToQueue(Sequence sequence)
    {
        sequence.OnKill(() => PlayNext());
        if (activeSequence != null)
        {
            movementQueue.Enqueue(sequence);
            return;
        }

        activeSequence = sequence;
        activeSequence.Play();
    }

    private IEnumerator WaitToRearrange()
    {
        yield return new WaitForSeconds(0.5f);

        if (movementQueue.Count <= 0)
        {
            GameManager.Instance.EVENT_REARRANGE_HAND.Invoke();
        }
    }
}