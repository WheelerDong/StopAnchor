using System;
using System.Collections.Generic;

[Serializable]
public class GameSaveData
{
    public int version = 1;
    public int currentLevelIndex = 0;
    public List<LevelSaveData> levels = new List<LevelSaveData>();
}

[Serializable]
public class LevelSaveData
{
    public int levelIndex;
    public int bestStarCount;
    public bool isCompleted;

    public LevelSaveData(int levelIndex)
    {
        this.levelIndex = levelIndex;
        bestStarCount = 0;
        isCompleted = false;
    }
}
