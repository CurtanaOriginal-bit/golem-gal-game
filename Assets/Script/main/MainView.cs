using UnityEngine;
using UnityEngine.UI;
using System;

public class MainView : ViewBase
{
    [Header("Settings UI")]
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button titleButton;

    [Header("Talk UI")]
    [SerializeField] private Button[] talkButtons;
    [SerializeField] private TalkWindowView talkWindowPrefab;

    [Header("Settings Prefab UI")]
    [SerializeField] private SettingsView settingsPrefab;
    [SerializeField] private RectTransform canvasTransform;

    private SettingsView activeSettingsInstance;
    private TalkWindowView activeTalkWindowInstance;

    // Presenterが登録するコールバック
    public event Action OnSettingsClicked;
    public event Action OnTitleClicked;
    public event Action<int> OnTalkButtonClicked;
    public event Action OnTalkWindowClicked;
    
    // 設定画面が生成されたときにPresenterに通知するコールバック
    public event Action<SettingsView> OnSettingsOpened;

    private void OnEnable()
    {
        if (settingsButton != null) settingsButton.onClick.AddListener(HandleSettingsClicked);
        if (titleButton != null) titleButton.onClick.AddListener(HandleTitleClicked);
        
        if (talkButtons != null)
        {
            for (int i = 0; i < talkButtons.Length; i++)
            {
                int index = i;
                if (talkButtons[i] != null)
                {
                    talkButtons[i].onClick.AddListener(() => OnTalkButtonClicked?.Invoke(index));
                }
            }
        }
    }

    private void OnDisable()
    {
        if (settingsButton != null) settingsButton.onClick.RemoveListener(HandleSettingsClicked);
        if (titleButton != null) titleButton.onClick.RemoveListener(HandleTitleClicked);
        
        if (talkButtons != null)
        {
            for (int i = 0; i < talkButtons.Length; i++)
            {
                if (talkButtons[i] != null)
                {
                    talkButtons[i].onClick.RemoveAllListeners();
                }
            }
        }
    }

    private void HandleSettingsClicked() => OnSettingsClicked?.Invoke();

    private void HandleTitleClicked() => OnTitleClicked?.Invoke();

    private void HandleTalkWindowClicked() => OnTalkWindowClicked?.Invoke();

    public bool IsSettingsPanelActive => activeSettingsInstance != null;
    public bool IsTalkWindowActive => activeTalkWindowInstance != null;

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
            Debug.LogError("[MainView] settingsPrefab または canvasTransform がアタッチされていません。");
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

    public void OpenTalkWindow(string initialText)
    {
        if (activeTalkWindowInstance != null) return;

        if (talkWindowPrefab != null && canvasTransform != null)
        {
            activeTalkWindowInstance = Instantiate(talkWindowPrefab, canvasTransform);
            activeTalkWindowInstance.SetText(initialText);
            activeTalkWindowInstance.OnWindowClicked += HandleTalkWindowClicked;
        }
        else
        {
            Debug.LogError("[MainView] talkWindowPrefab または canvasTransform がアタッチされていません。");
        }
    }

    public void UpdateTalkText(string text)
    {
        if (activeTalkWindowInstance != null)
        {
            activeTalkWindowInstance.SetText(text);
        }
    }

    public void CloseTalkWindow()
    {
        if (activeTalkWindowInstance != null)
        {
            activeTalkWindowInstance.OnWindowClicked -= HandleTalkWindowClicked;
            Destroy(activeTalkWindowInstance.gameObject);
            activeTalkWindowInstance = null;
        }
    }
}
