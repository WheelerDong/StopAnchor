using UnityEngine;

public class UI_LevelManager : MonoBehaviour
{
    public void InitLevelButtons()
    {
        UI_LevelElement[] levelElements = GetComponentsInChildren<UI_LevelElement>(true);

        for (int i = 0; i < levelElements.Length; i++)
        {
            levelElements[i].levelIndex = i;
        }
    }
}