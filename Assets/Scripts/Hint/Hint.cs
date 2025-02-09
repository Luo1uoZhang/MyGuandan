using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Search;
using System;


public struct SimpleCard
{
    public int rank;
    public int suit;
    public int priority;
}

public partial class Hint
{
    public Dictionary<int, List<Card>> cardGroupsByRank = new();
    public Dictionary<int, List<Card>> cardGroupsByPriority = new();
    public List<List<SimpleCard>> hints = new();
    private int currentIndex;
    public DownHoldArea downHoldArea;

    public Hint(DownHoldArea downHoldArea)
    {
        this.downHoldArea = downHoldArea;
    }

    public void GenerateHints(HandInfo handInfo)
    {
        Stopwatch sw = new();
        sw.Start();
        currentIndex = 0;

        int countWildCards = downHoldArea.CountWildCards();
        var cards = HandUtil.RemoveWildCards(downHoldArea.cards, countWildCards);
        BuildDictionaries(cards);

        for (int i = 0; i <= countWildCards; i++)
        {
            switch (handInfo.Type)
            {
                case HandType.SINGLE:
                    HintSingle(handInfo.Rank, i);
                    break;
                case HandType.PAIR:
                    HintPair(handInfo.Rank, i);
                    break;
                case HandType.TRIPS:
                    HintTrips(handInfo.Rank, i);
                    break;
                case HandType.THREEWITHTWO:
                    HintThreeWithTwo(handInfo.Rank, i);
                    break;
                case HandType.STRAIGHT:
                    HintStraight(handInfo.Rank, i);
                    break;
                case HandType.TWOTRIPS:
                    HintTwoTrips(handInfo.Rank, i);
                    break;
                case HandType.THREEPAIR:
                    HintThreePair(handInfo.Rank, i);
                    break;
                case HandType.BOMB:
                    HintBomb(handInfo.Rank, i);
                    break;
                default:
                    HintSingle(handInfo.Rank, i);
                    HintPair(handInfo.Rank, i);
                    HintTrips(handInfo.Rank, i);
                    HintThreeWithTwo(handInfo.Rank, i);
                    HintStraight(handInfo.Rank, i);
                    HintTwoTrips(handInfo.Rank, i);
                    HintThreePair(handInfo.Rank, i);
                    break;
            }
        }

        if (handInfo.Type != HandType.BOMB)
        {
            for (int i = 0; i <= countWildCards; i++)
            {
                HintBomb(handInfo.Rank, i);
            }

        }
        sw.Stop();
        UnityEngine.Debug.Log("Generated " + hints.Count + " hints, cost " + sw.ElapsedMilliseconds + " ms");
    }

    private void BuildDictionaries(List<Card> cards)
    {
        cardGroupsByRank.Clear();
        cardGroupsByPriority.Clear();
        hints.Clear();
        foreach (Card card in cards)
        {
            if (!cardGroupsByRank.ContainsKey(card.rank))
            {
                cardGroupsByRank.Add(card.rank, new List<Card> { card });
            }
            else
            {
                cardGroupsByRank[card.rank].Add(card);
            }

            if (!cardGroupsByPriority.ContainsKey(card.priority))
            {
                cardGroupsByPriority.Add(card.priority, new List<Card> { card });
            }
            else
            {
                cardGroupsByPriority[card.priority].Add(card);
            }

            if (card.rank == 14)
            {
                if (!cardGroupsByRank.ContainsKey(1))
                {
                    cardGroupsByRank.Add(1, new List<Card> { card });
                }
                else
                {
                    cardGroupsByRank[1].Add(card);
                }
            }
        }
    }

    private void HintSingle(int priority, int countWildCards)
    {
        switch (countWildCards)
        {
            case 0:
                var hint = GetAllSingle(priority);
                hints.AddRange(hint);
                break;
            case 1:
                HintSingleWithOneWild(priority);
                break;
            default:
                break;

        }
        // 暂时先这么用着，如果后面在枚举其他牌型的时候有问题了再回顾一下看看解决方案吧
    }

    private List<List<SimpleCard>> GetAllSingle(int priority)
    {
        List<List<SimpleCard>> results = new();
        for (int i = priority + 1; i < 18; i++)
        {
            List<List<SimpleCard>> cards = DifferentSuitCards(By.Priority, i);
            if (cards == null)
            {
                UnityEngine.Debug.Log("No cards found for rank " + i);
            }
            else
            {
                foreach (List<SimpleCard> cardsList in cards)
                {
                    if (cardsList == null) continue;
                    results.Add(cardsList);
                }
            }
        }
        return results;
    }

