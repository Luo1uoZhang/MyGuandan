using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CommonUtil
{
    public static int CountBit(int value)
    {
        int count = 0;
        while (value > 0)
        {
            value &= value - 1;
            count++;
        }
        return count;
    }

    public static IEnumerator<WaitForSeconds> DelayMakeUp(HoldArea player)
    {
        yield return new WaitForSeconds(0.3f);
        player.MakeUp();
    }
}