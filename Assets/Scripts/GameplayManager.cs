using System;
using System.Collections;
using System.Collections.Generic;
using Singleton.Base;
using UnityEngine;

public class GameplayManager : SingletonMono<GameplayManager>
{
    private float currentGlobelAngle = 0f;
    private float rotateSpeed = 180f;
    private void Update()
    {
        HandleRotateInput();
    }
    
    private void HandleRotateInput()
    {
        float input = 0f;

        // A：逆时针
        if (Input.GetKey(KeyCode.A))
        {
            input += 1f;
        }

        // D：顺时针
        if (Input.GetKey(KeyCode.D))
        {
            input -= 1f;
        }

        if (Mathf.Approximately(input, 0f))
        {
            return;
        }

        currentGlobelAngle += input * rotateSpeed * Time.deltaTime;
        
    }
    
    public float GetCurrentGlobelAngle() => currentGlobelAngle;
}
