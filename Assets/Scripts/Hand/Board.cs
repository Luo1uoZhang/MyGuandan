using UnityEngine.XR;
using UnityEngine;

class Board {
    private static Board instance;
    private int NumOfPasses = 0;
    private HandInfo lastHandInfo;

    public static Board Instance {
        get {
            instance ??= new Board();
            return instance;
        }
    }

    private Board()
    {
        lastHandInfo = new HandInfo(HandType.NONE, -1);
    }
    
    public HandInfo GetLastHandInfo() {
        return lastHandInfo;
    }
    
    public void UpdateHandInfo(HandInfo handInfo) {
        if (handInfo.Type == HandType.NONE)
        {
            NumOfPasses++;
        }
        else
        {
            lastHandInfo = handInfo;
            NumOfPasses = 0;
        }
        Debug.Log("There are " + NumOfPasses + " passes now.");
    }

    public void Reset()
    {
        lastHandInfo = new HandInfo(HandType.NONE, -1);
        NumOfPasses = 0;
    }

    public int GetNumOfPasses()
    {
        return NumOfPasses;
    }
}