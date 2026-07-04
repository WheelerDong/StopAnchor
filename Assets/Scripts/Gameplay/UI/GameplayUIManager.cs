using System.Collections;
using System.Collections.Generic;
using Singleton.Base;
using UnityEngine;

public class GameplayUIManager : SingletonMono<GameplayUIManager>
{
    [SerializeField] private UI_AnchorManager anchorManager;
    [SerializeField] private UI_Setting setting;
    [SerializeField] private UI_WinPage winPage;

    public void Init()
    {
        HideSettingUI();
        HideWinPage();
    }
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


    public void ShowWinPage()
    {
        winPage.gameObject.SetActive(true);
    }
    public void HideWinPage()
    {
        winPage.gameObject.SetActive(false);
    }
}
