using UnityEngine;
using UnityEngine.UI;
using System;

public class SettingsView : MonoBehaviour
{
    [SerializeField] private Button closeSettingsButton;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider seVolumeSlider;
    [SerializeField] private Button masterMuteButton;
    [SerializeField] private Button bgmMuteButton;
    [SerializeField] private Button seMuteButton;

    public Action OnCloseSettingsClicked;
    public Action<float> OnMasterVolumeChanged;
    public Action<float> OnBgmVolumeChanged;
    public Action<float> OnSeVolumeChanged;

    private bool _isInitializing = false;

    private void OnEnable()
    {
        if (closeSettingsButton != null) closeSettingsButton.onClick.AddListener(HandleCloseSettingsClicked);
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(HandleMasterVolumeChanged);
        if (bgmVolumeSlider != null) bgmVolumeSlider.onValueChanged.AddListener(HandleBgmVolumeChanged);
        if (seVolumeSlider != null) seVolumeSlider.onValueChanged.AddListener(HandleSeVolumeChanged);

        if (masterMuteButton != null) masterMuteButton.onClick.AddListener(MuteMaster);
        if (bgmMuteButton != null) bgmMuteButton.onClick.AddListener(MuteBgm);
        if (seMuteButton != null) seMuteButton.onClick.AddListener(MuteSe);
    }

    private void OnDisable()
    {
        if (closeSettingsButton != null) closeSettingsButton.onClick.RemoveListener(HandleCloseSettingsClicked);
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.RemoveListener(HandleMasterVolumeChanged);
        if (bgmVolumeSlider != null) bgmVolumeSlider.onValueChanged.RemoveListener(HandleBgmVolumeChanged);
        if (seVolumeSlider != null) seVolumeSlider.onValueChanged.RemoveListener(HandleSeVolumeChanged);

        if (masterMuteButton != null) masterMuteButton.onClick.RemoveListener(MuteMaster);
        if (bgmMuteButton != null) bgmMuteButton.onClick.RemoveListener(MuteBgm);
        if (seMuteButton != null) seMuteButton.onClick.RemoveListener(MuteSe);
    }

    public void InitializeSliders(float master, float bgm, float se)
    {
        _isInitializing = true;
        if (masterVolumeSlider != null) masterVolumeSlider.value = master;
        if (bgmVolumeSlider != null) bgmVolumeSlider.value = bgm;
        if (seVolumeSlider != null) seVolumeSlider.value = se;
        _isInitializing = false;
    }

    private void HandleCloseSettingsClicked() => OnCloseSettingsClicked?.Invoke();

    private void HandleMasterVolumeChanged(float val)
    {
        if (_isInitializing) return;
        OnMasterVolumeChanged?.Invoke(val);
    }

    private void HandleBgmVolumeChanged(float val)
    {
        if (_isInitializing) return;
        OnBgmVolumeChanged?.Invoke(val);
    }

    private void HandleSeVolumeChanged(float val)
    {
        if (_isInitializing) return;
        OnSeVolumeChanged?.Invoke(val);
    }

    private void MuteMaster() { if (masterVolumeSlider != null) masterVolumeSlider.value = 0f; }
    private void MuteBgm() { if (bgmVolumeSlider != null) bgmVolumeSlider.value = 0f; }
    private void MuteSe() { if (seVolumeSlider != null) seVolumeSlider.value = 0f; }
}
