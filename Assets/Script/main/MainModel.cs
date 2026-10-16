using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 着せる・脱がすの操作モードを表す列挙型
/// </summary>
public enum OutfitControlMode
{
    /// <summary>操作なし</summary>
    None,

    /// <summary>着せる</summary>
    Dress,

    /// <summary>脱がす</summary>
    Strip
}

/// <summary>
/// 着替えのアクション種別を表す列挙型
/// </summary>
public enum OutfitActionType
{
    /// <summary>上半身の服を脱がした</summary>
    StripUpper,

    /// <summary>下半身の服を脱がした</summary>
    StripLower,

    /// <summary>上半身の服を着せた</summary>
    DressUpper,

    /// <summary>下半身の服を着せた</summary>
    DressLower
}

/// <summary>
/// メイン画面の Model です。設定値・服装・ゲージ・表情・挿入/フェラの状態・アクション回数・会話の進行を管理します。
/// </summary>
public class MainModel : MonoBehaviour
{
    /// <summary>
    /// ゲージの最大値
    /// </summary>
    public const float MaxGaugeValue = 100f;

    /// <summary>
    /// タイトルシーンの名前
    /// </summary>
    private const string TitleSceneName = "Title";

    /// <summary>
    /// エンディングシーンの名前
    /// </summary>
    private const string EndingSceneName = "Ending";

    /// <summary>
    /// 挿入中・フェラ中に射精感を増やす間隔(秒)
    /// </summary>
    private const float Gauge2IncreaseInterval = 1f;

    /// <summary>
    /// 挿入中・フェラ中に 1 回で増やす射精感の量
    /// </summary>
    private const float Gauge2IncreaseAmount = 1f;

    /// <summary>
    /// 表情 1（少し興奮）に切り替わる興奮度
    /// </summary>
    private const float FaceLevel1Gauge = 20f;

    /// <summary>
    /// 表情 2（興奮）に切り替わる興奮度
    /// </summary>
    private const float FaceLevel2Gauge = 50f;

    /// <summary>
    /// 表情 3（とても興奮）に切り替わる興奮度
    /// </summary>
    private const float FaceLevel3Gauge = 80f;

    /// <summary>
    /// テキストファイルを 1 行ずつに分けるときの区切り文字
    /// </summary>
    private static readonly char[] LineSeparators = { '\r', '\n' };

    /// <summary>
    /// 会話ボタンごとのセリフを書いたテキストファイル（1 行 1 セリフ）
    /// </summary>
    [Header("Talk Settings")]
    [Tooltip("各ボタンに対応するセリフが書かれたテキストファイル(.txt)。1行1セリフとして読み込まれます。")]
    [SerializeField] private TextAsset[] talkTextFiles;

    /// <summary>
    /// 会話ボタンごとのセリフ
    /// </summary>
    [SerializeField] private TalkGroup[] talkGroups;

    /// <summary>
    /// オート会話モードかどうか
    /// </summary>
    [SerializeField] private bool isAutoTalkMode;

    /// <summary>
    /// オート会話で次のセリフへ進むまでの秒数
    /// </summary>
    [SerializeField] private float autoTalkInterval = 2.0f;

    /// <summary>
    /// 音量を反映する AudioMixer
    /// </summary>
    [Header("Audio Settings")]
    [SerializeField] private AudioMixer audioMixer;

    /// <summary>
    /// アクションごとの実行回数（キーはアクション名）
    /// </summary>
    private readonly Dictionary<string, int> _actionCounts = new Dictionary<string, int>();

    /// <summary>
    /// タイトルシーンへ遷移するためのローダー
    /// </summary>
    private SceneLoader _titleSceneLoader;

    /// <summary>
    /// エンディングシーンへ遷移するためのローダー
    /// </summary>
    private SceneLoader _endingSceneLoader;

    /// <summary>
    /// 挿入中に射精感を増やすための経過時間(秒)
    /// </summary>
    private float _penetrationTimer;

