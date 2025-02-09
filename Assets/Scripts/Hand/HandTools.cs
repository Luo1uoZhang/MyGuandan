using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.XR;

public enum HandType
{
    SINGLE,
    PAIR,
    TRIPS,
    THREEPAIR,
    THREEWITHTWO,
    TWOTRIPS,
    STRAIGHT,
    STRAIGHTFLUSH,
    BOMB,
    NONE,
    THREEPAIRORTWOTRIPS
}

public class HandInfo
{
    public HandType Type { get; set; }
    public int Rank { get; set; }

    public HandInfo(HandType ht, int r)
    {
        Type = ht;
        Rank = r;
    }
}

static public class HandTools
{
    static public HandInfo GetHandInfo(List<Card> hand)
    {
        int countWildCards = CountWildCards(hand);
        return countWildCards switch
        {
            0 => CaseNoWildCard(hand),
            1 => CaseOneWildCard(hand),
            2 => CaseTwoWildCards(hand),
            _ => throw new Exception("Invalid number of wild cards."),
        };
    }

    private static int CountWildCards(List<Card> hand)
    {
        int count = 0;
        int gameRank = GameRankManager.Instance.GetGameRank();
        foreach (Card card in hand)
        {
            if (card.rank == gameRank && card.suit == 1)
            {
                count++;
            }
        }
        return count;
    }

    private static List<Card> RemoveWildAndSort(List<Card> hand)
    {
        List<Card> result = new();
        int gameRank = GameRankManager.Instance.GetGameRank();
        foreach (Card card in hand)
        {
            if (card.rank == gameRank && card.suit == 1)
            {
                continue;
            }
            result.Add(card);
        }
        result.Sort((a, b) =>
        {
            return a.rank.CompareTo(b.rank);
        });
        return result;
    }

