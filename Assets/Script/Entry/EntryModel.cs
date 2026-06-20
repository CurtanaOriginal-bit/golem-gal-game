using UnityEngine;
using UnityEngine.SceneManagement;

public class EntryModel : MonoBehaviour
{
    private const string MasterVolumeKey = "MasterVolume";
    private const string BGMVolumeKey = "BGMVolume";
    private const string SEVolumeKey = "SEVolume";

    public float MasterVolume { get; private set; }
    public float BGMVolume { get; private set; }
    public float SEVolume { get; private set; }

    public void LoadSettings()
    {
        MasterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        BGMVolume = PlayerPrefs.GetFloat(BGMVolumeKey, 1f);
        SEVolume = PlayerPrefs.GetFloat(SEVolumeKey, 1f);
    }

    public void SaveMasterVolume(float value)
    {
        MasterVolume = value;
        PlayerPrefs.SetFloat(MasterVolumeKey, value);
        PlayerPrefs.Save();
        ApplyVolume();
    }

    public void SaveBGMVolume(float value)
    {
        BGMVolume = value;
        PlayerPrefs.SetFloat(BGMVolumeKey, value);
        PlayerPrefs.Save();
        ApplyVolume();
    }

    public void SaveSEVolume(float value)
    {
        SEVolume = value;
        PlayerPrefs.SetFloat(SEVolumeKey, value);
        PlayerPrefs.Save();
        ApplyVolume();
    }

    private void ApplyVolume()
    {
        // 実際のサウンドマネージャーや AudioMixer に反映するロジック
        AudioListener.volume = MasterVolume; // 全体の音量をマスター音量に紐付ける
        Debug.Log($"音量適用 - Master: {MasterVolume}, BGM: {BGMVolume}, SE: {SEVolume}");
    }

    public void QuitGame()
    {
        Debug.Log("ゲームを終了します。");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void LoadIntroductionScene()
    {
        SceneManager.LoadScene("Introduction");
    }
}