    /// <summary>
    /// フェラ中に射精感を増やすための経過時間(秒)
    /// </summary>
    private float _blowjobTimer;

    /// <summary>
    /// 進行中の会話の番号（会話中でなければ -1）
    /// </summary>
    private int _currentTalkIndex = -1;

    /// <summary>
    /// 進行中の会話で表示しているセリフの番号（会話中でなければ -1）
    /// </summary>
    private int _currentSentenceIndex = -1;

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
    /// 現在の操作モード（着せる・脱がす・なし）
    /// </summary>
    public OutfitControlMode CurrentMode { get; private set; }

    /// <summary>
    /// 上半身の奥側の服（画像3）を表示しているかどうか
    /// </summary>
    public bool UpperBackVisible { get; private set; } = true;

    /// <summary>
    /// 上半身の手前側の服（画像2）を表示しているかどうか
    /// </summary>
    public bool UpperFrontVisible { get; private set; } = true;

    /// <summary>
    /// 下半身の奥側の服（画像5）を表示しているかどうか
    /// </summary>
    public bool LowerBackVisible { get; private set; } = true;

    /// <summary>
    /// 下半身の手前側の服（画像4）を表示しているかどうか
    /// </summary>
    public bool LowerFrontVisible { get; private set; } = true;

    /// <summary>
    /// 興奮度（Gauge1）の現在値
    /// </summary>
    public float Gauge1Value { get; private set; }

    /// <summary>
    /// 射精感（Gauge2）の現在値
    /// </summary>
    public float Gauge2Value { get; private set; }

    /// <summary>
    /// 現在の表情の番号（0〜3）
    /// </summary>
    public int CurrentFaceIndex { get; private set; }

    /// <summary>
    /// 挿入中かどうか
    /// </summary>
    public bool IsPenetrationActive { get; private set; }

    /// <summary>
    /// フェラ中かどうか
    /// </summary>
    public bool IsBlowjobActive { get; private set; }

    /// <summary>
    /// 会話中かどうか
    /// </summary>
    public bool IsTalking => _currentTalkIndex >= 0;

    /// <summary>
    /// オート会話モードかどうかを取得・設定します。
    /// </summary>
    public bool IsAutoTalkMode
    {
        get => isAutoTalkMode;
        set => isAutoTalkMode = value;
    }

    /// <summary>
    /// オート会話で次のセリフへ進むまでの秒数
    /// </summary>
    public float AutoTalkInterval => autoTalkInterval;

    /// <summary>
    /// 服装の表示状態が変わったときに発火するイベント
    /// </summary>
    public event Action OnOutfitStateChanged;

    /// <summary>
    /// 操作モードが変わったときに発火するイベント
    /// </summary>
    public event Action<OutfitControlMode> OnControlModeChanged;

    /// <summary>
    /// 着替えのアクションが実行され、表示が切り替わった際に発火するイベント
    /// </summary>
    public event Action<OutfitActionType> OnOutfitActionExecuted;

    /// <summary>
    /// ゲージの値が変わったときに発火するイベント（引数は Gauge1, Gauge2）
    /// </summary>
    public event Action<float, float> OnGaugeChanged;

    /// <summary>
    /// 表情が変わったときに発火するイベント
    /// </summary>
    public event Action<int> OnFaceChanged;

    /// <summary>
    /// 挿入・フェラの状態が変わったときに発火するイベント（引数は挿入中, フェラ中）
    /// </summary>
    public event Action<bool, bool> OnOrderStateChanged;

    /// <summary>
    /// シーンのローダーを作成し、テキストファイルから会話のセリフを読み込みます。
    /// </summary>
    private void Awake()
    {
        _titleSceneLoader = new SceneLoader(TitleSceneName);
        _endingSceneLoader = new SceneLoader(EndingSceneName);

        LoadTalkTextFiles();
    }

