using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public partial class Hint
{
    private void HintSingleWithOneWild(int priority)
    {
        var x = GetSingleWithOneWild(priority);
        hints.AddRange(x);
    }

    private List<List<SimpleCard>> GetSingleWithOneWild(int priority)
    {
        if (priority < 15)
        {
            return new() { new() { new SimpleCard() { priority = 15, suit = 1 } } };
        }
        return new();
    }

    private void HintPairWithOneWild(int priority)
    {
        var x = GetPairWithOneWild(priority);
        hints.AddRange(x);
    }

    private List<List<SimpleCard>> GetPairWithOneWild(int priority)
    {
        List<List<SimpleCard>> allSingle = GetAllSingle(priority);
        RemoveJoker(allSingle);

        foreach (List<SimpleCard> hand in allSingle)
        {
            hand.Add(new SimpleCard() { priority = 15, suit = 1 });
        }

        return allSingle;
    }

    // GetAllSingle creates a list of SimpleCard, where SimpleCard only have priority and suit declared.
    private void RemoveJoker(List<List<SimpleCard>> hint)
    {
        int x = hint.RemoveAll(x => x.Exists(y => y.rank >= 16 || y.priority >= 16));
    }

    private void HintTripsWithOneWild(int priority)
    {
        var x = GetPairWithNoWild(priority);
        RemoveJoker(x);

        foreach (List<SimpleCard> hand in x)
        {
            hand.Add(new SimpleCard() { priority = 15, suit = 1 });
            hints.Add(hand);
        }
    }

    private void HintThreeWithTwoWithOneWild(int priority)
    {
        // TODO: Two possible ways. But we need to handle jokers.

        // Case 1
        var threes = GetSubSequencesByPriority(priority, 3);
        var singles = GetSubSequencesByPriority(-1, 1);
        RemoveJoker(singles);

        var threeWithTwos = HandUtil.MergeTuples(threes, singles);
        foreach (List<SimpleCard> hand in threeWithTwos)
        {
            hand.Add(new SimpleCard() { priority = 15, suit = 1 });
        }
        hints.AddRange(threeWithTwos);

        var twosAsThree = GetSubSequencesByPriority(priority, 2);
        var twos = GetSubSequencesByPriority(-1, 2);
        RemoveJoker(twosAsThree);
        // threeWithTwos = HandUtil.MergeTuples(twosAsThree, twos);
        foreach (List<SimpleCard> three in twosAsThree)
        {
            int priorityOfThree = three[0].priority;
            foreach (List<SimpleCard> two in twos)
            {
                if (two[0].priority < 16 && two[0].priority >= priorityOfThree)
                {
                    continue;
                }
                List<SimpleCard> threeWithTwo = new();
                threeWithTwo.AddRange(three);
                threeWithTwo.Add(new SimpleCard() { priority = 15, suit = 1 });
                threeWithTwo.AddRange(two);

                hints.Add(threeWithTwo);
            }
        }
    }

    // This method is to get all k-tuples of all cards with priority higher than input parameter
    // We are sure that we wont use this method when calculating
    // three pairs or two trips or 
    private List<List<SimpleCard>> GetSubSequencesByPriority(int priority, int k)
    {
        List<List<SimpleCard>> result = new();
        for (int i = Math.Max(2, priority + 1); i < 18; i++)
        {
            var x = HandUtil.GetAllKTuples(cardGroupsByPriority.GetValueOrDefault(i), k);
            result.AddRange(x);
        }

        return result;
    }

    private void HintTwoTripsWithOneWild(int rank)
    {
        // 2 + 3 or 3 + 2
        for (int i = Math.Max(2, rank + 1); i <= 14; i++)
        {
            SearchTwoTripsWithOneWild(i, i - 1, true);
            SearchTwoTripsWithOneWild(i, i - 1, false);
        }
    }

    private void SearchTwoTripsWithOneWild(int firstTripRank, int secondTripRank, bool firstTripUseWild)
    {
        var firstTrip = HandUtil.GetAllKTuples(cardGroupsByRank.GetValueOrDefault(firstTripRank), firstTripUseWild ? 2 : 3);
        if (firstTripUseWild)
        {
            foreach (List<SimpleCard> hand in firstTrip)
            {
                hand.Add(new SimpleCard() { priority = 15, suit = 1 });
            }
        }
        var secondTrip = HandUtil.GetAllKTuples(cardGroupsByRank.GetValueOrDefault(secondTripRank), firstTripUseWild ? 3 : 2);
        if (!firstTripUseWild)
        {
            foreach (List<SimpleCard> hand in secondTrip)
            {
                hand.Add(new SimpleCard() { priority = 15, suit = 1 });
            }
        }

        var twoTrips = HandUtil.MergeTuples(firstTrip, secondTrip);
        hints.AddRange(twoTrips);
    }

    private void HintThreePairWithOneWild(int rank)
    {
        // 122, 212, 221
        if (rank < 3)
        {
            SearchThreePairWithOneWild(14, 2, 3);
        }

        for (int i = Math.Max(4, rank + 1); i <= 14; i++)
        {
            SearchThreePairWithOneWild(i - 2, i - 1, i);
        }
    }

    private void SearchThreePairWithOneWild(int firstPairRank, int secondPairRank, int thirdPairRank)
    {
        SearchThreePairWithOneWild(firstPairRank, secondPairRank, thirdPairRank, 0);
        SearchThreePairWithOneWild(firstPairRank, secondPairRank, thirdPairRank, 1);
        SearchThreePairWithOneWild(firstPairRank, secondPairRank, thirdPairRank, 2);
    }

    private void SearchThreePairWithOneWild(int firstPairRank, int secondPairRank, int thirdPairRank, int indexUseWild)
    {
        List<List<SimpleCard>> firstPair = SearchPairWithNoOrOneWild(firstPairRank, indexUseWild == 0);
        List<List<SimpleCard>> secondPair = SearchPairWithNoOrOneWild(secondPairRank, indexUseWild == 1);
        List<List<SimpleCard>> thirdPair = SearchPairWithNoOrOneWild(thirdPairRank, indexUseWild == 2);

        var threePairs = HandUtil.MergeTuples(firstPair, secondPair);
        threePairs = HandUtil.MergeTuples(threePairs, thirdPair);

        hints.AddRange(threePairs);
    }

    private List<List<SimpleCard>> SearchPairWithNoOrOneWild(int rank, bool useWild)
    {
        var pair = HandUtil.GetAllKTuples(cardGroupsByRank.GetValueOrDefault(rank), useWild ? 1 : 2);
        if (useWild)
        {
            foreach (List<SimpleCard> hand in pair)
            {
                hand.Add(new SimpleCard() { priority = 15, suit = 1 });
            }
        }

        return pair;
    }

    private void HintStraightWithOneWild(int rank)
    {
        for (int i = Math.Max(5, rank + 1); i <= 14; i++)
        {
            List<List<SimpleCard>> straights = StraightOfRankWithOneWild(i);
            hints.AddRange(straights);
        }
    }

    private List<List<SimpleCard>> StraightOfRankWithOneWild(int rank, bool removeFlush = true)
    {
        List<List<SimpleCard>> straights = new();
        // a straight of rank X and 1 wild card can be only presented by 01111, 10111, 11011, 11101.
        // their int format are 15, 23, 27 and 29.

        foreach (int i in new int[] { 15, 23, 27, 29 })
        {
            List<List<SimpleCard>> straight = new();
            for (int j = 0; j < 5; j++)
            {
                if ((i & (1 << j)) == 0) continue;
                if (!cardGroupsByRank.ContainsKey(rank - j))
                {
                    straight = new();
                    break;
                }

                if (straight.Count == 0)
                {
                    straight = HandUtil.GetAllKTuples(cardGroupsByRank.GetValueOrDefault(rank - j), 1);
                }
                else
                {
                    List<List<SimpleCard>> single = HandUtil.GetAllKTuples(cardGroupsByRank.GetValueOrDefault(rank - j), 1);
                    straight = HandUtil.MergeTuples(straight, single);
                }
            }
            straights.AddRange(straight);
        }

        RemoveOrRetainFlush(straights, removeFlush);
        foreach (List<SimpleCard> straight in straights)
        {
            straight.Add(new SimpleCard() { priority = 15, suit = 1 });
        }

        return straights;
    }

    private void HintBombWithOneWild(int priority)
    {
        for (int i = 3; i <= 4; i++)
        {
            for (int p = 2; p <= 15; p++)
            {
                if (18 * (i - 2) + p < priority) continue;

                List<List<SimpleCard>> bomb = HandUtil.GetAllKTuples(cardGroupsByPriority.GetValueOrDefault(p), i);
                foreach (List<SimpleCard> hand in bomb)
                {
                    hand.Add(new SimpleCard() { priority = 15, suit = 1 });
                }
                hints.AddRange(bomb);
            }
        }
        
        for (int i = 5; i <= 14; i++)
        {
            if (54 + i < priority) continue;
            List<List<SimpleCard>> flush = StraightOfRankWithOneWild(i, false);
            hints.AddRange(flush);
        }

        for (int i = 5; i <= 8; i++)
        {
            for (int p = 2; p <= 15; p++)
            {
                if (18 * (i - 1) + p < priority) continue;

                List<List<SimpleCard>> bomb = HandUtil.GetAllKTuples(cardGroupsByPriority.GetValueOrDefault(p), i);
                foreach (List<SimpleCard> hand in bomb)
                {
                    hand.Add(new SimpleCard() { priority = 15, suit = 1 });
                }
                hints.AddRange(bomb);
            }
        }
    }
}