    public List<SimpleCard> NextHint()
    {
        if (hints.Count > 0)
            return hints[currentIndex++ % hints.Count];
        return new();
    }

    // private List<List<SimpleCard>> DifferentSuitCards(int rank)
    // {
    //     if (!cardGroupsByRank.ContainsKey(rank))
    //     {
    //         return new List<List<SimpleCard>>();
    //     }
    //     List<int> suits = new();
    //     suits.AddRange(from Card card in cardGroupsByRank[rank]
    //                    select card.suit);
    //     suits = suits.Distinct().ToList();

    //     List<List<SimpleCard>> results = new();
    //     foreach (int suit in suits)
    //     {
    //         SimpleCard card = new() { rank = rank, suit = suit };
    //         results.Add(new List<SimpleCard> { card });
    //     }
    //     return results;
    // }

    private List<List<SimpleCard>> DifferentSuitCards(By by, int value)
    {
        var query = by == By.Rank ? cardGroupsByRank : cardGroupsByPriority;

        if (!query.ContainsKey(value))
        {
            return new List<List<SimpleCard>>();
        }
        List<int> suits = new();
        suits.AddRange(from Card card in query[value]
                       select card.suit);
        suits = suits.Distinct().ToList();

        List<List<SimpleCard>> results = new();
        foreach (int suit in suits)
        {
            SimpleCard card = new();
            if (by == By.Rank)
            {
                card.rank = value;
                card.suit = suit;
            }
            else
            {
                card.priority = value;
                card.suit = suit;
            }
            results.Add(new List<SimpleCard> { card });
        }
        return results;
    }

    private void HintPair(int priority, int countWildCards)
    {
        // 在枚举三带二的时候，实际是枚举三不带+枚举对子，所以hashset依然没问题
        switch (countWildCards)
        {
            case 0:
                HintPairWithNoWild(priority);
                break;
            case 1:
                HintPairWithOneWild(priority);
                break;
            default:
                HintPairWithTwoWild(priority);
                break;
        }
    }

    private void HintPairWithNoWild(int priority)
    {
        var x = GetPairWithNoWild(priority);
        hints.AddRange(x);
    }

    private List<List<SimpleCard>> GetPairWithNoWild(int priority)
    {
        List<List<SimpleCard>> results = new();
        for (int i = priority + 1; i < 18; i++)
        {
            if (!cardGroupsByPriority.ContainsKey(i)) continue;

            var cards = cardGroupsByPriority[i];
            if (cards.Count < 2) continue;
            List<List<SimpleCard>> result = HandUtil.GetAllKTuples(cards, 2);
            results.AddRange(result);
        }
        return results;
    }

    // Acutually, HintPair and HintTrips only differs in the value passed into GetAllKTuples, 
    // which means they can be abstracted into a single method.
    // And so do HintSingle.
    private void HintTrips(int priority, int countWildCards)
    {
        switch (countWildCards)
        {
            case 0:
                HintTripsWithNoWild(priority);
                break;
            case 1:
                HintTripsWithOneWild(priority);
                break;
            default:
                HintTripsWithTwoWild(priority);
                break;
        }
    }

    private void HintTripsWithNoWild(int priority)
    {
        for (int i = priority + 1; i < 18; i++)
        {
            if (!cardGroupsByPriority.ContainsKey(i)) continue;

            var cards = cardGroupsByPriority[i];
            if (cards.Count < 3) continue;
            List<List<SimpleCard>> result = HandUtil.GetAllKTuples(cards, 3);
            hints.AddRange(result);
        }
    }

    private void HintThreeWithTwo(int priority, int countWildCards)
    {
        switch (countWildCards)
        {
            case 0:
                HintThreeWithTwoWithNoWild(priority);
                break;
            case 1:
                HintThreeWithTwoWithOneWild(priority);
                break;
            default:
                HintThreeWithTwoWithTwoWild(priority);
                break;
        }
    }

