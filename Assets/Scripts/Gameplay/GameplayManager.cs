using System;
using System.Collections;
using System.Collections.Generic;
using Singleton.Base;
using UnityEngine;

public class GameplayManager : SingletonMono<GameplayManager>
{
    private int currentAnchorCount = 0;
    private int currentStarCount = 0;
    private bool isPaused = false;
    public bool IsPaused => isPaused;
    public void Init(int anchorCount)
    {
        currentAnchorCount = anchorCount;
        GameplayUIManager.Instance.UpdateAnchorUI(currentAnchorCount);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
            {
                GameplayUIManager.Instance.ShowSettingUI();
            }
            else
            {
                GameplayUIManager.Instance.HideSettingUI();
            }
            
            isPaused = !isPaused;
        }
    }

    public void AddStar()
    {
        currentStarCount++;
    }

    public void WinThisLevel()
    {
        
    }

    public void Pause()
    {
        isPaused = true;
    }

    public void Resume()
    {
        isPaused = false;
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
