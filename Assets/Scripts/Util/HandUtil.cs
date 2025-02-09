using System;
using System.Collections.Generic;

public static class HandUtil
{
    public static List<List<SimpleCard>> GetAllKTuples(List<Card> cards, int k)
    {
        if (cards == null || cards.Count < k) return new List<List<SimpleCard>>();

        HashSet<int> hashSet = new();
        List<List<SimpleCard>> result = new();

        int upperBound = (int)Math.Pow(2, cards.Count);
        for (int i = 0; i < upperBound; i++)
        {
            int bitCounts = CommonUtil.CountBit(i);
            if (bitCounts != k) continue;

            int hashValue = 0;
            List<SimpleCard> tuple = new();
            for (int j = 0; j < cards.Count; j++)
            {
                if ((i & (1 << j)) == 0) continue;

                hashValue += (cards[j].suit + 10) * 100 + cards[j].rank;

                tuple.Add(new SimpleCard() { rank = cards[j].rank, suit = cards[j].suit, priority = cards[j].priority });
            }
            if (hashSet.Contains(hashValue)) continue;

            hashSet.Add(hashValue);
            result.Add(tuple);
        }

        return result;
    }

    public static List<List<SimpleCard>> MergeTuples(List<List<SimpleCard>> tuplesA, List<List<SimpleCard>> tuplesB)
    {
        List<List<SimpleCard>> result = new();
        foreach (List<SimpleCard> tupleA in tuplesA)
        {
            foreach (List<SimpleCard> tupleB in tuplesB)
            {
                if (tupleA[0].rank == tupleB[0].rank || tupleA[0].priority == tupleB[0].priority) continue;

                List<SimpleCard> mergedTuple = new();
                mergedTuple.AddRange(tupleA);
                mergedTuple.AddRange(tupleB);
                result.Add(mergedTuple);
            }
        }
        return result;
    }

    public static List<Card> RemoveWildCards(List<Card> cards, int k)
    {
        int gameRank = GameRankManager.Instance.GetGameRank();

        List<Card> result = new();
        int count = 0;

        foreach (Card card in cards)
        {
            if (count >= k)
            {
                result.Add(card);
            }
            else
            {
                if (card.rank == gameRank && card.suit == 1)
                {
                    count++;
                }
                else
                {
                    result.Add(card);
                }
            }
        }
        return result;
    }
}