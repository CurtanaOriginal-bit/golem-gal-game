using UnityEngine;
using UnityEngine.SceneManagement;

public enum OutfitControlMode
{
    None,
    Dress,
    Strip
}

public class MainModel : MonoBehaviour
{
    private const string MasterVolumeKey = "MasterVolume";
    private const string BGMVolumeKey = "BGMVolume";
    private const string SEVolumeKey = "SEVolume";

    public float MasterVolume { get; private set; }
    public float BGMVolume { get; private set; }
    public float SEVolume { get; private set; }

    // === Outfit State ===
    public OutfitControlMode CurrentMode { get; private set; } = OutfitControlMode.None;
    public bool UpperBackVisible { get; private set; } = true;  // 画像3
    public bool UpperFrontVisible { get; private set; } = true; // 画像2
    public bool LowerBackVisible { get; private set; } = true;  // 画像5
    public bool LowerFrontVisible { get; private set; } = true; // 画像4

    public event System.Action OnOutfitStateChanged;
    public event System.Action<OutfitControlMode> OnControlModeChanged;

    // === Gauge Data ===
    public const float MaxGaugeValue = 100f;
    public float Gauge1Value { get; private set; }
    public float Gauge2Value { get; private set; }
    public event System.Action<float, float> OnGaugeChanged;

    private SceneLoader _titleSceneLoader;
    private SceneLoader _endingSceneLoader;

    [System.Serializable]
    public class TalkGroup
    {
        [TextArea(3, 5)]
        [SerializeField] private string[] sentences;

        public string[] Sentences => sentences;
    }

    [Header("Talk Settings")]
    [SerializeField] private TalkGroup[] talkGroups;

    private int _currentTalkIndex = -1;
    private int _currentSentenceIndex = -1;

    public bool IsTalking => _currentTalkIndex >= 0;

    private void Awake()
    {
        _titleSceneLoader = new SceneLoader("Title");
        _endingSceneLoader = new SceneLoader("Ending");
    }

    public void LoadSettings()
    {
        MasterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        BGMVolume = PlayerPrefs.GetFloat(BGMVolumeKey, 1f);
        SEVolume = PlayerPrefs.GetFloat(SEVolumeKey, 1f);
        ApplyVolume();
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
        AudioListener.volume = MasterVolume;
        Debug.Log($"[MainModel] 音量適用 - Master: {MasterVolume}, BGM: {BGMVolume}, SE: {SEVolume}");
    }

    public void LoadTitleScene()
    {
        _titleSceneLoader.Load();
    }

    public void LoadEndingScene()
    {
        _endingSceneLoader.Load();
    }

    public void StartTalk(int talkIndex)
    {
        if (talkGroups == null || talkIndex < 0 || talkIndex >= talkGroups.Length) return;
        _currentTalkIndex = talkIndex;
        _currentSentenceIndex = 0;
        Debug.Log($"[MainModel] 会話開始 - TalkIndex: {talkIndex}");
    }

    public string GetCurrentSentence()
    {
        if (!IsTalking) return null;
        if (talkGroups == null || _currentTalkIndex >= talkGroups.Length) return null;
        var sentences = talkGroups[_currentTalkIndex].Sentences;
        if (sentences == null || _currentSentenceIndex >= sentences.Length) return null;
        return sentences[_currentSentenceIndex];
    }

    public bool AdvanceSentence()
    {
        if (!IsTalking) return false;
        if (talkGroups == null || _currentTalkIndex >= talkGroups.Length) return false;
        var sentences = talkGroups[_currentTalkIndex].Sentences;
        if (sentences == null) return false;
        
        _currentSentenceIndex++;
        if (_currentSentenceIndex >= sentences.Length)
        {
            EndTalk();
            return false; // 会話終了
        }
        return true; // 次のセリフあり
    }

    public void EndTalk()
    {
        Debug.Log($"[MainModel] 会話終了 - TalkIndex: {_currentTalkIndex}");
        _currentTalkIndex = -1;
        _currentSentenceIndex = -1;
    }

    public void SetControlMode(OutfitControlMode mode)
    {
        CurrentMode = mode;
        Debug.Log($"[MainModel] 操作モード変更: {CurrentMode}");
        OnOutfitStateChanged?.Invoke(); // 表示状態を更新させるため
        OnControlModeChanged?.Invoke(CurrentMode);
    }

    public void ClickUpperArea()
    {
        if (CurrentMode == OutfitControlMode.None) return;

        bool changed = false;
        if (CurrentMode == OutfitControlMode.Dress)
        {
            // 着せる: 非表示のレイヤがあれば、下（背面）から表示
            if (!UpperBackVisible)
            {
                UpperBackVisible = true;
                changed = true;
            }
            else if (!UpperFrontVisible)
            {
                UpperFrontVisible = true;
                changed = true;
            }
        }
        else if (CurrentMode == OutfitControlMode.Strip)
        {
            // 脱がす: 表示されているレイヤがあれば、上（前面）から非表示
            if (UpperFrontVisible)
            {
                UpperFrontVisible = false;
                changed = true;
            }
            else if (UpperBackVisible)
            {
                UpperBackVisible = false;
                changed = true;
            }
        }

        if (changed)
        {
            Debug.Log($"[MainModel] 上半分レイヤ更新 - Back: {UpperBackVisible}, Front: {UpperFrontVisible}");
            OnOutfitStateChanged?.Invoke();
        }
    }

    public void ClickLowerArea()
    {
        if (CurrentMode == OutfitControlMode.None) return;

        bool changed = false;
        if (CurrentMode == OutfitControlMode.Dress)
        {
            // 着せる: 非表示のレイヤがあれば、下（背面）から表示
            if (!LowerBackVisible)
            {
                LowerBackVisible = true;
                changed = true;
            }
            else if (!LowerFrontVisible)
            {
                LowerFrontVisible = true;
                changed = true;
            }
        }
        else if (CurrentMode == OutfitControlMode.Strip)
        {
            // 脱がす: 表示されているレイヤがあれば、上（前面）から非表示
            if (LowerFrontVisible)
            {
                LowerFrontVisible = false;
                changed = true;
            }
            else if (LowerBackVisible)
            {
                LowerBackVisible = false;
                changed = true;
            }
        }

        if (changed)
        {
            Debug.Log($"[MainModel] 下半分レイヤ更新 - Back: {LowerBackVisible}, Front: {LowerFrontVisible}");
            OnOutfitStateChanged?.Invoke();
        }
    }

    public void InitializeGauges()
    {
        Gauge1Value = 0f;
        Gauge2Value = 0f;
        OnGaugeChanged?.Invoke(Gauge1Value, Gauge2Value);
        Debug.Log($"[MainModel] ゲージ初期化 - Gauge1: {Gauge1Value}, Gauge2: {Gauge2Value}");
    }

    public void IncreaseGauge1(float amount)
    {
        Gauge1Value = Mathf.Clamp(Gauge1Value + amount, 0f, MaxGaugeValue);
        OnGaugeChanged?.Invoke(Gauge1Value, Gauge2Value);
        Debug.Log($"[MainModel] Gauge1増加: {Gauge1Value}/{MaxGaugeValue}");
    }

    // === インナークラス定義 ===
    private class SceneLoader
    {
        private readonly string _sceneName;

        public SceneLoader(string sceneName)
        {
            _sceneName = sceneName;
        }

        public void Load()
        {
            SceneManager.LoadScene(_sceneName);
        }
    }
}
