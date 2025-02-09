

static class HandComparator
{
    public static bool IsGreater(HandInfo hand, HandInfo other)
    {
        if (other.Type == HandType.NONE && hand.Type!= HandType.NONE)
            return true;
        
        if (hand.Type == HandType.BOMB)
            return hand.Rank > other.Rank;
        
        if (hand.Type != other.Type)
            return false;
        
        return hand.Rank > other.Rank;
    }
}