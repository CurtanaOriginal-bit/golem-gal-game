using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

/// <summary>
/// タイトル画面の Model です。音量・表示モードの読み込みと保存、ゲーム終了、シーン遷移を行います。
/// </summary>
public class EntryModel : MonoBehaviour
{
    /// <summary>
    /// 導入シーンの名前
    /// </summary>
    private const string IntroductionSceneName = "Introduction";

    /// <summary>
    /// 音量を反映する AudioMixer
    /// </summary>
    [FormerlySerializedAs("_audioMixer")]
    [SerializeField] private AudioMixer audioMixer;

    /// <summary>
    /// 導入シーンへ遷移するためのローダー
    /// </summary>
    private SceneLoader _introductionSceneLoader;

    /// <summary>
    /// マスター音量（0〜1）
    /// </summary>
    public float MasterVolume { get; private set; }

    /// <summary>
    /// BGM 音量（0〜1）
    /// </summary>
    public float BGMVolume { get; private set; }

    /// <summary>
    /// SE 音量（0〜1）
    /// </summary>
    public float SEVolume { get; private set; }

    /// <summary>
    /// フルスクリーン表示かどうか
    /// </summary>
    public bool IsFullScreen { get; private set; }

    /// <summary>
    /// 導入シーンへのローダーを作成します。
    /// </summary>
    private void Awake()
    {
        _introductionSceneLoader = new SceneLoader(IntroductionSceneName);
    }

    /// <summary>
    /// 保存されている音量と表示モードを読み込み、反映します。
    /// </summary>
    public void LoadSettings()
    {
        MasterVolume = VolumeSettings.Load(VolumeSettings.MasterVolumeKey);
        BGMVolume = VolumeSettings.Load(VolumeSettings.BGMVolumeKey);
        SEVolume = VolumeSettings.Load(VolumeSettings.SEVolumeKey);
        ApplyVolume();

        IsFullScreen = ScreenModeSettings.Load();
        ScreenModeSettings.Apply(IsFullScreen);
    }

    /// <summary>
    /// 表示モードを保存して反映します。
    /// </summary>
    public void SaveFullScreen(bool isFullScreen)
    {
        IsFullScreen = isFullScreen;
        ScreenModeSettings.Save(isFullScreen);
    }

    /// <summary>
    /// マスター音量を保存して反映します。
    /// </summary>
    public void SaveMasterVolume(float value)
    {
        MasterVolume = value;
        VolumeSettings.Save(VolumeSettings.MasterVolumeKey, value);
        ApplyVolume();
    }

    /// <summary>
    /// BGM 音量を保存して反映します。
    /// </summary>
    public void SaveBGMVolume(float value)
    {
        BGMVolume = value;
        VolumeSettings.Save(VolumeSettings.BGMVolumeKey, value);
        ApplyVolume();
    }

    /// <summary>
    /// SE 音量を保存して反映します。
    /// </summary>
    public void SaveSEVolume(float value)
    {
        SEVolume = value;
        VolumeSettings.Save(VolumeSettings.SEVolumeKey, value);
        ApplyVolume();
    }

    /// <summary>
    /// ゲームを終了します（エディタ上では再生を止めます）。
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("[EntryModel] ゲームを終了します。");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// 導入シーンへ遷移します。
    /// </summary>
    public void LoadIntroductionScene()
    {
        _introductionSceneLoader.Load();
    }

    /// <summary>
    /// 現在の音量を AudioMixer に反映します。
    /// </summary>
    private void ApplyVolume()
    {
        VolumeSettings.ApplyToMixer(audioMixer, VolumeSettings.MasterVolumeKey, MasterVolume);
        VolumeSettings.ApplyToMixer(audioMixer, VolumeSettings.BGMVolumeKey, BGMVolume);
        VolumeSettings.ApplyToMixer(audioMixer, VolumeSettings.SEVolumeKey, SEVolume);

        Debug.Log($"[EntryModel] 音量適用 - Master: {MasterVolume}, BGM: {BGMVolume}, SE: {SEVolume}");
    }
}
