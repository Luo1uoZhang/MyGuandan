
using UnityEngine;
using System.Collections.Generic;

public class HandArea
{
    private Vector3 center;
    private int moveAxis;
    private List<Card> cards = new();
    public HandArea(Vector3 center, int moveAxis)
    {
        this.center = center;
        this.moveAxis = moveAxis;
    }

    public void PushHand(List<Card> cards)
    {
        ClearHand();
        if (cards.Count == 0) return;
        
        float offset = - (cards.Count + 1) / 2;
        Vector3 to = new(center.x, center.y, center.z);
        foreach (Card card in cards)
        {
            CardBehaviour cb = card.cardObject.GetComponent<CardBehaviour>();
            cb.ResetCard();
            cb.SetCanPick(false);
            to = NextTo(to);
            LeanTween.move(card.cardObject, to, 0.05f);
            offset += 1;
        }
        this.cards = cards;
    }

    private Vector3 NextTo(Vector3 to)
    {
        Vector3 nextTo = to;
        if (moveAxis == 0)
        {
            nextTo.x += 0.4f;
        }
        else
        {
            nextTo.y -= 0.4f;
        }
        nextTo.z -= 0.1f;
        return nextTo;
    }

    public void ClearHand()
    {
        foreach (Card card in cards)
        {
            CardObjectPool.Instance.ReturnCard(card);
        }

        cards.Clear();
    }

    public void ClearHandAtTributePeriod()
    {
        cards.Clear();
    }
}