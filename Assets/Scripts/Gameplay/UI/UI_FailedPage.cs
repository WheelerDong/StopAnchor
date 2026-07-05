using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_FailedPage : MonoBehaviour
{
    [SerializeField] private Button restart;

    private void Awake()
    {
        restart.onClick.AddListener(OnRestartClicked);
    }

    private void OnRestartClicked()
    {
        GameplayManager.Instance.RestartLevel();
    }
}
