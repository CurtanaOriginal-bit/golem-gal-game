using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// 設定画面の View です。音量スライダー・ミュートボタン・フルスクリーン切り替えの操作を通知します。
/// </summary>
public class SettingsView : MonoBehaviour
{
    /// <summary>
    /// 設定画面を閉じるボタン
    /// </summary>
    [SerializeField] private Button closeSettingsButton;

    /// <summary>
    /// マスター音量のスライダー
    /// </summary>
    [SerializeField] private Slider masterVolumeSlider;

    /// <summary>
    /// BGM 音量のスライダー
    /// </summary>
    [SerializeField] private Slider bgmVolumeSlider;

    /// <summary>
    /// SE 音量のスライダー
    /// </summary>
    [SerializeField] private Slider seVolumeSlider;

    /// <summary>
    /// マスター音量を 0 にするボタン
    /// </summary>
    [SerializeField] private Button masterMuteButton;

    /// <summary>
    /// BGM 音量を 0 にするボタン
    /// </summary>
    [SerializeField] private Button bgmMuteButton;

    /// <summary>
    /// SE 音量を 0 にするボタン
    /// </summary>
    [SerializeField] private Button seMuteButton;

    /// <summary>
    /// フルスクリーン表示を切り替えるトグル
    /// </summary>
    [SerializeField] private Toggle fullScreenToggle;

    /// <summary>
    /// 閉じるボタンが押されたときに呼ばれる処理
    /// </summary>
    public Action OnCloseSettingsClicked { get; set; }

    /// <summary>
    /// マスター音量が変更されたときに呼ばれる処理
    /// </summary>
    public Action<float> OnMasterVolumeChanged { get; set; }

    /// <summary>
    /// BGM 音量が変更されたときに呼ばれる処理
    /// </summary>
    public Action<float> OnBgmVolumeChanged { get; set; }

    /// <summary>
    /// SE 音量が変更されたときに呼ばれる処理
    /// </summary>
    public Action<float> OnSeVolumeChanged { get; set; }

    /// <summary>
    /// フルスクリーン表示が切り替えられたときに呼ばれる処理
    /// </summary>
    public Action<bool> OnFullScreenChanged { get; set; }

    /// <summary>
    /// 初期値を反映している最中かどうか（その間は変更を通知しない）
    /// </summary>
    private bool _isInitializing;

    /// <summary>
    /// 有効化時に各 UI の操作イベントを購読します。
    /// </summary>
    private void OnEnable()
    {
        if (closeSettingsButton != null) closeSettingsButton.onClick.AddListener(HandleCloseSettingsClicked);
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(HandleMasterVolumeChanged);
        if (bgmVolumeSlider != null) bgmVolumeSlider.onValueChanged.AddListener(HandleBgmVolumeChanged);
        if (seVolumeSlider != null) seVolumeSlider.onValueChanged.AddListener(HandleSeVolumeChanged);

        if (masterMuteButton != null) masterMuteButton.onClick.AddListener(MuteMaster);
        if (bgmMuteButton != null) bgmMuteButton.onClick.AddListener(MuteBgm);
        if (seMuteButton != null) seMuteButton.onClick.AddListener(MuteSe);

        if (fullScreenToggle != null) fullScreenToggle.onValueChanged.AddListener(HandleFullScreenChanged);
    }

    /// <summary>
    /// 無効化時に各 UI の操作イベントの購読を解除します。
    /// </summary>
    private void OnDisable()
    {
        if (closeSettingsButton != null) closeSettingsButton.onClick.RemoveListener(HandleCloseSettingsClicked);
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.RemoveListener(HandleMasterVolumeChanged);
        if (bgmVolumeSlider != null) bgmVolumeSlider.onValueChanged.RemoveListener(HandleBgmVolumeChanged);
        if (seVolumeSlider != null) seVolumeSlider.onValueChanged.RemoveListener(HandleSeVolumeChanged);

        if (masterMuteButton != null) masterMuteButton.onClick.RemoveListener(MuteMaster);
        if (bgmMuteButton != null) bgmMuteButton.onClick.RemoveListener(MuteBgm);
        if (seMuteButton != null) seMuteButton.onClick.RemoveListener(MuteSe);

        if (fullScreenToggle != null) fullScreenToggle.onValueChanged.RemoveListener(HandleFullScreenChanged);
    }

    /// <summary>
    /// スライダーに現在の音量を反映します（変更の通知はしません）。
    /// </summary>
    public void InitializeSliders(float master, float bgm, float se)
    {
        _isInitializing = true;
        if (masterVolumeSlider != null) masterVolumeSlider.value = master;
        if (bgmVolumeSlider != null) bgmVolumeSlider.value = bgm;
        if (seVolumeSlider != null) seVolumeSlider.value = se;
        _isInitializing = false;
    }

    /// <summary>
    /// トグルに現在の表示モードを反映します（変更の通知はしません）。
    /// </summary>
    public void InitializeFullScreenToggle(bool isFullScreen)
    {
        _isInitializing = true;
        if (fullScreenToggle != null) fullScreenToggle.isOn = isFullScreen;
        _isInitializing = false;
    }

    /// <summary>
    /// 閉じるボタンが押されたことを通知します。
    /// </summary>
    private void HandleCloseSettingsClicked() => OnCloseSettingsClicked?.Invoke();

    /// <summary>
    /// マスター音量の変更を通知します（初期値の反映中は通知しません）。
    /// </summary>
    private void HandleMasterVolumeChanged(float val)
    {
        if (_isInitializing) return;
        OnMasterVolumeChanged?.Invoke(val);
    }

    /// <summary>
    /// BGM 音量の変更を通知します（初期値の反映中は通知しません）。
    /// </summary>
    private void HandleBgmVolumeChanged(float val)
    {
        if (_isInitializing) return;
        OnBgmVolumeChanged?.Invoke(val);
    }

    /// <summary>
    /// SE 音量の変更を通知します（初期値の反映中は通知しません）。
    /// </summary>
    private void HandleSeVolumeChanged(float val)
    {
        if (_isInitializing) return;
        OnSeVolumeChanged?.Invoke(val);
    }

    /// <summary>
    /// フルスクリーン表示の切り替えを通知します（初期値の反映中は通知しません）。
    /// </summary>
    private void HandleFullScreenChanged(bool isOn)
    {
        if (_isInitializing) return;
        OnFullScreenChanged?.Invoke(isOn);
    }

    /// <summary>
    /// マスター音量のスライダーを 0 にします。
    /// </summary>
    private void MuteMaster()
    {
        Mute(masterVolumeSlider);
    }

    /// <summary>
    /// BGM 音量のスライダーを 0 にします。
    /// </summary>
    private void MuteBgm()
    {
        Mute(bgmVolumeSlider);
    }

    /// <summary>
    /// SE 音量のスライダーを 0 にします。
    /// </summary>
    private void MuteSe()
    {
        Mute(seVolumeSlider);
    }

    /// <summary>
    /// 指定したスライダーを 0 にします（未設定なら何もしません）。
    /// </summary>
    private static void Mute(Slider slider)
    {
        if (slider == null) return;

        slider.value = 0f;
    }
}
