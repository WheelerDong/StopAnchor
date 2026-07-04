using System.Collections;
using System.Collections.Generic;
using Singleton.Base;
using UnityEngine;
using UnityEngine.Serialization;

public class GameUIManager : SingletonMono<GameUIManager>
{
    [FormerlySerializedAs("MainPage")] [SerializeField] private GameObject mainPage;
    [SerializeField] private GameObject levelsPage;
    


    public void ShowMainPage()
    {
        mainPage.SetActive(true);
        levelsPage.SetActive(false);
    }

    public void ShowLevelsPage()
    {
        levelsPage.SetActive(true);
        mainPage.SetActive(false);
    }

   
}
