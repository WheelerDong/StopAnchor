using System.Collections;
using System.Collections.Generic;
using Singleton.Base;
using UnityEngine;

public class GameplayUIManager : SingletonMono<GameplayUIManager>
{
    [SerializeField] private UI_AnchorManager anchorManager;
    [SerializeField] private UI_Setting setting;
    [SerializeField] private UI_WinPage winPage;
    [SerializeField] private UI_FailedPage failedPage;
    [SerializeField] private UI_TimeLimit timeLimit;
    

    public void Init()
    {
        HideSettingUI();
        HideWinPage();
        HideFailedPage();
    }

    public void UpdateTimeLimit(float time) => timeLimit.UpdateTimeText(time);
    public void UpdateAnchorUI(int currentAnchorCount)
    {
        anchorManager.UpdateUI(currentAnchorCount);
    }
    
    public void ShowFailedPage()
    {
        failedPage.gameObject.SetActive(true);
    }

    public void HideFailedPage()
    {
        failedPage.gameObject.SetActive(false);
    }

    public void ShowSettingUI()
    {
        setting.gameObject.SetActive(true);
    }

    public void HideSettingUI()
    {
        setting.gameObject.SetActive(false);
    }


    public void ShowWinPage(int starCount)
    {
        //winPage.gameObject.SetActive(true);
        winPage.Show(starCount);
    }
    public void HideWinPage()
    {
        winPage.gameObject.SetActive(false);
    }
}