    /// <summary>
    /// 挿入中・フェラ中は一定時間ごとに射精感を増やします。
    /// </summary>
    private void Update()
    {
        UpdateGauge2Timer(IsPenetrationActive, ref _penetrationTimer);
        UpdateGauge2Timer(IsBlowjobActive, ref _blowjobTimer);
    }

    /// <summary>
    /// 保存されている音量と表示モードを読み込み、音量を反映します。
    /// </summary>
    public void LoadSettings()
    {
        MasterVolume = VolumeSettings.Load(VolumeSettings.MasterVolumeKey);
        BGMVolume = VolumeSettings.Load(VolumeSettings.BGMVolumeKey);
        SEVolume = VolumeSettings.Load(VolumeSettings.SEVolumeKey);
        ApplyVolume();

        IsFullScreen = ScreenModeSettings.Load();
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
    /// タイトルシーンへ遷移します。
    /// </summary>
    public void LoadTitleScene()
    {
        _titleSceneLoader.Load();
    }

    /// <summary>
    /// エンディングシーンへ遷移します。
    /// </summary>
    public void LoadEndingScene()
    {
        _endingSceneLoader.Load();
    }

    /// <summary>
    /// 挿入の ON/OFF を切り替えます（ON にするとフェラは OFF になります）。
    /// </summary>
    public void TogglePenetration()
    {
        IsPenetrationActive = !IsPenetrationActive;
        if (IsPenetrationActive)
        {
            IsBlowjobActive = false; // 排他制御
        }
        OnOrderStateChanged?.Invoke(IsPenetrationActive, IsBlowjobActive);
    }

    /// <summary>
    /// フェラの ON/OFF を切り替えます（ON にすると挿入は OFF になります）。
    /// </summary>
    public void ToggleBlowjob()
    {
        IsBlowjobActive = !IsBlowjobActive;
        if (IsBlowjobActive)
        {
            IsPenetrationActive = false; // 排他制御
        }
        OnOrderStateChanged?.Invoke(IsPenetrationActive, IsBlowjobActive);
    }

    /// <summary>
    /// 指定した番号の会話を最初のセリフから始めます。
    /// </summary>
    public void StartTalk(int talkIndex)
    {
        if (talkGroups == null || talkIndex < 0 || talkIndex >= talkGroups.Length) return;

        _currentTalkIndex = talkIndex;
        _currentSentenceIndex = 0;
        Debug.Log($"[MainModel] 会話開始 - TalkIndex: {talkIndex}");
    }

    /// <summary>
    /// 表示中のセリフを取得します（会話中でなければ null）。
    /// </summary>
    public string GetCurrentSentence()
    {
        string[] sentences = GetCurrentSentences();
        if (sentences == null || _currentSentenceIndex >= sentences.Length) return null;

        return sentences[_currentSentenceIndex];
    }

    /// <summary>
    /// 次のセリフへ進めます。最後のセリフを過ぎたら会話を終了して false を返します。
    /// </summary>
    public bool AdvanceSentence()
    {
        string[] sentences = GetCurrentSentences();
        if (sentences == null) return false;

        _currentSentenceIndex++;
        if (_currentSentenceIndex >= sentences.Length)
        {
            EndTalk();
            return false; // 会話終了
        }
        return true; // 次のセリフあり
    }

    /// <summary>
    /// 会話を終了します。
    /// </summary>
    public void EndTalk()
    {
        Debug.Log($"[MainModel] 会話終了 - TalkIndex: {_currentTalkIndex}");
        _currentTalkIndex = -1;
        _currentSentenceIndex = -1;
    }

    /// <summary>
    /// 操作モードを切り替えます。
    /// </summary>
    public void SetControlMode(OutfitControlMode mode)
    {
        CurrentMode = mode;
        Debug.Log($"[MainModel] 操作モード変更: {CurrentMode}");
        OnOutfitStateChanged?.Invoke(); // 表示状態を更新させるため
        OnControlModeChanged?.Invoke(CurrentMode);
    }

    /// <summary>
    /// 上半身がクリックされたとき、操作モードに応じて服を 1 枚着せる・脱がします。
    /// </summary>
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
            NotifyOutfitAction(OutfitActionType.StripUpper, OutfitActionType.DressUpper);
        }
    }

    /// <summary>
    /// 下半身がクリックされたとき、操作モードに応じて服を 1 枚着せる・脱がします。
    /// </summary>
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
            NotifyOutfitAction(OutfitActionType.StripLower, OutfitActionType.DressLower);
        }
    }

    /// <summary>
    /// ゲージと表情を初期値に戻します。
    /// </summary>
    public void InitializeGauges()
    {
        Gauge1Value = 0f;
        Gauge2Value = 0f;
        OnGaugeChanged?.Invoke(Gauge1Value, Gauge2Value);
        Debug.Log($"[MainModel] ゲージ初期化 - Gauge1: {Gauge1Value}, Gauge2: {Gauge2Value}");

        CurrentFaceIndex = CalculateFaceIndex(Gauge1Value);
        OnFaceChanged?.Invoke(CurrentFaceIndex);
    }

    /// <summary>
    /// 興奮度（Gauge1）を増やし、表情も更新します。
    /// </summary>
    public void IncreaseGauge1(float amount)
    {
        Gauge1Value = Mathf.Clamp(Gauge1Value + amount, 0f, MaxGaugeValue);
        OnGaugeChanged?.Invoke(Gauge1Value, Gauge2Value);
        Debug.Log($"[MainModel] Gauge1増加: {Gauge1Value}/{MaxGaugeValue}");
        UpdateFaceExpression();
    }

    /// <summary>
    /// 射精感（Gauge2）を増やします。
    /// </summary>
    public void IncreaseGauge2(float amount)
    {
        Gauge2Value = Mathf.Clamp(Gauge2Value + amount, 0f, MaxGaugeValue);
        OnGaugeChanged?.Invoke(Gauge1Value, Gauge2Value);
        Debug.Log($"[MainModel] Gauge2増加: {Gauge2Value}/{MaxGaugeValue}");
    }

    /// <summary>
    /// 指定されたキーのアクション実行回数を1増やします。
    /// </summary>
    public void IncrementActionCount(string actionKey)
    {
        if (string.IsNullOrEmpty(actionKey)) return;

        _actionCounts.TryGetValue(actionKey, out int count);
        _actionCounts[actionKey] = count + 1;
        Debug.Log($"[MainModel] アクション回数増加: {actionKey} = {count + 1}");
    }

    /// <summary>
    /// 指定されたキーのアクション実行回数を取得します。
    /// </summary>
    public int GetActionCount(string actionKey)
    {
        if (string.IsNullOrEmpty(actionKey)) return 0;

        return _actionCounts.TryGetValue(actionKey, out int count) ? count : 0;
    }

    /// <summary>
    /// テキストファイルから会話のセリフを読み込みます（1 行 1 セリフ、空行は無視します）。
    /// </summary>
    private void LoadTalkTextFiles()
    {
        if (talkTextFiles == null || talkTextFiles.Length == 0) return;

        if (talkGroups == null || talkGroups.Length < talkTextFiles.Length)
        {
            var newGroups = new TalkGroup[talkTextFiles.Length];
            for (int i = 0; i < talkTextFiles.Length; i++)
            {
                newGroups[i] = (talkGroups != null && i < talkGroups.Length && talkGroups[i] != null)
                               ? talkGroups[i] : new TalkGroup();
            }
            talkGroups = newGroups;
        }

        for (int i = 0; i < talkTextFiles.Length; i++)
        {
            if (talkTextFiles[i] == null) continue;

            // 改行コードで分割（空白行は除去する）
            string[] lines = talkTextFiles[i].text.Split(LineSeparators, StringSplitOptions.RemoveEmptyEntries);

            if (talkGroups[i] == null)
            {
                talkGroups[i] = new TalkGroup();
            }
            talkGroups[i].SetSentences(lines);
        }
    }

    /// <summary>
    /// トグルが ON の間、経過時間を数え、一定時間ごとに射精感を増やします（OFF なら経過時間を戻します）。
    /// </summary>
    private void UpdateGauge2Timer(bool isActive, ref float timer)
    {
        if (!isActive)
        {
            timer = 0f; // OFFになったらタイマーリセット
            return;
        }

        timer += Time.deltaTime;
        if (timer >= Gauge2IncreaseInterval)
        {
            IncreaseGauge2(Gauge2IncreaseAmount);
            timer -= Gauge2IncreaseInterval;
        }
    }

    /// <summary>
    /// 進行中の会話のセリフ一覧を取得します（会話中でなければ null）。
    /// </summary>
    private string[] GetCurrentSentences()
    {
        if (!IsTalking) return null;
        if (talkGroups == null || _currentTalkIndex >= talkGroups.Length) return null;

        return talkGroups[_currentTalkIndex].Sentences;
    }

    /// <summary>
    /// 服装の変更と、着替えのアクション（脱がした・着せた）を通知します。
    /// </summary>
    private void NotifyOutfitAction(OutfitActionType stripAction, OutfitActionType dressAction)
    {
        OnOutfitStateChanged?.Invoke();

        OutfitActionType actionType = (CurrentMode == OutfitControlMode.Strip) ? stripAction : dressAction;
        OnOutfitActionExecuted?.Invoke(actionType);
    }

    /// <summary>
    /// 興奮度から表情の番号を求めます。
    /// </summary>
    private static int CalculateFaceIndex(float gauge1Value)
    {
        if (gauge1Value >= FaceLevel3Gauge) return 3;
        if (gauge1Value >= FaceLevel2Gauge) return 2;
        if (gauge1Value >= FaceLevel1Gauge) return 1;
        return 0;
    }

    /// <summary>
    /// 興奮度に合わせて表情を更新します（変わったときだけ通知します）。
    /// </summary>
    private void UpdateFaceExpression()
    {
        int newIndex = CalculateFaceIndex(Gauge1Value);
        if (newIndex == CurrentFaceIndex) return;

        CurrentFaceIndex = newIndex;
        Debug.Log($"[MainModel] 表情変更: {CurrentFaceIndex} (Gauge1: {Gauge1Value})");
        OnFaceChanged?.Invoke(CurrentFaceIndex);
    }

    /// <summary>
    /// 現在の音量を AudioMixer に反映します（タイトル画面・導入画面と同じ反映方法）。
    /// </summary>
    private void ApplyVolume()
    {
        VolumeSettings.ApplyToMixer(audioMixer, VolumeSettings.MasterVolumeKey, MasterVolume);
        VolumeSettings.ApplyToMixer(audioMixer, VolumeSettings.BGMVolumeKey, BGMVolume);
        VolumeSettings.ApplyToMixer(audioMixer, VolumeSettings.SEVolumeKey, SEVolume);

        Debug.Log($"[MainModel] 音量適用 - Master: {MasterVolume}, BGM: {BGMVolume}, SE: {SEVolume}");
    }

    /// <summary>
    /// 1 つの会話ボタンに対応するセリフのまとまりです。
    /// </summary>
    [Serializable]
    public class TalkGroup
    {
        /// <summary>
        /// 会話のセリフ（表示順）
        /// </summary>
        [TextArea(3, 5)]
        [SerializeField] private string[] sentences;

        /// <summary>
        /// 会話のセリフを取得します。
        /// </summary>
        public string[] Sentences => sentences;

        /// <summary>
        /// 会話のセリフを差し替えます。
        /// </summary>
        public void SetSentences(string[] newSentences)
        {
            sentences = newSentences;
        }
    }
}
