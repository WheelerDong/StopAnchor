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

    protected override bool IsPersistent { get; } = true;

    private int currentLevel = 0;
    private bool isLoadingLevel = false;

    public void PlayCurrentLevel()
    {
        PlayLevel(currentLevel);
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
        if (levelLibrary == null)
        {
            Debug.LogError("[GameFlowManager] LevelLibrary 没有赋值。");
            yield break;
        }

        if (levelLibrary.levels == null || levelLibrary.levels.Count == 0)
        {
            Debug.LogError("[GameFlowManager] LevelLibrary 中没有配置任何关卡。");
            yield break;
        }

        if (levelIndex < 0 || levelIndex >= levelLibrary.levels.Count)
        {
            Debug.LogError($"[GameFlowManager] 关卡索引越界：{levelIndex}");
            yield break;
        }

        LevelMono levelPrefab = levelLibrary.levels[levelIndex];

        if (levelPrefab == null)
        {
            Debug.LogError($"[GameFlowManager] LevelLibrary 中第 {levelIndex} 个关卡为空。");
            yield break;
        }

        isLoadingLevel = true;

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

        LevelMono levelInstance = Instantiate(levelPrefab, Vector3.zero, Quaternion.identity);
        Debug.Log("GameFlowManager is initializing level");
        levelInstance.Init();

        isLoadingLevel = false;
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
