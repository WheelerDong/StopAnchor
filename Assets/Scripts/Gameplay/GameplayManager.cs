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
    private LevelMono levelMono;
    public bool IsPaused => isPaused;
    public void Init(LevelMono levelMono)
    {
        this.levelMono = levelMono;

        currentAnchorCount = levelMono.anchorCount;
        currentStarCount = 0;
        isPaused = false;

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
        GameplayUIManager.Instance.ShowWinPage();
        Pause();
    }

    public void NextLevel()
    {
        
    }
    
    public void RestartLevel()
    {
        isPaused = false;

        if (GameplayUIManager.Instance != null)
        {
            GameplayUIManager.Instance.HideSettingUI();
        }

        if (GameFlowManager.Instance == null)
        {
            Debug.LogError("[GameplayManager] 找不到 GameFlowManager，无法重启关卡。");
            return;
        }

        GameFlowManager.Instance.RestartCurrentLevel(levelMono);

        levelMono = null;
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
