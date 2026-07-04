using System.Collections;
using System.Collections.Generic;
using Singleton.Base;
using UnityEngine;

public class GameplayUIManager : SingletonMono<GameplayUIManager>
{
    [SerializeField] private UI_AnchorManager anchorManager;
    
    public void UpdateAnchorUI(int currentAnchorCount)
    {
        anchorManager.UpdateUI(currentAnchorCount);
    }
}
