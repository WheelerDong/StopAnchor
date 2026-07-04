using System;
using System.Collections;
using System.Collections.Generic;
using Singleton.Base;
using UnityEngine;

public class GameplayManager : SingletonMono<GameplayManager>
{
    private int currentAnchorCount = 0;
    private int currentStarCount = 0;
    public void Init(int anchorCount)
    {
        currentAnchorCount = anchorCount;
        GameplayUIManager.Instance.UpdateAnchorUI(currentAnchorCount);
    }

    public void AddStar()
    {
        currentStarCount++;
    }

    public bool TryUseAnchor()
    {
        if (currentAnchorCount <= 0)
            return false;
        currentAnchorCount--;
        GameplayUIManager.Instance.UpdateAnchorUI(currentAnchorCount);
        return true;
    }

    public void GiveAnchorBack()
    {
        currentAnchorCount++;
        GameplayUIManager.Instance.UpdateAnchorUI(currentAnchorCount);
    }
}
