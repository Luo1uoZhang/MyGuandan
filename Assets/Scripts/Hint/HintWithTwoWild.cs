using System;
using System.Collections.Generic;

public partial class Hint
{
    private SimpleCard MakeWildCard()
    {
        return new SimpleCard() { priority = 15, suit = 1 };
    }

    private List<SimpleCard> MakeWildCards(int count)
    {
        List<SimpleCard> cards = new();
        for (int i = 0; i < count; i++)
        {
            cards.Add(MakeWildCard());
        }
        return cards;
    }

    private void HintPairWithTwoWild(int priority)
    {
        if (priority >= 15) return;
        hints.Add(MakeWildCards(2));
    }

    private void HintTripsWithTwoWild(int priority)
    {
        for (int i = Math.Max(2, priority + 1); i <= 15; i++)
        {
            List<List<SimpleCard>> trips = HandUtil.GetAllKTuples(cardGroupsByPriority.GetValueOrDefault(i), 1);
            foreach (List<SimpleCard> trip in trips)
            {
                trip.AddRange(MakeWildCards(2));
            }
            hints.AddRange(trips);
        }
    }

    private void HintThreeWithTwoWithTwoWild(int priority)
    {
        // 2 + 1 or 1 + 2

        // case 2 + 1
        var onesAsThree = GetSubSequencesByPriority(priority, 1);
        var twosAsTwo = GetSubSequencesByPriority(-1, 2);
        RemoveJoker(onesAsThree);

        foreach (List<SimpleCard> three in onesAsThree)
        {
            int priorityOfThree = three[0].priority;
            foreach (List<SimpleCard> two in twosAsTwo)
            {
                if (two[0].priority < 16 && two[0].priority >= priorityOfThree)
                {
                    continue;
                }
                List<SimpleCard> threeWithTwo = new();
                threeWithTwo.AddRange(three);
                threeWithTwo.AddRange(MakeWildCards(2));
                threeWithTwo.AddRange(two);

                hints.Add(threeWithTwo);
            }
        }

        // case 1 + 2
        var twosAsThree = GetSubSequencesByPriority(priority, 2);
        var onesAsTwo = GetSubSequencesByPriority(-1, 1);
        RemoveJoker(twosAsThree);
        RemoveJoker(onesAsTwo);

        foreach (List<SimpleCard> three in twosAsThree)
        {
            int priorityOfThree = three[0].priority;
            foreach (List<SimpleCard> two in onesAsTwo)
            {
                if (two[0].priority >= priorityOfThree)
                {
                    continue;
                }
                List<SimpleCard> threeWithTwo = new();
                threeWithTwo.AddRange(three);
                threeWithTwo.AddRange(MakeWildCards(2));
                threeWithTwo.AddRange(two);

                hints.Add(threeWithTwo);
            }
        }
    }

    private void HintTwoTripsWithTwoWild(int rank)
    {
        // 3 + 1, 2 + 2, 1 + 3
        for (int i = Math.Max(2, rank + 1); i <= 14; i++)
        {
            HintTwoTripsWithTwoWildOfRank(i, 1);
            HintTwoTripsWithTwoWildOfRank(i, 2);
            HintTwoTripsWithTwoWildOfRank(i, 3);
        }
    }

    private void HintTwoTripsWithTwoWildOfRank(int rank, int numberOfSecond)
    {
        if (numberOfSecond < 1 || numberOfSecond > 3) return;

        int numberOfFirst = 4 - numberOfSecond;

        var firstTrip = HandUtil.GetAllKTuples(cardGroupsByRank.GetValueOrDefault(rank - 1), numberOfFirst);
        var secondTrip = HandUtil.GetAllKTuples(cardGroupsByRank.GetValueOrDefault(rank), numberOfSecond);
        List<List<SimpleCard>> twoTrips = HandUtil.MergeTuples(firstTrip, secondTrip);
        foreach (List<SimpleCard> trip in twoTrips)
        {
            trip.AddRange(MakeWildCards(2));
        }
        hints.AddRange(twoTrips);
    }

