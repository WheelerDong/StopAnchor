using System;
using System.Collections;
using System.Collections.Generic;
using Singleton.Base;
using UnityEngine;

public class GameplayManager : SingletonMono<GameplayManager>
{
    private int currentAnchorCount = 0;

    // 当前剩余时间。-1 表示无限时间
    private float timeLimit = -1f;

    private int currentStarCount = 0;
    private bool isPaused = false;
    private bool isGameEnded = false;

    private LevelMono levelMono;

    // 用于避免每帧重复刷新 UI
    private int lastDisplayedTime = int.MinValue;

    public bool IsPaused => isPaused;
    public float TimeLimit => timeLimit;
    public bool HasTimeLimit => timeLimit >= 0f;

    public void Init(LevelMono levelMono)
    {
        this.levelMono = levelMono;

        currentAnchorCount = levelMono.anchorCount;
        timeLimit = levelMono.timeLimit;
        currentStarCount = 0;

        isPaused = false;
        isGameEnded = false;
        lastDisplayedTime = int.MinValue;

        if (GameplayUIManager.Instance != null)
        {
            GameplayUIManager.Instance.Init();
            GameplayUIManager.Instance.UpdateAnchorUI(currentAnchorCount);
        }

        UpdateTimeLimitUI(true);
    }

    private void Update()
    {
        HandlePauseInput();
        UpdateTimeLimit();
    }

    private void HandlePauseInput()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
        {
            return;
        }

        if (isGameEnded)
        {
            return;
        }

        if (!isPaused)
        {
            GameplayUIManager.Instance.ShowSettingUI();
            Pause();
        }
        else
        {
            GameplayUIManager.Instance.HideSettingUI();
            Resume();
        }
    }

    private void UpdateTimeLimit()
    {
        if (isPaused)
        {
            return;
        }

        if (isGameEnded)
        {
            return;
        }

        // -1 表示无限时间，不倒计时
        if (timeLimit < 0f)
        {
            return;
        }

        timeLimit -= Time.deltaTime;

        if (timeLimit <= 0f)
        {
            timeLimit = 0f;
            UpdateTimeLimitUI(true);
            TimeOutThisLevel();
            return;
        }

        UpdateTimeLimitUI(false);
    }

    private void UpdateTimeLimitUI(bool forceUpdate)
    {
        if (GameplayUIManager.Instance == null)
        {
            return;
        }

        // 无限时间
        if (timeLimit < 0f)
        {
            GameplayUIManager.Instance.UpdateTimeLimit(-1f);
            return;
        }

        // 用 CeilToInt 更适合倒计时显示
        int displayTime = Mathf.CeilToInt(timeLimit);

        if (!forceUpdate && displayTime == lastDisplayedTime)
        {
            return;
        }

        lastDisplayedTime = displayTime;
        GameplayUIManager.Instance.UpdateTimeLimit(displayTime);
    }

    private void TimeOutThisLevel()
    {
        if (isGameEnded)
        {
            return;
        }

        isGameEnded = true;

        Debug.Log("[GameplayManager] 时间耗尽。");

        // 这里先暂停游戏。
        // 如果你之后有失败界面，可以在这里调用：
        // GameplayUIManager.Instance.ShowLosePage();
        GameplayUIManager.Instance.ShowFailedPage();

        Pause();
    }

    public void AddStar()
    {
        if (isGameEnded)
        {
            return;
        }

        currentStarCount++;
    }

    public void WinThisLevel()
    {
        if (isGameEnded)
        {
            return;
        }

        isGameEnded = true;

        GameplayUIManager.Instance.ShowWinPage();
        Pause();
    }

    public void FailThisLevel()
    {
        
    }

    public void NextLevel()
    {
        isPaused = false;
        isGameEnded = false;

        if (GameplayUIManager.Instance != null)
        {
            GameplayUIManager.Instance.HideSettingUI();
        }

        if (GameFlowManager.Instance == null)
        {
            Debug.LogError("[GameplayManager] 找不到 GameFlowManager，无法进入下一关。");
            return;
        }

        GameFlowManager.Instance.PlayNextLevel();

        levelMono = null;
    }

    public void RestartLevel()
    {
        isPaused = false;
        isGameEnded = false;

        if (GameplayUIManager.Instance != null)
        {
            GameplayUIManager.Instance.Init();
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
        if (isGameEnded)
        {
            return;
        }

        isPaused = false;
    }

    public bool TryUseAnchor()
    {
        if (isGameEnded)
        {
            return false;
        }

        if (currentAnchorCount <= 0)
        {
            return false;
        }

        currentAnchorCount--;

        if (GameplayUIManager.Instance != null)
        {
            GameplayUIManager.Instance.UpdateAnchorUI(currentAnchorCount);
        }

        return true;
    }

    public void GiveAnchorBack()
    {
        if (isGameEnded)
        {
            return;
        }

        currentAnchorCount++;

        if (GameplayUIManager.Instance != null)
        {
            GameplayUIManager.Instance.UpdateAnchorUI(currentAnchorCount);
        }
    }
}