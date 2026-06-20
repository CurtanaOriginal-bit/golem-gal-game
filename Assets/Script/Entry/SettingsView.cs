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

    void Start()
    {
        if (closeSettingsButton != null) closeSettingsButton.onClick.AddListener(() => OnCloseSettingsClicked?.Invoke());
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(val => OnMasterVolumeChanged?.Invoke(val));
        if (bgmVolumeSlider != null) bgmVolumeSlider.onValueChanged.AddListener(val => OnBgmVolumeChanged?.Invoke(val));
        if (seVolumeSlider != null) seVolumeSlider.onValueChanged.AddListener(val => OnSeVolumeChanged?.Invoke(val));

        if (masterMuteButton != null) masterMuteButton.onClick.AddListener(() => { if (masterVolumeSlider != null) masterVolumeSlider.value = 0f; });
        if (bgmMuteButton != null) bgmMuteButton.onClick.AddListener(() => { if (bgmVolumeSlider != null) bgmVolumeSlider.value = 0f; });
        if (seMuteButton != null) seMuteButton.onClick.AddListener(() => { if (seVolumeSlider != null) seVolumeSlider.value = 0f; });
    }

    public void InitializeSliders(float master, float bgm, float se)
    {
        if (masterVolumeSlider != null) masterVolumeSlider.value = master;
        if (bgmVolumeSlider != null) bgmVolumeSlider.value = bgm;
        if (seVolumeSlider != null) seVolumeSlider.value = se;
    }
}
