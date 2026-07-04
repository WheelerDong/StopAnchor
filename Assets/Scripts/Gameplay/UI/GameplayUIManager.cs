using System.Collections;
using System.Collections.Generic;
using Singleton.Base;
using UnityEngine;

public class GameplayUIManager : SingletonMono<GameplayUIManager>
{
    [SerializeField] private UI_AnchorManager anchorManager;
    [SerializeField] private UI_Setting setting;
    
    public void UpdateAnchorUI(int currentAnchorCount)
    {
        anchorManager.UpdateUI(currentAnchorCount);
    }

    public void ShowSettingUI()
    {
        setting.gameObject.SetActive(true);
    }

    public void HideSettingUI()
    {
        setting.gameObject.SetActive(false);
    }
}