    private void HintThreeWithTwoWithNoWild(int priority)
    {
        List<List<SimpleCard>> pairs = new();
        for (int i = 2; i < 18; i++)
        {
            if (!cardGroupsByPriority.ContainsKey(i)) continue;

            var cards = cardGroupsByPriority[i];
            if (cards.Count < 2) continue;
            List<List<SimpleCard>> result = HandUtil.GetAllKTuples(cards, 2);
            pairs.AddRange(result);
        }

        List<List<SimpleCard>> trips = new();
        for (int i = priority + 1; i < 18; i++)
        {
            if (!cardGroupsByPriority.ContainsKey(i)) continue;

            var cards = cardGroupsByPriority[i];
            if (cards.Count < 3) continue;
            List<List<SimpleCard>> result = HandUtil.GetAllKTuples(cards, 3);
            trips.AddRange(result);
        }

        List<List<SimpleCard>> results = HandUtil.MergeTuples(trips, pairs);
        hints.AddRange(results);
    }

    private void HintStraight(int rank, int countWildCards)
    {
        switch (countWildCards)
        {
            case 0:
                HintStraightWithNoWild(rank);
                break;
            case 1:
                HintStraightWithOneWild(rank);
                break;
            default:
                HintStraightWithTwoWild(rank);
                break;
        }
    }

    private void HintStraightWithNoWild(int rank)
    {
        var results = GetAllStraight(rank);
        RemoveOrRetainFlush(results, true);
        hints.AddRange(results);
    }

    // I am genius.
    private void RemoveOrRetainFlush(List<List<SimpleCard>> cards, bool remove)
    {
        cards.RemoveAll(x => remove ? IsSimpleCardFlush(x) : !IsSimpleCardFlush(x));
    }

    private bool IsSimpleCardFlush(List<SimpleCard> cards)
    {
        for (int i = 1; i < cards.Count; i++)
        {
            if (cards[i].suit != cards[i - 1].suit)
            {
                return false;
            }
        }
        return true;
    }

    private List<List<SimpleCard>> GetAllStraight(int rank)
    {
        // We should first handle the special situation where Ace can be used as 1.
        // There are only 1 case we should handle this situation: we are the leader. means rank == -1.

        // 有一个Bug（特性），这里不会因为“没有某张牌导致顺子变成4张”，是因为merge的时候
        // 如果 tuplesA 或者 tuplesB 为空，则会直接返回空，导致结果为空。

        List<List<SimpleCard>> results = new();
        if (rank < 5)
        {
            results = HandUtil.GetAllKTuples(cardGroupsByRank.GetValueOrDefault(14), 1);
            for (int i = 2; i <= 5; i++)
            {
                var x = HandUtil.GetAllKTuples(cardGroupsByRank.GetValueOrDefault(i), 1);
                results = HandUtil.MergeTuples(results, x);
            }
        }

        for (int i = Math.Max(6, rank + 1); i <= 14; i++)
        {
            List<List<SimpleCard>> rankIStraights = HandUtil.GetAllKTuples(cardGroupsByRank.GetValueOrDefault(i - 4), 1);
            for (int j = i - 3; j <= i; j++)
            {
                var x = HandUtil.GetAllKTuples(cardGroupsByRank.GetValueOrDefault(j), 1);
                rankIStraights = HandUtil.MergeTuples(rankIStraights, x);
            }

            results.AddRange(rankIStraights);
        }

        return results;
    }

    /// <summary>
    /// 4 Bomb is in (18, 36)
    /// 5 Bomb is in (36, 54)
    /// Flush is in (54, 72)
    /// 6 Bomb is in (72, 90)
    /// 7 Bomb is in (90, 108)
    /// 8 Bomb is in (108, 126)
    /// 9 Bomb is in (126, 144)
    /// 10 Bomb is in (144, 162) Of course 9 and 10 Bombs must contains 2 wild cards.
    /// 4 Jokers is 999.
    /// </summary>
    /// <param name="rank"></param>
    private void HintBomb(int rank, int countWildCards)
    {
        switch (countWildCards)
        {
            case 0:
                HintBombWithNoWild(rank);
                break;
            case 1:
                HintBombWithOneWild(rank);
                break;
            case 2:
                HintBombWithTwoWild(rank);
                break;
            default:
                break;
        }
    }

