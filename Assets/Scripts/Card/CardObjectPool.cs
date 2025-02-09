using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 常规牌的牌序是`2, 3, 4, 5, 6, 7, 8, 9, T, J, Q, K, A, BJ, HJ`，级牌在小王之下。
// 牌的rank肯定还是用int表示最好，`A`的rank是14，那么`BJ` 、`HJ`的rank应该被置为16和17。
// 这里会有个问题，在打出的手牌是顺子时，如果最大的牌刚好是级牌，他的rank表示会有问题。
// 那么，我们考虑级牌会影响哪些牌型：单张、对子、三不带、三带二、炸弹。
// 当前，我们把同花顺也归类为炸弹，并且炸弹的Rank是（张数 - 4）* 16 + Rank，这导致此处的修正会有一些问题。
// 所以，一个解决方案是在打出受级牌影响的手牌时，设置他的rank。
// 这种做法又有一个问题，我们当然希望级牌被显示在小王的旁边，这该怎么实现呢？

public class CardObjectPool
{
    private int gameRank;

    private static CardObjectPool instance = new();
    public static CardObjectPool Instance
    {
        get
        {
            return instance;
        }
    }
    private readonly Dictionary<int, Dictionary<int, Queue<Card>>> pool = new();

    private List<HoldArea> HoldAreas = new();
    private CardObjectPool()
    {

    }

    public void SetHoldAreas(List<HoldArea> HoldAreas)
    {
        this.HoldAreas = HoldAreas;
    }

    public void ReturnAllCards()
    {
        foreach (HoldArea HoldArea in HoldAreas)
        {
            HoldArea.ReturnAllCards();
        }
    }

    public void Init()
    {
        GameRankManager.Instance.GetGameRank();
        pool.Clear();
        for (int i = 0; i < 4; i++)
        {
            if (!pool.ContainsKey(i))
            {
                pool[i] = new Dictionary<int, Queue<Card>>();
            }
            for (int j = 2; j < 18; j++)
            {
                if (!pool[i].ContainsKey(j))
                {
                    pool[i][j] = new Queue<Card>();
                }
            }
        }
        for (int k = 0; k < 2; k++)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 2; j <= 14; j++)
                {
                    CreateCard(i, j);
                }
            }
            CreateCard(0, 16);
            CreateCard(1, 17);
        }
    }

    private void CreateCard(int suit, int rank)
    {
        GameObject cardObject = Object.Instantiate(Resources.Load<GameObject>("Prefabs/Card1"));
        Card card = new(suit, rank, cardObject);
        pool[suit][rank].Enqueue(card);
    }

    public List<Card> GetCards()
    {
        List<Card> cards = new();
        int gameRank = GameRankManager.Instance.GetGameRank();

        foreach (var poolNode in pool.Values)
        {
            foreach (var poolCards in poolNode.Values)
            {
                while (poolCards.Count > 0)
                {
                    Card card = poolCards.Dequeue();
                    card.cardObject.SetActive(true);
                    card.SetPriority(card.rank == gameRank ? 15 : card.rank);
                    cards.Add(card);
                }
            }
        }

        return cards;
    }

    public Card GetCard(int suit, int rank)
    {
        if (pool.ContainsKey(suit) && pool[suit].ContainsKey(rank) && pool[suit][rank].Count > 0)
        {
            return pool[suit][rank].Dequeue();
        }
        else
        {
            Debug.Log("Card not found in pool");
            return null;
        }
    }
    public void ReturnCard(Card card)
    {
        CardBehaviour cb = card.cardObject.GetComponent<CardBehaviour>();
        cb.ResetCard();
        cb.SetCanPick(true);
        card.cardObject.SetActive(false);
        pool[card.suit][card.rank].Enqueue(card);
    }
}