

using System;

class GameRankManager
{
    private static GameRankManager instance;
    public static GameRankManager Instance
    {
        get
        {
            instance ??= new GameRankManager();
            return instance;
        }
    }

    private int Rank;

    public void SetGameRank(int rank)
    {
        Rank = rank;
    }

    public int GetGameRank()
    {
        return Rank;
    }
}