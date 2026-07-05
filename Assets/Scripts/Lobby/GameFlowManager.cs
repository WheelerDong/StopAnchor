using System.Collections;
using Singleton.Base;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowManager : SingletonMono<GameFlowManager>
{
    [Header("Level")]
    [SerializeField] private LevelLibrary levelLibrary;

    [Header("Scene")]
    [SerializeField] private string gameplaySceneName = "Gameplay";
    [SerializeField] private string lobbySceneName = "Lobby";

    public LevelLibrary LevelLibrary => levelLibrary;
    protected override bool IsPersistent { get; } = true;

    private int currentLevel = 0;
    private bool isLoadingLevel = false;

    private LevelMono currentLevelInstance;

    public int CurrentLevelIndex => currentLevel;

    public void PlayCurrentLevel()
    {
        PlayLevel(currentLevel);
    }
    
    public void PlayNextLevel()
    {
        if (isLoadingLevel)
        {
            return;
        }

        int nextLevelIndex = currentLevel + 1;

        if (!IsValidLevelIndex(nextLevelIndex))
        {
            Debug.Log("[GameFlowManager] 已经是最后一关，返回大厅。");
            GoIntoLobby();
            return;
        }

        PlayLevel(nextLevelIndex);
    }

    public void PlayLevel(int levelIndex)
    {
        if (isLoadingLevel)
        {
            return;
        }

        StartCoroutine(LoadGameplayAndSpawnLevel(levelIndex));
    }

    private IEnumerator LoadGameplayAndSpawnLevel(int levelIndex)
    {
        if (!IsValidLevelIndex(levelIndex))
        {
            yield break;
        }

        isLoadingLevel = true;
        currentLevel = levelIndex;

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(gameplaySceneName, LoadSceneMode.Single);

        if (loadOperation == null)
        {
            Debug.LogError($"[GameFlowManager] 场景加载失败，请检查场景名是否正确：{gameplaySceneName}");
            isLoadingLevel = false;
            yield break;
        }

        while (!loadOperation.isDone)
        {
            yield return null;
        }

        SpawnThisLevel(currentLevel);

        isLoadingLevel = false;
    }

    public void RestartCurrentLevel(LevelMono levelToDestroy)
    {
        if (isLoadingLevel)
        {
            return;
        }

        StartCoroutine(RestartCurrentLevelCoroutine(levelToDestroy));
    }

    private IEnumerator RestartCurrentLevelCoroutine(LevelMono levelToDestroy)
    {
        if (!IsValidLevelIndex(currentLevel))
        {
            yield break;
        }

        isLoadingLevel = true;

        if (levelToDestroy != null)
        {
            Destroy(levelToDestroy.gameObject);
        }
        else if (currentLevelInstance != null)
        {
            Destroy(currentLevelInstance.gameObject);
        }

        currentLevelInstance = null;

        // Destroy 会在当前帧结束后真正执行，所以等一帧，避免新旧 Level / Player 同时存在
        yield return null;

        SpawnThisLevel(currentLevel);

        isLoadingLevel = false;
    }

    private void SpawnThisLevel(int levelIndex)
    {
        LevelMono levelPrefab = levelLibrary.levels[levelIndex];

        if (levelPrefab == null)
        {
            Debug.LogError($"[GameFlowManager] LevelLibrary 中第 {levelIndex} 个关卡为空。");
            return;
        }

        currentLevelInstance = Instantiate(levelPrefab, Vector3.zero, Quaternion.identity);

        Debug.Log("GameFlowManager is initializing level");

        currentLevelInstance.Init();
    }

    private bool IsValidLevelIndex(int levelIndex)
    {
        if (levelLibrary == null)
        {
            Debug.LogError("[GameFlowManager] LevelLibrary 没有赋值。");
            return false;
        }

        if (levelLibrary.levels == null || levelLibrary.levels.Count == 0)
        {
            Debug.LogError("[GameFlowManager] LevelLibrary 中没有配置任何关卡。");
            return false;
        }

        if (levelIndex < 0 || levelIndex >= levelLibrary.levels.Count)
        {
            Debug.LogError($"[GameFlowManager] 关卡索引越界：{levelIndex}");
            return false;
        }

        return true;
    }

    public void GoIntoLobby()
    {
        SceneManager.LoadScene(lobbySceneName, LoadSceneMode.Single);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}