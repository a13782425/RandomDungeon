using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenerateTest : MonoBehaviour
{

    /// <summary>
    /// 生成按钮
    /// </summary>
    public Button GenerateButton;
    /// <summary>
    /// 初始化按钮
    /// </summary>
    public Button InitButton;
    /// <summary>
    /// 重置按钮
    /// </summary>
    public Button ResetButton;

    public GenerateCode generateCode;


    private bool _isInit = false;

    // Use this for initialization
    void Start()
    {
        if (generateCode == null)
        {
            generateCode = Transform.FindObjectOfType<GenerateCode>();
        }
        InitButton.onClick.AddListener(InitButtonClick);
        GenerateButton.onClick.AddListener(GenerateButtonClick);
        ResetButton.onClick.AddListener(ResetButtonClick);
    }

    private void ResetButtonClick()
    {
        if (!_isInit)
        {
            return;
        }
        generateCode.Reset();
    }

    private void GenerateButtonClick()
    {
        if (!_isInit)
        {
            return;
        }
        generateCode.Generate();
    }

    private void InitButtonClick()
    {
        if (_isInit)
        {
            return;
        }
        _isInit = true;
        generateCode.Init();
    }
}
