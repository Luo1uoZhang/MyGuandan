using System;
using System.Collections.Generic;
public class CardShuffler
{
    public static void Shuffle(List<Card> cards)
    {
        Random random = new();
        int n = cards.Count;

        for (int i = n - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            (cards[j], cards[i]) = (cards[i], cards[j]);
        }
    }
}