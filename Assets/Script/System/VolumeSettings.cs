using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 音量（マスター・BGM・SE）の保存と、AudioMixer への反映を行うクラス。
/// タイトル画面・導入画面・メイン画面で共通して使う。
/// </summary>
public static class VolumeSettings
{
    /// <summary>
    /// マスター音量の保存キー（AudioMixer のパラメータ名と共通）
    /// </summary>
    public const string MasterVolumeKey = "MasterVolume";

    /// <summary>
    /// BGM 音量の保存キー（AudioMixer のパラメータ名と共通）
    /// </summary>
    public const string BGMVolumeKey = "BGMVolume";

    /// <summary>
    /// SE 音量の保存キー（AudioMixer のパラメータ名と共通）
    /// </summary>
    public const string SEVolumeKey = "SEVolume";

    /// <summary>
    /// 未保存のときの音量
    /// </summary>
    private const float DefaultVolume = 1f;

    /// <summary>
    /// デシベルに変換するときの下限（0 だと -∞ になるため）
    /// </summary>
    private const float MinVolume = 0.0001f;

    /// <summary>
    /// 保存されている音量（0〜1）を返します。未保存なら 1 を返します。
    /// </summary>
    public static float Load(string key)
    {
        return PlayerPrefs.GetFloat(key, DefaultVolume);
    }

    /// <summary>
    /// 音量（0〜1）を保存します。
    /// </summary>
    public static void Save(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 音量（0〜1）を AudioMixer に反映します。AudioMixer が未設定なら何もしません。
    /// </summary>
    public static void ApplyToMixer(AudioMixer audioMixer, string key, float value)
    {
        if (audioMixer == null) return;

        audioMixer.SetFloat(key, ToDecibel(value));
    }

    /// <summary>
    /// スライダー値（0〜1）をデシベルに変換します。
    /// </summary>
    public static float ToDecibel(float linear)
    {
        return Mathf.Log10(Mathf.Clamp(linear, MinVolume, 1f)) * 20f;
    }
}
