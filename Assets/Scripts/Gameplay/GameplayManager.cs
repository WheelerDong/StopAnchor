using System;
using System.Collections;
using System.Collections.Generic;
using Singleton.Base;
using UnityEngine;

public class GameplayManager : SingletonMono<GameplayManager>
{
    private int currentAnchorCount = 0;
    public void Init(int anchorCount)
    {
        currentAnchorCount = anchorCount;
        GameplayUIManager.Instance.UpdateAnchorUI(currentAnchorCount);
    }
    
    
}
