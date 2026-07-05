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
    public event Action OnStartClicked;
    public event Action OnSettingsClicked;
    public event Action OnExitClicked;
    
    // 設定画面が生成されたときにPresenterに通知するコールバック
    public event Action<SettingsView> OnSettingsOpened;

    private void OnEnable()
    {
        // UIのイベント登録
        if (startButton != null) startButton.onClick.AddListener(HandleStartClicked);
        if (settingsButton != null) settingsButton.onClick.AddListener(HandleSettingsClicked);
        if (exitButton != null) exitButton.onClick.AddListener(HandleExitClicked);
    }

    private void OnDisable()
    {
        // UIのイベント解除
        if (startButton != null) startButton.onClick.RemoveListener(HandleStartClicked);
        if (settingsButton != null) settingsButton.onClick.RemoveListener(HandleSettingsClicked);
        if (exitButton != null) exitButton.onClick.RemoveListener(HandleExitClicked);
    }

    private void HandleStartClicked() => OnStartClicked?.Invoke();
    private void HandleSettingsClicked() => OnSettingsClicked?.Invoke();
    private void HandleExitClicked() => OnExitClicked?.Invoke();

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
