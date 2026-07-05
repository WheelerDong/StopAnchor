using System.Collections.Generic;
using UnityEngine;

public static class GameSaveManager
{
    private const string SaveKey = "AGameName.SaveData";

    private static GameSaveData cachedData;

    public static GameSaveData Data
    {
        get
        {
            if (cachedData == null)
            {
                cachedData = Load();
            }

            return cachedData;
        }
    }

    public static void InitializeLevelProgress(int levelCount)
    {
        if (levelCount <= 0)
        {
            return;
        }

        GameSaveData data = Data;
        EnsureLevelProgress(data, levelCount);
        data.currentLevelIndex = ClampLevelIndex(data.currentLevelIndex, levelCount);
        Save(data);
    }

    public static int GetCurrentLevelIndex(int levelCount)
    {
        if (levelCount <= 0)
        {
            return 0;
        }

        InitializeLevelProgress(levelCount);
        return ClampLevelIndex(Data.currentLevelIndex, levelCount);
    }

    public static int GetBestStarCount(int levelIndex)
    {
        LevelSaveData levelData = FindLevelData(Data, levelIndex);
        return levelData != null ? levelData.bestStarCount : 0;
    }

    public static bool IsLevelCompleted(int levelIndex)
    {
        LevelSaveData levelData = FindLevelData(Data, levelIndex);
        return levelData != null && levelData.isCompleted;
    }

    public static void SetCurrentLevel(int levelIndex, int levelCount)
    {
        if (levelCount <= 0)
        {
            return;
        }

        GameSaveData data = Data;
        EnsureLevelProgress(data, levelCount);
        data.currentLevelIndex = ClampLevelIndex(levelIndex, levelCount);
        Save(data);
    }

    public static void CompleteLevel(int levelIndex, int starCount, int levelCount)
    {
        if (levelCount <= 0)
        {
            return;
        }

        GameSaveData data = Data;
        EnsureLevelProgress(data, levelCount);

        int safeLevelIndex = ClampLevelIndex(levelIndex, levelCount);
        LevelSaveData levelData = GetOrCreateLevelData(data, safeLevelIndex);
        levelData.isCompleted = true;
        levelData.bestStarCount = Mathf.Max(levelData.bestStarCount, Mathf.Max(0, starCount));

        if (data.currentLevelIndex <= safeLevelIndex)
        {
            data.currentLevelIndex = FindFirstUncompletedLevel(data, levelCount, safeLevelIndex + 1);
        }

        Save(data);
    }

    public static void DeleteSave()
    {
        cachedData = new GameSaveData();
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
    }

    private static GameSaveData Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            return new GameSaveData();
        }

        string json = PlayerPrefs.GetString(SaveKey);

        if (string.IsNullOrEmpty(json))
        {
            return new GameSaveData();
        }

        try
        {
            GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
            return data ?? new GameSaveData();
        }
        catch
        {
            Debug.LogWarning("[GameSaveManager] Save data is broken. A new save will be created.");
            return new GameSaveData();
        }
    }

    private static void Save(GameSaveData data)
    {
        cachedData = data;
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    private static void EnsureLevelProgress(GameSaveData data, int levelCount)
    {
        if (data.levels == null)
        {
            data.levels = new List<LevelSaveData>();
        }

        for (int i = 0; i < levelCount; i++)
        {
            GetOrCreateLevelData(data, i);
        }

        for (int i = data.levels.Count - 1; i >= 0; i--)
        {
            if (data.levels[i] == null || data.levels[i].levelIndex < 0 || data.levels[i].levelIndex >= levelCount)
            {
                data.levels.RemoveAt(i);
            }
        }
    }

    private static LevelSaveData GetOrCreateLevelData(GameSaveData data, int levelIndex)
    {
        LevelSaveData levelData = FindLevelData(data, levelIndex);

        if (levelData != null)
        {
            return levelData;
        }

        levelData = new LevelSaveData(levelIndex);
        data.levels.Add(levelData);
        return levelData;
    }

    private static LevelSaveData FindLevelData(GameSaveData data, int levelIndex)
    {
        if (data.levels == null)
        {
            return null;
        }

        for (int i = 0; i < data.levels.Count; i++)
        {
            LevelSaveData levelData = data.levels[i];

            if (levelData != null && levelData.levelIndex == levelIndex)
            {
                return levelData;
            }
        }

        return null;
    }

    private static int FindFirstUncompletedLevel(GameSaveData data, int levelCount, int preferredStartIndex)
    {
        for (int i = Mathf.Clamp(preferredStartIndex, 0, levelCount - 1); i < levelCount; i++)
        {
            LevelSaveData levelData = GetOrCreateLevelData(data, i);

            if (!levelData.isCompleted)
            {
                return i;
            }
        }

        for (int i = 0; i < levelCount; i++)
        {
            LevelSaveData levelData = GetOrCreateLevelData(data, i);

            if (!levelData.isCompleted)
            {
                return i;
            }
        }

        return levelCount - 1;
    }

    private static int ClampLevelIndex(int levelIndex, int levelCount)
    {
        return Mathf.Clamp(levelIndex, 0, levelCount - 1);
    }
}
