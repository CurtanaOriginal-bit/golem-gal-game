using UnityEngine;
using UnityEngine.UI;
using System;

public class EntryView : ViewBase
{
    [Header("Main Menu UI")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private GameObject mainGameUI; // GamesシーンのStartUpViewなど

    [Header("Settings Panel UI")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button closeSettingsButton;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider seVolumeSlider;

    // Presenterが登録するコールバック
    public Action OnStartClicked;
    public Action OnSettingsClicked;
    public Action OnExitClicked;
    public Action OnCloseSettingsClicked;
    public Action<float> OnMasterVolumeChanged;
    public Action<float> OnBgmVolumeChanged;
    public Action<float> OnSeVolumeChanged;

    void Start()
    {
        // UIのイベント登録
        if (startButton != null) startButton.onClick.AddListener(() => OnStartClicked?.Invoke());
        if (settingsButton != null) settingsButton.onClick.AddListener(() => OnSettingsClicked?.Invoke());
        if (exitButton != null) exitButton.onClick.AddListener(() => OnExitClicked?.Invoke());
        if (closeSettingsButton != null) closeSettingsButton.onClick.AddListener(() => OnCloseSettingsClicked?.Invoke());

        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(val => OnMasterVolumeChanged?.Invoke(val));
        if (bgmVolumeSlider != null) bgmVolumeSlider.onValueChanged.AddListener(val => OnBgmVolumeChanged?.Invoke(val));
        if (seVolumeSlider != null) seVolumeSlider.onValueChanged.AddListener(val => OnSeVolumeChanged?.Invoke(val));

        // 初期状態で設定画面は非表示
        SetSettingsPanelActive(false);
    }

    public void SetSettingsPanelActive(bool active)
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(active);
        }
    }

    public void StartGame()
    {
        // タイトル画面を非表示にし、メインゲームUIを表示する
        gameObject.SetActive(false);
        if (mainGameUI != null)
        {
            mainGameUI.SetActive(true);
        }
    }

    public void InitializeSliders(float master, float bgm, float se)
    {
        if (masterVolumeSlider != null) masterVolumeSlider.value = master;
        if (bgmVolumeSlider != null) bgmVolumeSlider.value = bgm;
        if (seVolumeSlider != null) seVolumeSlider.value = se;
    }
}
