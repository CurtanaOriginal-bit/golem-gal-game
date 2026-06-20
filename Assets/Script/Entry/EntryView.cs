using UnityEngine;
using UnityEngine.UI;
using System;

public class EntryView : ViewBase
{
    [Header("Main Menu UI")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    [Header("Settings Prefab UI")]
    [SerializeField] private SettingsView settingsPrefab;
    [SerializeField] private RectTransform canvasTransform;

    private SettingsView activeSettingsInstance;

    // Presenterが登録するコールバック
    public Action OnStartClicked;
    public Action OnSettingsClicked;
    public Action OnExitClicked;
    
    // 設定画面が生成されたときにPresenterに通知するコールバック
    public Action<SettingsView> OnSettingsOpened;

    void Start()
    {
        // UIのイベント登録
        if (startButton != null) startButton.onClick.AddListener(() => OnStartClicked?.Invoke());
        if (settingsButton != null) settingsButton.onClick.AddListener(() => OnSettingsClicked?.Invoke());
        if (exitButton != null) exitButton.onClick.AddListener(() => OnExitClicked?.Invoke());
    }

    public bool IsSettingsPanelActive => activeSettingsInstance != null;

    public void OpenSettings()
    {
        if (activeSettingsInstance != null) return;

        if (settingsPrefab != null && canvasTransform != null)
        {
            activeSettingsInstance = Instantiate(settingsPrefab, canvasTransform);
            OnSettingsOpened?.Invoke(activeSettingsInstance);
        }
        else
        {
            Debug.LogError("settingsPrefab または canvasTransform がアタッチされていません。");
        }
    }

    public void CloseSettings()
    {
        if (activeSettingsInstance != null)
        {
            Destroy(activeSettingsInstance.gameObject);
            activeSettingsInstance = null;
        }
    }
}