    private void HintThreePairWithTwoWild(int rank)
    {
        // 2 1 1 || 1 2 1 || 1 1 2 || 2 2 0 || 2 0 2
        for (int i = Math.Max(3, rank + 1); i <= 14; i++)
        {
            HintSpecificThreePairWithTwoWild(i, 2, 1, 1);
            HintSpecificThreePairWithTwoWild(i, 1, 2, 1);
            HintSpecificThreePairWithTwoWild(i, 1, 1, 2);
            HintSpecificThreePairWithTwoWild(i, 0, 2, 2);
            HintSpecificThreePairWithTwoWild(i, 2, 0, 2);
        }
    }

    private void HintSpecificThreePairWithTwoWild(int rank, int first, int second, int third)
    {
        List<List<SimpleCard>> threePairs;
        var firstPair = HandUtil.GetAllKTuples(cardGroupsByRank.GetValueOrDefault(rank), first);
        var secondPair = HandUtil.GetAllKTuples(cardGroupsByRank.GetValueOrDefault(rank - 1), second);
        if (first == 0)
        {
            threePairs = secondPair;
        }
        else if (second == 0)
        {
            threePairs = firstPair;
        }
        else
        {
            threePairs = HandUtil.MergeTuples(firstPair, secondPair);
        }
        var thirdPair = HandUtil.GetAllKTuples(cardGroupsByRank.GetValueOrDefault(rank - 2), third);

        threePairs = HandUtil.MergeTuples(threePairs, thirdPair);
        foreach (List<SimpleCard> threePair in threePairs)
        {
            threePair.AddRange(MakeWildCards(2));
        }
        hints.AddRange(threePairs);
    }

    private void HintStraightWithTwoWild(int rank)
    {
        // 11100 || 11010 || 10110 || 10011 || 10101 || 11001
        // 28 || 26 || 22 || 19 || 21 || 25

        for (int r = Math.Max(5, rank + 1); r <= 14; r++)
        {
            var x = StraightOfRankWithTwoWild(r);
            hints.AddRange(x);
        }
    }

    private List<List<SimpleCard>> StraightOfRankWithTwoWild(int rank, bool removeFlush = true)
    {
        List<List<SimpleCard>> straights = new();
        foreach (int i in new int[] { 19, 21, 22, 25, 26, 28 })
        {
            List<List<SimpleCard>> straight = HandUtil.GetAllKTuples(cardGroupsByRank.GetValueOrDefault(rank - 4), 1);
            for (int j = 3; j >= 0; j--)
            {
                if ((i & (1 << j)) == 0) continue;

                straight = HandUtil.MergeTuples(straight, HandUtil.GetAllKTuples(cardGroupsByRank.GetValueOrDefault(rank - j), 1));
            }
            RemoveOrRetainFlush(straight, removeFlush);
            foreach (List<SimpleCard> s in straight)
            {
                s.AddRange(MakeWildCards(2));
            }
            straights.AddRange(straight);
        }
        return straights;
    }

    private void HintBombWithTwoWild(int priority)
    {
        for (int i = 2; i <= 3; i++)
        {
            for (int p = 2; p <= 15; p++)
            {
                if (18 * (i - 1) + p < priority) continue;

                List<List<SimpleCard>> bomb = HandUtil.GetAllKTuples(cardGroupsByPriority.GetValueOrDefault(p), i);
                foreach (List<SimpleCard> hand in bomb)
                {
                    hand.AddRange(MakeWildCards(2));
                }
                hints.AddRange(bomb);
            }
        }
        
        for (int i = 5; i <= 14; i++)
        {
            if (54 + i < priority) continue;
            List<List<SimpleCard>> flush = StraightOfRankWithTwoWild(i, false);
            hints.AddRange(flush);
        }

        for (int i = 4; i <= 8; i++)
        {
            for (int p = 2; p <= 15; p++)
            {
                if (18 * i + p < priority) continue;

                List<List<SimpleCard>> bomb = HandUtil.GetAllKTuples(cardGroupsByPriority.GetValueOrDefault(p), i);
                foreach (List<SimpleCard> hand in bomb)
                {
                    hand.AddRange(MakeWildCards(2));
                }
                hints.AddRange(bomb);
            }
        }
    }
}