    private static HandInfo CaseOneWildCard(List<Card> hand)
    {
        List<Card> newHand = RemoveWildAndSort(hand);
        int gameRank = GameRankManager.Instance.GetGameRank();

        HandType type = HandType.NONE;
        int rank = -1;
        switch (newHand.Count)
        {
            case 0:
                type = HandType.SINGLE;
                rank = 15;
                break;
            case 1:
                if (ContainJoker(newHand)) break;

                type = HandType.PAIR;
                rank = CorrectRank(newHand[0].rank, gameRank);
                break;
            case 2:
                if (ContainJoker(newHand)) break;

                if (IsPair(newHand))
                {
                    type = HandType.TRIPS;
                    rank = CorrectRank(newHand[0].rank, gameRank);
                }
                break;
            case 3:
                if (ContainJoker(newHand)) break;

                if (IsTrips(newHand))
                {
                    type = HandType.BOMB;
                    rank = CorrectRank(newHand[0].rank, gameRank) + 18;
                }
                break;
            case 4:
                Dictionary<int, List<Card>> caseFourStatistic = GetHandStatistic(newHand);
                switch (caseFourStatistic.Keys.Count)
                {
                    case 1:
                        type = HandType.BOMB;
                        rank = CorrectRank(newHand[0].rank, gameRank) + 36;
                        break;
                    case 2:
                        int tempRank = -1;
                        foreach (int key in caseFourStatistic.Keys)
                        {
                            if (caseFourStatistic[key].Count == 1)
                            {
                                if (caseFourStatistic[key][0].rank >= 16) break;
                                tempRank = CorrectRank(caseFourStatistic[key][0].rank, gameRank);
                            }
                            if (caseFourStatistic[key].Count == 2)
                            {
                                if (caseFourStatistic[key][0].rank >= 16) break;
                                tempRank = Math.Max(tempRank, CorrectRank(caseFourStatistic[key][0].rank, gameRank));
                            }
                        }
                        if (tempRank == -1) break;
                        type = HandType.THREEWITHTWO;
                        rank = tempRank;
                        break;
                    case 4:
                        if (ContainJoker(newHand)) break;
                        List<int> ranks = caseFourStatistic.Keys.ToList();
                        ranks.Sort();
                        int diff;
                        if (ranks[^1] == 14)
                        {
                            diff = Math.Min(ranks[^2] - 1, 14 - ranks[0]);
                        }
                        else
                        {
                            diff = ranks[^1] - ranks[0];
                        }

                        if (diff <= 4)
                        {
                            type = HandType.STRAIGHT;
                            if (diff == 4)
                            {
                                if (ranks[^1] == 14)
                                {
                                    rank = ranks[0] < 10 ? 5 : 14;
                                }
                                else
                                {
                                    rank = ranks[^1];
                                }
                            }
                            else
                            {
                                if (ranks[^1] == 14)
                                {
                                    rank = ranks[0] < 10 ? 5 : 14;
                                }
                                else
                                {
                                    rank = ranks[^1] + 1;
                                }
                            }

                            if (IsFlush(newHand))
                            {
                                type = HandType.BOMB;
                                rank += 54;
                            }
                        }

                        break;
                    default:
                        break;
                }
                break;
            case 5:
                Dictionary<int, List<Card>> caseFiveStatistic = GetHandStatistic(newHand);
                switch (caseFiveStatistic.Keys.Count)
                {
                    case 1:
                        type = HandType.BOMB;
                        rank = CorrectRank(newHand[0].rank, gameRank) + 72;
                        break;
                    case 2:
                        if (ContainJoker(newHand)) break;
                        List<int> ranks = caseFiveStatistic.Keys.ToList();
                        ranks.Sort();
                        int diff;
                        if (ranks[1] == 14)
                        {
                            if (ranks[0] == 2 || ranks[0] == 13)
                            {
                                diff = 1;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            diff = ranks[1] - ranks[0];
                        }

                        if (diff != 1) break;

                        foreach (var pair in caseFiveStatistic.Values)
                        {
                            if (pair.Count == 2)
                            {
                                type = HandType.TWOTRIPS;
                                rank = ranks[1] == 14 ? ranks[0] == 2 ? 2 : 14 : ranks[1];
                            }
                        }

                        break;
                    case 3:
                        if (ContainJoker(newHand)) break;
                        List<int> caseFiveThreePairRanks = caseFiveStatistic.Keys.ToList();
                        caseFiveThreePairRanks.Sort();
                        int tempDiff;
                        if (caseFiveThreePairRanks[2] == 14)
                        {
                            tempDiff = Math.Min(14 - caseFiveThreePairRanks[0], caseFiveThreePairRanks[1] - 1);
                        }
                        else
                        {
                            tempDiff = caseFiveThreePairRanks[2] - caseFiveThreePairRanks[0];
                        }
                        if (tempDiff != 2) break;

                        foreach (var pair in caseFiveStatistic.Values)
                        {
                            if (pair.Count == 1)
                            {
                                type = HandType.THREEPAIR;
                                rank = caseFiveThreePairRanks[2] == 14 ? caseFiveThreePairRanks[0] == 2 ? 3 : 14 : caseFiveThreePairRanks[2];
                            }
                        }

                        foreach (var pair in caseFiveStatistic.Values)
                        {
                            if (pair.Count == 3)
                            {
                                type = HandType.NONE;
                                rank = -1;
                            }
                        }

                        break;
                    default:
                        break;
                }
                break;
            default:
                if (IsBomb(newHand))
                {
                    type = HandType.BOMB;
                    rank = CorrectRank(newHand[0].rank, gameRank) + 18 * (newHand.Count - 1);
                }
                break;
        }
        return new HandInfo(type, rank);
    }

    private static Dictionary<int, List<Card>> GetHandStatistic(List<Card> newHand)
    {
        Dictionary<int, List<Card>> statistic = new();
        foreach (Card card in newHand)
        {
            if (!statistic.ContainsKey(card.rank))
            {
                statistic.Add(card.rank, new List<Card> { card });
            }
            else
            {
                statistic[card.rank].Add(card);
            }
        }

        return statistic;
    }

    private static HandInfo CaseTwoWildCards(List<Card> hand)
    {
        List<Card> newHand = RemoveWildAndSort(hand);
        int gameRank = GameRankManager.Instance.GetGameRank();
        Dictionary<int, List<Card>> statistic = GetHandStatistic(newHand);

        HandType type = HandType.NONE;
        int rank = -1;
        switch (newHand.Count)
        {
            case 0:
                type = HandType.PAIR;
                rank = 15;
                break;
            case 1:
                if (ContainJoker(newHand)) break;

                type = HandType.TRIPS;
                rank = CorrectRank(newHand[0].rank, gameRank);
                break;
            case 2:
                if (ContainJoker(newHand)) break;

                if (IsPair(newHand))
                {
                    type = HandType.BOMB;
                    rank = CorrectRank(newHand[0].rank, gameRank) + 18;
                }
                break;
            case 3:
                switch (statistic.Keys.Count)
                {
                    case 1:
                        type = HandType.BOMB;
                        rank = CorrectRank(newHand[0].rank, gameRank) + 36;
                        break;
                    case 2:
                        int tempRank = -1;
                        foreach (var value in statistic.Values)
                        {
                            if (value[0].rank <= 15)
                            {
                                tempRank = Math.Max(tempRank, CorrectRank(value[0].rank, gameRank));
                            }
                        }
                        foreach (var value in statistic.Values)
                        {
                            if (value.Count == 1 && value[0].rank >= 16)
                            {
                                tempRank = -1;
                            }
                        }
                        if (tempRank != -1)
                        {
                            type = HandType.THREEWITHTWO;
                        }
                        break;
                    case 3:
                        List<int> keys = statistic.Keys.ToList();
                        keys.Sort();
                        int tempDiff;
                        tempDiff = keys[2] == 14 ? Math.Min(14 - keys[0], keys[1] - 1) : keys[2] - keys[0];
                        if (tempDiff > 4)
                        {
                            break;
                        }
                        type = HandType.STRAIGHT;
                        if (IsFlush(newHand)) type = HandType.BOMB;

                        // 不懂，编辑器教我这么改的
                        rank = keys[2] == 14
                            ? keys[0] < 10 ? 5 : 14
                            : tempDiff switch
                            {
                                4 => keys[2],
                                3 => keys[2] + 1,
                                _ => Math.Min(14, keys[2] + 2),
                            };

                        if (type == HandType.BOMB) rank += 54;
                        break;
                }
                break;
            case 4:
                switch (statistic.Keys.Count)
                {
                    case 1:
                        if (ContainJoker(newHand)) break;
                        type = HandType.BOMB;
                        rank = CorrectRank(newHand[0].rank, gameRank) + 72;
                        break;
                    case 2:
                        if (ContainJoker(newHand)) break;
                        List<int> keys = statistic.Keys.ToList();
                        keys.Sort();
                        if (keys[1] == 14)
                        {
                            if (keys[0] <= 3 || keys[0] >= 12)
                            {
                                int diff = Math.Min(14 - keys[0], keys[0] - 1);
                                if (diff == 2)
                                {
                                    if (statistic[keys[0]].Count != 2) break;

                                    type = HandType.THREEPAIR;
                                    rank = keys[0] <= 3 ? 3 : 14;
                                }
                                else if (diff == 1)
                                {
                                    if (statistic[keys[0]].Count != 2)
                                    {
                                        type = HandType.TWOTRIPS;
                                        rank = keys[0] == 2 ? 2 : 14;
                                    }
                                    else
                                    {
                                        type = Board.Instance.GetLastHandInfo().Type switch
                                    {
                                        HandType.THREEPAIR => HandType.THREEPAIR,
                                        HandType.TWOTRIPS => HandType.TWOTRIPS,
                                        _ => HandType.THREEPAIRORTWOTRIPS,
                                    };
                                        rank = keys[0] == 2 ? 2 : 14;
                                    }
                                }
                            }
                        }
                        else
                        {
                            int diff = keys[1] - keys[0];
                            if (diff == 2)
                            {
                                if (statistic[keys[0]].Count != 2) break;

                                type = HandType.THREEPAIR;
                                rank = keys[1];
                            }
                            else if (diff == 1)
                            {
                                if (statistic[keys[0]].Count != 2)
                                {
                                    type = HandType.TWOTRIPS;
                                    rank = keys[1];
                                }
                                else
                                {
                                    type = Board.Instance.GetLastHandInfo().Type switch
                                    {
                                        HandType.THREEPAIR => HandType.THREEPAIR,
                                        HandType.TWOTRIPS => HandType.TWOTRIPS,
                                        _ => HandType.THREEPAIRORTWOTRIPS,
                                    };
                                    rank = keys[1];
                                }
                            }
                        }
                        break;
                    case 3:
                        if (ContainJoker(newHand)) break;
                        List<int> caseThreeKeys = statistic.Keys.ToList();
                        caseThreeKeys.Sort();
                        int tempDiff;
                        if (caseThreeKeys[2] == 14)
                        {
                            tempDiff = Math.Min(14 - caseThreeKeys[0], caseThreeKeys[1] - 1);
                            if (tempDiff != 2) break;

                            type = HandType.THREEPAIR;
                            rank = caseThreeKeys[0] <= 3 ? 3 : 14;
                        }
                        else
                        {
                            tempDiff = caseThreeKeys[2] - caseThreeKeys[0];
                            if (tempDiff != 2) break;

                            type = HandType.THREEPAIR;
                            rank = caseThreeKeys[2];
                        }
                        
                        foreach (var value in statistic.Values)
                        {
                            if (value.Count == 3)
                            {
                                type = HandType.NONE;
                                rank = -1;
                            }
                        }
                        break;
                    default:
                        break;
                }
                break;
            case int n when n > 4:
                if (IsBomb(newHand))
                {
                    type = HandType.BOMB;
                    rank = CorrectRank(newHand[0].rank, gameRank) + n * 18;
                }
                break;
        }
        return new HandInfo(type, rank);
    }

    private static HandInfo CaseNoWildCard(List<Card> hand)
    {
        hand.Sort((a, b) =>
        {
            return a.rank.CompareTo(b.rank);
        });
        HandType type = HandType.NONE;
        int rank = -1;
        int gameRank = GameRankManager.Instance.GetGameRank();
        switch (hand.Count)
        {
            case 0:
                break;
            case 1:
                type = HandType.SINGLE;
                rank = CorrectRank(hand[0].rank, gameRank);
                break;
            case 2:
                if (!IsPair(hand))
                {
                    break;
                }
                type = HandType.PAIR;
                rank = CorrectRank(hand[0].rank, gameRank);
                break;
            case 3:
                if (!IsTrips(hand))
                {
                    break;
                }
                type = HandType.TRIPS;
                rank = CorrectRank(hand[0].rank, gameRank);
                break;
            case 4:
                if (IsBomb(hand))
                {
                    type = HandType.BOMB;
                    rank = CorrectRank(hand[0].rank, gameRank) + 18;
                }
                if (IsFourJokers(hand))
                {
                    type = HandType.BOMB;
                    rank = 999;
                }
                break;
            case 5:
                if (IsStraight(hand))
                {
                    if (hand[4].rank == 14)
                    {
                        if (hand[0].rank == 2)
                        {
                            rank = 5;
                        }
                        else
                        {
                            rank = 14;
                        }
                    }
                    else
                    {
                        rank = hand[4].rank;
                    }

                    if (IsFlush(hand))
                    {
                        type = HandType.BOMB;
                        rank += 54;
                    }
                    else
                    {
                        type = HandType.STRAIGHT;
                    }
                }
                if (IsThreeWithTwo(hand))
                {
                    rank = CorrectRank(hand[2].rank, gameRank); ;
                    type = HandType.THREEWITHTWO;
                }
                if (IsBomb(hand))
                {
                    type = HandType.BOMB;
                    rank = CorrectRank(hand[0].rank, gameRank) + 36;
                }
                break;
            case 6:
                if (IsThreePair(hand))
                {
                    rank = hand[5].rank;
                    type = HandType.THREEPAIR;
                }
                if (IsTwoTrips(hand))
                {
                    rank = hand[5].rank;
                    type = HandType.TWOTRIPS;
                }
                if (IsBomb(hand))
                {
                    type = HandType.BOMB;
                    rank = CorrectRank(hand[0].rank, gameRank) + 72;
                }
                break;
            default:
                if (IsBomb(hand))
                {
                    type = HandType.BOMB;
                    rank = CorrectRank(hand[0].rank, gameRank) + 18 * (hand.Count - 2);
                }
                break;
        }
        return new HandInfo(type, rank);
    }

    static public bool IsPair(List<Card> hand)
    {
        return hand[0].rank == hand[1].rank;
    }

    static public bool IsTrips(List<Card> hand)
    {
        return hand[0].rank == hand[1].rank && hand[0].rank == hand[2].rank;
    }

    static public bool IsBomb(List<Card> hand)
    {
        for (int i = 1; i < hand.Count; i++)
        {
            if (hand[i].rank != hand[i - 1].rank)
                return false;
        }
        return true;
    }

    static public bool IsFourJokers(List<Card> hand)
    {
        foreach (Card card in hand)
        {
            if (card.rank != 15 && card.rank != 16)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Cards should be sorted by rank first.
    /// </summary>
    /// <param name="hand"></param>
    /// <returns></returns>
    static public bool IsStraight(List<Card> hand)
    {
        for (int i = 1; i < hand.Count; i++)
        {
            if (hand[i].rank == 15 || hand[i].rank == 16)
                return false;

            // If this card is Ace, it can only be the last card.
            // Then, because hand[i - 1] == hand[i - 2],
            // only hand[i - 1].rank == 13 or hand[0].rank == 2
            if (hand[i].rank == 14)
            {
                if (i != hand.Count - 1)
                {
                    return false;
                }
                else
                {
                    if (hand[i - 1].rank == 13 || hand[0].rank == 2)
                    {
                        return true;
                    }
                    return false;
                }
            }

            if (hand[i].rank - hand[i - 1].rank != 1)
                return false;
        }
        return true;
    }

    // This method only checks if all cards are the same suit.
    // To check whether a hand is a straight flush, we need to check if it is a straight first and then a flush.
    static public bool IsFlush(List<Card> hand)
    {
        for (int i = 1; i < hand.Count; i++)
        {
            if (hand[i].suit != hand[i - 1].suit)
                return false;
        }
        return true;
    }

    static public bool IsThreeWithTwo(List<Card> hand)
    {
        Dictionary<int, int> count = new();

        foreach (Card card in hand)
        {
            if (!count.ContainsKey(card.rank))
            {
                count.Add(card.rank, 1);
            }
            else
            {
                count[card.rank]++;
            }
        }
        foreach (int n in count.Values)
        {
            if (n != 3 && n != 2)
                return false;
        }
        return true;
    }


    // TODO: Ace Special.
    static public bool IsThreePair(List<Card> hand)
    {
        for (int i = 1; i < hand.Count; i++)
        {
            if ((i & 1) == 1)
            {
                if (hand[i].rank != hand[i - 1].rank)
                    return false;
            }
            else
            {
                if (hand[i].rank != hand[i - 2].rank + 1)
                    return false;
            }
        }
        return true;
    }

    // TODO: Ace Special.
    static public bool IsTwoTrips(List<Card> hand)
    {
        for (int i = 1; i < 3; i++)
        {
            if (hand[i].rank != hand[i - 1].rank)
                return false;
        }
        for (int i = 4; i < 6; i++)
        {
            if (hand[i].rank != hand[i - 1].rank)
                return false;
        }
        if (hand[3].rank != hand[0].rank + 1)
            return false;
        return true;
    }

    static public int CorrectRank(int rank, int gameRank)
    {
        if (rank == gameRank)
        {
            return 15;
        }
        return rank;
    }

    static private bool ContainJoker(List<Card> hand)
    {
        foreach (Card card in hand)
        {
            if (card.rank == 16 || card.rank == 17)
            {
                return true;
            }
        }
        return false;
    }
}