    private void HintBombWithNoWild(int rank)
    {
        int gameRank = GameRankManager.Instance.GetGameRank();
        List<List<SimpleCard>> results = new();
        for (int count = 4; count <= 5; count++)
        {
            for (int i = 2; i <= 15; i++)
            {
                int bombRank = (i == gameRank ? 15 : i) + (count - 3) * 18;
                if (bombRank <= rank) continue;

                var bomb = HandUtil.GetAllKTuples(cardGroupsByPriority.GetValueOrDefault(i), count);
                results.AddRange(bomb);
            }
        }

        var flushes = GetAllStraight(-1);
        RemoveOrRetainFlush(flushes, false);
        foreach (var flush in flushes)
        {
            int bombRank = flush[^1].rank + 54;
            if (bombRank <= rank) continue;
            results.Add(flush);
        }

        for (int count = 6; count <= 8; count++)
        {
            for (int i = 2; i <= 15; i++)
            {
                int bombRank = (i == gameRank ? 15 : i) + (count - 3) * 18;
                if (bombRank <= rank) continue;

                var bomb = HandUtil.GetAllKTuples(cardGroupsByPriority.GetValueOrDefault(i), count);
                results.AddRange(bomb);
            }
        }
        hints.AddRange(results);

        if (cardGroupsByRank.ContainsKey(16) && cardGroupsByRank[16].Count == 2 && cardGroupsByRank.ContainsKey(17) && cardGroupsByRank[17].Count == 2)
        {
            List<SimpleCard> Jokers = new();
            for (int i = 16; i <= 17; i++)
            {
                for (int j = 0; j <= 1; j++)
                {
                    Jokers.Add(new SimpleCard { rank = i, suit = i == 16 ? 0 : 1 });
                }
            }
            hints.Add(Jokers);
        }
    }

    private void HintTwoTrips(int rank, int countWildCards)
    {
        // Same as Hint Straight, we should handle the special situation where Ace can be used as 1.
        switch (countWildCards)
        {
            case 0:
                HintTwoTripsWithNoWild(rank);
                break;
            case 1:
                HintTwoTripsWithOneWild(rank);
                break;
            case 2:
                HintTwoTripsWithTwoWild(rank);
                break;
            default:
                break;
        }
    }

    private void HintTwoTripsWithNoWild(int rank)
    {
        List<List<SimpleCard>> results = new();
        if (rank < 2)
        {
            results = HandUtil.GetAllKTuples(cardGroupsByRank.GetValueOrDefault(14), 3);
            var x = HandUtil.GetAllKTuples(cardGroupsByRank.GetValueOrDefault(2), 3);
            results = HandUtil.MergeTuples(results, x);
        }

        for (int i = Math.Max(3, rank + 1); i <= 14; i++)
        {
            List<List<SimpleCard>> TripsA = HandUtil.GetAllKTuples(cardGroupsByRank.GetValueOrDefault(i - 1), 3);
            List<List<SimpleCard>> TripsB = HandUtil.GetAllKTuples(cardGroupsByRank.GetValueOrDefault(i), 3);

            var x = HandUtil.MergeTuples(TripsA, TripsB);
            results.AddRange(x);
        }

        hints.AddRange(results);
    }

    private void HintThreePair(int rank, int countWildCards)
    {
        switch (countWildCards)
        {
            case 0:
                HintThreePairWithNoWild(rank);
                break;
            case 1:
                HintThreePairWithOneWild(rank);
                break;
            case 2:
                HintThreePairWithTwoWild(rank);
                break;
            default:
                break;
        }
    }

    private void HintThreePairWithNoWild(int rank)
    {
        List<List<SimpleCard>> results = new();
        if (rank < 3)
        {
            results = HandUtil.GetAllKTuples(cardGroupsByRank.GetValueOrDefault(14), 2);
            for (int i = 2; i <= 3; i++)
            {
                var x = HandUtil.GetAllKTuples(cardGroupsByRank.GetValueOrDefault(i), 2);
                results = HandUtil.MergeTuples(results, x);
            }
        }

        for (int i = Math.Max(4, rank + 1); i <= 14; i++)
        {
            List<List<SimpleCard>> threePair = HandUtil.GetAllKTuples(cardGroupsByRank.GetValueOrDefault(i - 2), 2);
            for (int j = i - 1; j <= i; j++)
            {
                List<List<SimpleCard>> pair = HandUtil.GetAllKTuples(cardGroupsByRank.GetValueOrDefault(j), 2);
                threePair = HandUtil.MergeTuples(threePair, pair);
            }
            results.AddRange(threePair);
        }

        hints.AddRange(results);
    }
}