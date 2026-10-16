using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// メイン画面の View です。各ボタンの操作の通知と、服装・表情・ゲージ・アニメーション・会話ウィンドウ・設定画面の表示を担当します。
/// </summary>
public class MainView : ViewBase
{
    /// <summary>
    /// 設定画面を開くボタン
    /// </summary>
    [Header("Settings UI")]
    [SerializeField] private Button settingsButton;

    /// <summary>
    /// タイトル画面へ戻るボタン
    /// </summary>
    [SerializeField] private Button titleButton;

    /// <summary>
    /// エンディングへ進むボタン
    /// </summary>
    [SerializeField] private Button endingButton;

    /// <summary>
    /// 再生中のアニメーションを止めるボタン
    /// </summary>
    [Header("Cancel UI")]
    [SerializeField] private Button cancelButton;

    /// <summary>
    /// オート会話の ON/OFF を切り替えるトグル
    /// </summary>
    [Header("Talk UI")]
    [SerializeField] private Toggle autoTalkToggle;

    /// <summary>
    /// 会話ボタン（会話の番号順）
    /// </summary>
    [SerializeField] private Button[] talkButtons;

    /// <summary>
    /// 各会話ボタンの解放条件（talkButtons と同じ順・同じ数）
    /// </summary>
    [Tooltip("各トークボタンの解禁条件（talkButtonsと同じ要素数にしてください）")]
    [SerializeField] private TalkButtonCondition[] talkButtonConditions;

    /// <summary>
    /// 会話ウィンドウのプレハブ
    /// </summary>
    [SerializeField] private TalkWindowView talkWindowPrefab;

    /// <summary>
    /// 会話ボタンの解放条件です。
    /// </summary>
    [Serializable]
    public struct TalkButtonCondition
    {
        /// <summary>
        /// 解放に必要な最小の興奮度（Gauge1）
        /// </summary>
        [Tooltip("解放に必要な最小の興奮度(Gauge1)")]
        public float MinGaugeValue;

        /// <summary>
        /// 解放に追加で必要なアクション回数の条件（空なら条件なし）
        /// </summary>
        [Tooltip("追加で必要なアクション回数条件（空の場合は不要）")]
        public List<RequiredActionCount> RequiredActionCounts;
    }

    /// <summary>
    /// 設定画面のプレハブ
    /// </summary>
    [Header("Settings Prefab UI")]
    [SerializeField] private SettingsView settingsPrefab;

    /// <summary>
    /// 設定画面・会話ウィンドウを生成する親の Canvas
    /// </summary>
    [SerializeField] private RectTransform canvasTransform;

    /// <summary>
    /// 1 つのスクリーンで再生するアニメーションの設定です。
    /// </summary>
    [Serializable]
    public struct ScreenAnimationData
    {
        /// <summary>
        /// アニメーションを表示するスクリーン
        /// </summary>
        public RawImage targetScreen; // 表示先スクリーン

        /// <summary>
        /// そのスクリーンで順番に表示する画像
        /// </summary>
        public Texture2D[] textures; // そのスクリーンで再生する画像
    }

    /// <summary>
    /// ボタン 1 つに対応するアニメーションの設定です。
    /// </summary>
    [Serializable]
    public struct LoopAnimationData
    {
        /// <summary>
        /// アニメーションを再生するボタン
        /// </summary>
        public Button button;

        /// <summary>
        /// 再生するスクリーンと画像の組み合わせ
        /// </summary>
        public ScreenAnimationData[] screenAnimations; // スクリーンと画像のペア

        /// <summary>
        /// 自動でループ再生するかどうか（false なら押すたびにコマ送り）
        /// </summary>
        public bool isLoop; // ループ再生するかどうか
    }

    /// <summary>
    /// スクリーン未指定のアニメーションを表示する RawImage
    /// </summary>
    [Header("Loop Animation UI")]
    [SerializeField] private RawImage animationRawImage;

    /// <summary>
    /// アニメーション再生時に排他的に表示を切り替えるサブスクリーン
    /// </summary>
    [SerializeField] private RawImage[] managedSubScreens; // 排他表示制御するためのサブスクリーン一覧

    /// <summary>
    /// ボタンごとのアニメーション設定
    /// </summary>
    [SerializeField] private LoopAnimationData[] loopAnimations;

    /// <summary>
    /// アニメーションの 1 コマを表示する秒数
    /// </summary>
    [SerializeField] private float animationFrameRate = 0.5f;

    /// <summary>
    /// ボタンを押したときにサブスクリーンを強制的に切り替える設定です。
    /// </summary>
    [Serializable]
    public struct SubScreenToggleData
    {
        /// <summary>
        /// 切り替えのきっかけとなるボタン
        /// </summary>
        public Button triggerButton;     // 押下するボタン

        /// <summary>
        /// 表示に切り替える RawImage
        /// </summary>
        public RawImage targetToEnable;  // オン(チェックを入れる)にするRawImage

        /// <summary>
        /// 非表示に切り替える RawImage
        /// </summary>
        public RawImage targetToDisable; // オフ(チェックを外す)にするRawImage
    }

    /// <summary>
    /// ボタンを押したときのサブスクリーンの強制切り替え設定
    /// </summary>
    [Header("Exclusive Sub Screen Toggles")]
    [SerializeField] private SubScreenToggleData[] subScreenToggles; // ボタン押下時の強制トグル設定

    /// <summary>
    /// 「着せる」モードにするボタン
    /// </summary>
    [Header("Dressing/Stripping UI")]
    [SerializeField] private Button dressButton;

    /// <summary>
    /// 「脱がす」モードにするボタン
    /// </summary>
    [SerializeField] private Button stripButton;

    /// <summary>
    /// キャラクターの上半身のクリック判定
    /// </summary>
    [SerializeField] private Button upperAreaButton;

    /// <summary>
    /// キャラクターの下半身のクリック判定
    /// </summary>
    [SerializeField] private Button lowerAreaButton;

    /// <summary>
    /// キャラクターのベース画像（画像1）
    /// </summary>
    [SerializeField] private RawImage characterRawImage; // ベース画像 (画像1)

    /// <summary>
    /// 上半身の奥側の服の画像（画像3）
    /// </summary>
    [SerializeField] private RawImage layerUpperBack;    // 画像3 (上半分・背面)

    /// <summary>
    /// 上半身の手前側の服の画像（画像2）
    /// </summary>
    [SerializeField] private RawImage layerUpperFront;   // 画像2 (上半分・前面)

    /// <summary>
    /// 下半身の奥側の服の画像（画像5）
    /// </summary>
    [SerializeField] private RawImage layerLowerBack;    // 画像5 (下半分・背面)

    /// <summary>
    /// 下半身の手前側の服の画像（画像4）
    /// </summary>
    [SerializeField] private RawImage layerLowerFront;   // 画像4 (下半分・前面)

    /// <summary>
    /// 表情を表示する RawImage（未設定ならベース画像を差し替えます）
    /// </summary>
    [Header("Face Expression UI")]
    [SerializeField] private RawImage characterFaceImage; // 表情画像 (顔部分の重ね合わせ、未設定の場合は characterRawImage を差し替える)

    /// <summary>
    /// 表情ごとの画像（0:通常, 1〜3:差分）
    /// </summary>
    [SerializeField] private Texture2D[] faceTextures;   // 表情差分テクスチャ (0:元の表情, 1:差分1, 2:差分2, 3:差分3)

    /// <summary>
    /// 開いている設定画面（開いていなければ null）
    /// </summary>
    private SettingsView _activeSettingsInstance;

    /// <summary>
    /// 開いている会話ウィンドウ（開いていなければ null）
    /// </summary>
    private TalkWindowView _activeTalkWindowInstance;

    /// <summary>
    /// ゲージを増やすボタンと、その増加量の組み合わせです。
    /// </summary>
    [Serializable]
    public struct GaugeIncreaseData
    {
        /// <summary>
        /// ゲージを増やすボタン
        /// </summary>
        public Button button;

        /// <summary>
        /// ボタンを押したときのゲージの増加量
        /// </summary>
        public float increaseAmount;
    }

    /// <summary>
    /// 挿入ボタン
    /// </summary>
    [Header("Order UI")]
    [SerializeField] private Button orderPenetrationButton;

    /// <summary>
    /// 挿入ボタンの表示テキスト
    /// </summary>
    [SerializeField] private TextMeshProUGUI orderPenetrationText;

    /// <summary>
    /// フェラボタン
    /// </summary>
    [SerializeField] private Button orderBlowjobButton;

    /// <summary>
    /// フェラボタンの表示テキスト
    /// </summary>
    [SerializeField] private TextMeshProUGUI orderBlowjobText;

    /// <summary>
    /// 挿入中にベース画像で再生するアニメーションの画像
    /// </summary>
    [Header("Order Animations")]
    [SerializeField] private Texture2D[] penetrationTextures; // 挿入時のアニメーション画像

    /// <summary>
    /// 実行中の挿入アニメーション（未実行なら null）
    /// </summary>
    private Coroutine _penetrationAnimationCoroutine;

    /// <summary>
    /// ベース画像の初期テクスチャ（アニメーション終了後に戻すため）
    /// </summary>
    private Texture _defaultCharacterTexture;

    /// <summary>
    /// 実行中のループアニメーション（未実行なら null）
    /// </summary>
    private Coroutine _activeLoopCoroutine;

    /// <summary>
    /// 再生中のアニメーションの番号（再生していなければ -1）
    /// </summary>
    private int _activeAnimationIndex = -1;

    /// <summary>
    /// アニメーションごとの表示中の画像の番号
    /// </summary>
    private int[] _currentAnimationIndices;

    /// <summary>
    /// 挿入中かどうか（View 側で保持している状態）
    /// </summary>
    private bool _isPenetrationActiveView;

    /// <summary>
    /// フェラ中かどうか（View 側で保持している状態）
    /// </summary>
    private bool _isBlowjobActiveView;

    /// <summary>
    /// ホバー時にポップアップを表示する設定です。
    /// </summary>
    [Serializable]
    public struct HoverPopupData
    {
        /// <summary>
        /// ホバーを検知する UI
        /// </summary>
        [Tooltip("ホバーを検知する対象のUI（ボタンなど）")]
        public GameObject targetArea;

        /// <summary>
        /// 表示・非表示を切り替えるポップアップ
        /// </summary>
        [Tooltip("表示・非表示を切り替えるポップアップのGameObject")]
        public GameObject popupObject;
    }

    /// <summary>
    /// ホバー時のポップアップ設定
    /// </summary>
    [Header("Hover Popup UI")]
    [SerializeField] private HoverPopupData[] hoverPopups;

    /// <summary>
    /// 興奮度（Gauge1）のゲージの中身の画像
    /// </summary>
    [Header("Gauge UI")]
    [SerializeField] private Image gaugeInside1;

    /// <summary>
    /// 射精感（Gauge2）のゲージの中身の画像
    /// </summary>
    [SerializeField] private Image gaugeInside2;

    /// <summary>
    /// おさわりパネル（配下のボタンが興奮度を増やします）
    /// </summary>
    [SerializeField] private Transform contOpeTransform;

    /// <summary>
    /// おさわりボタン 1 回あたりの興奮度の増加量（個別設定がないボタン用）
    /// </summary>
    [SerializeField] private float defaultGaugeIncreaseAmount = 10f;

    /// <summary>
    /// 興奮度を増やすボタンの個別設定（増加量を変えたいボタン用）
    /// </summary>
    [SerializeField] private GaugeIncreaseData[] customGaugeIncreaseButtons;

    /// <summary>
    /// 射精感を増やすボタンと増加量の設定
    /// </summary>
    [SerializeField] private GaugeIncreaseData[] customGauge2IncreaseButtons;

    /// <summary>
    /// 興奮度を増やすボタンが押されたときに発火するイベント（引数は増加量）
    /// </summary>
    public event Action<float> OnOpeButtonClicked;

    /// <summary>
    /// 射精感を増やすボタンが押されたときに発火するイベント（引数は増加量）
    /// </summary>
    public event Action<float> OnGauge2IncreaseButtonClicked;

    /// <summary>
    /// ポップアップ対象にカーソルが乗ったときに発火するイベント（引数は番号）
    /// </summary>
    public event Action<int> OnPopupHoverEntered;

    /// <summary>
    /// ポップアップ対象からカーソルが外れたときに発火するイベント（引数は番号）
    /// </summary>
    public event Action<int> OnPopupHoverExited;

    // Presenterが登録するコールバック
    /// <summary>
    /// 設定ボタンが押されたときに発火するイベント
    /// </summary>
    public event Action OnSettingsClicked;

    /// <summary>
    /// タイトルボタンが押されたときに発火するイベント
    /// </summary>
    public event Action OnTitleClicked;

    /// <summary>
    /// エンディングボタンが押されたときに発火するイベント
    /// </summary>
    public event Action OnEndingClicked;

    /// <summary>
    /// キャンセルボタンが押されたときに発火するイベント
    /// </summary>
    public event Action OnCancelClicked;

    /// <summary>
    /// 会話ボタンが押されたときに発火するイベント（引数は会話の番号）
    /// </summary>
    public event Action<int> OnTalkButtonClicked;

    /// <summary>
    /// 会話ウィンドウがクリックされたときに発火するイベント
    /// </summary>
    public event Action OnTalkWindowClicked;

    /// <summary>
    /// オート会話トグルが切り替えられたときに発火するイベント
    /// </summary>
    public event Action<bool> OnAutoTalkToggleChanged;

    /// <summary>
    /// 「着せる」ボタンが押されたときに発火するイベント
    /// </summary>
    public event Action OnDressClicked;

    /// <summary>
    /// 「脱がす」ボタンが押されたときに発火するイベント
    /// </summary>
    public event Action OnStripClicked;

    /// <summary>
    /// 上半身がクリックされたときに発火するイベント
    /// </summary>
    public event Action OnUpperAreaClicked;

    /// <summary>
    /// 下半身がクリックされたときに発火するイベント
    /// </summary>
    public event Action OnLowerAreaClicked;

    /// <summary>
    /// アニメーションボタンが押されたときに発火するイベント（引数はアニメーションの番号）
    /// </summary>
    public event Action<int> OnLoopAnimationButtonClicked;

    /// <summary>
    /// 挿入ボタンが押されたときに発火するイベント
    /// </summary>
    public event Action OnOrderPenetrationClicked;

    /// <summary>
    /// フェラボタンが押されたときに発火するイベント
    /// </summary>
    public event Action OnOrderBlowjobClicked;
    
    // 設定画面が生成されたときにPresenterに通知するコールバック
    /// <summary>
    /// 設定画面を生成したときに発火するイベント
    /// </summary>
    public event Action<SettingsView> OnSettingsOpened;

    /// <summary>
    /// 未設定のゲージ・ボタンをシーンから探して補い、ゲージ画像を横方向の塗りつぶし表示に設定します。
    /// </summary>
    private void Awake()
    {
        // 自動アタッチのフォールバック
        if (gaugeInside1 == null)
        {
            var go = GameObject.Find("Gauge_Inside1");
            if (go != null) gaugeInside1 = go.GetComponent<Image>();
        }
        if (gaugeInside2 == null)
        {
            var go = GameObject.Find("Gauge_Inside2");
            if (go != null) gaugeInside2 = go.GetComponent<Image>();
        }
        if (contOpeTransform == null)
        {
            var go = GameObject.Find("Cont_ope");
            if (go != null) contOpeTransform = go.transform;
        }

        if (endingButton == null)
        {
            var go = GameObject.Find("Ending_Button");
            if (go != null) endingButton = go.GetComponent<Button>();
        }

        // ゲージの初期設定の自動適用（参考サイトの設定）
        SetupGaugeImage(gaugeInside1);
        SetupGaugeImage(gaugeInside2);
    }

    /// <summary>
    /// ゲージ画像を左から横方向に塗りつぶす表示に設定します。
    /// </summary>
    private static void SetupGaugeImage(Image img)
    {
        if (img != null)
        {
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            img.fillOrigin = (int)Image.OriginHorizontal.Left;
            Debug.Log($"[MainView] ゲージImage設定自動適用: {img.gameObject.name}");
        }
    }

    /// <summary>
    /// 必須の参照を確認し、ベース画像の初期テクスチャを保存して、オート会話トグルの初期値を通知します。
    /// </summary>
    private void Start()
    {
        ValidateAttachments();

        // ベース画像の初期テクスチャを保存しておく（アニメーション終了後の復元用）
        if (characterRawImage != null)
        {
            _defaultCharacterTexture = characterRawImage.texture;
        }

        // UIのToggle初期値をイベントとして通知し、Modelと同期させる
        if (autoTalkToggle != null)
        {
            OnAutoTalkToggleChanged?.Invoke(autoTalkToggle.isOn);
        }
    }

    /// <summary>
    /// 服装まわりの必須の参照が設定されているか確認し、未設定ならエラーを出します。
    /// </summary>
    private void ValidateAttachments()
    {
        if (dressButton == null) Debug.LogError("[MainView] dressButton がアタッチされていません。");
        if (stripButton == null) Debug.LogError("[MainView] stripButton がアタッチされていません。");
        if (upperAreaButton == null) Debug.LogError("[MainView] upperAreaButton がアタッチされていません。");
        if (lowerAreaButton == null) Debug.LogError("[MainView] lowerAreaButton がアタッチされていません。");
        if (layerUpperBack == null) Debug.LogError("[MainView] layerUpperBack (画像3) がアタッチされていません。");
        if (layerUpperFront == null) Debug.LogError("[MainView] layerUpperFront (画像2) がアタッチされていません。");
        if (layerLowerBack == null) Debug.LogError("[MainView] layerLowerBack (画像5) がアタッチされていません。");
        if (layerLowerFront == null) Debug.LogError("[MainView] layerLowerFront (画像4) がアタッチされていません。");
    }

    /// <summary>
    /// 有効化時に各ボタン・トグル・ホバー判定のイベントを購読します。
    /// </summary>
    private void OnEnable()
    {
        RegisterContOpeButtons();
        if (settingsButton != null) settingsButton.onClick.AddListener(HandleSettingsClicked);
        if (titleButton != null) titleButton.onClick.AddListener(HandleTitleClicked);
        if (endingButton != null) endingButton.onClick.AddListener(HandleEndingClicked);
        if (cancelButton != null) cancelButton.onClick.AddListener(HandleCancelClicked);
        if (dressButton != null) dressButton.onClick.AddListener(HandleDressClicked);
        if (stripButton != null) stripButton.onClick.AddListener(HandleStripClicked);
        if (upperAreaButton != null) upperAreaButton.onClick.AddListener(HandleUpperAreaClicked);
        if (lowerAreaButton != null) lowerAreaButton.onClick.AddListener(HandleLowerAreaClicked);
        if (orderPenetrationButton != null) orderPenetrationButton.onClick.AddListener(() => OnOrderPenetrationClicked?.Invoke());
        if (orderBlowjobButton != null) orderBlowjobButton.onClick.AddListener(() => OnOrderBlowjobClicked?.Invoke());
        
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

        if (autoTalkToggle != null)
        {
            autoTalkToggle.onValueChanged.AddListener(isOn => OnAutoTalkToggleChanged?.Invoke(isOn));
        }

        if (loopAnimations != null)
        {
            for (int i = 0; i < loopAnimations.Length; i++)
            {
                int index = i;
                if (loopAnimations[i].button != null)
                {
                    loopAnimations[i].button.onClick.AddListener(() => OnLoopAnimationButtonClicked?.Invoke(index));
                }
            }
        }

        if (hoverPopups != null)
        {
            for (int i = 0; i < hoverPopups.Length; i++)
            {
                if (hoverPopups[i].targetArea != null)
                {
                    var forwarder = hoverPopups[i].targetArea.GetComponent<PointerEventForwarder>();
                    if (forwarder == null) forwarder = hoverPopups[i].targetArea.AddComponent<PointerEventForwarder>();
                    forwarder.Index = i;
                    forwarder.OnEnter += HandlePopupHoverEntered;
                    forwarder.OnExit += HandlePopupHoverExited;
                }
            }
        }

        if (subScreenToggles != null)
        {
            foreach (var toggle in subScreenToggles)
            {
                if (toggle.triggerButton != null)
                {
                    var captureToggle = toggle;
                    captureToggle.triggerButton.onClick.AddListener(() =>
                    {
                        // C#側の状態(OFF)とInspectorのイベント(ON)の競合を防ぐため、やめる状態の時はブロックする
                        if (captureToggle.triggerButton == orderPenetrationButton && !_isPenetrationActiveView) return;
                        if (captureToggle.triggerButton == orderBlowjobButton && !_isBlowjobActiveView) return;

                        if (captureToggle.targetToEnable != null) captureToggle.targetToEnable.enabled = true;
                        if (captureToggle.targetToDisable != null) captureToggle.targetToDisable.enabled = false;
                        Debug.Log($"[MainView] サブスクリーンの強制トグルを実行: {captureToggle.targetToEnable?.gameObject.name} をON, {captureToggle.targetToDisable?.gameObject.name} をOFF");
                    });
                }
            }
        }
    }

    /// <summary>
    /// 無効化時に各ボタン・トグル・ホバー判定のイベントの購読を解除します。
    /// </summary>
    private void OnDisable()
    {
        UnregisterContOpeButtons();
        if (settingsButton != null) settingsButton.onClick.RemoveListener(HandleSettingsClicked);
        if (titleButton != null) titleButton.onClick.RemoveListener(HandleTitleClicked);
        if (endingButton != null) endingButton.onClick.RemoveListener(HandleEndingClicked);
        if (cancelButton != null) cancelButton.onClick.RemoveListener(HandleCancelClicked);
        if (dressButton != null) dressButton.onClick.RemoveListener(HandleDressClicked);
        if (stripButton != null) stripButton.onClick.RemoveListener(HandleStripClicked);
        if (upperAreaButton != null) upperAreaButton.onClick.RemoveListener(HandleUpperAreaClicked);
        if (lowerAreaButton != null) lowerAreaButton.onClick.RemoveListener(HandleLowerAreaClicked);
        if (orderPenetrationButton != null) orderPenetrationButton.onClick.RemoveAllListeners();
        if (orderBlowjobButton != null) orderBlowjobButton.onClick.RemoveAllListeners();
        
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

        if (autoTalkToggle != null)
        {
            autoTalkToggle.onValueChanged.RemoveAllListeners();
        }

        if (loopAnimations != null)
        {
            for (int i = 0; i < loopAnimations.Length; i++)
            {
                if (loopAnimations[i].button != null)
                {
                    loopAnimations[i].button.onClick.RemoveAllListeners();
                }
            }
        }

        if (subScreenToggles != null)
        {
            foreach (var toggle in subScreenToggles)
            {
                if (toggle.triggerButton != null)
                {
                    toggle.triggerButton.onClick.RemoveAllListeners();
                }
            }
        }

        if (hoverPopups != null)
        {
            for (int i = 0; i < hoverPopups.Length; i++)
            {
                if (hoverPopups[i].targetArea != null)
                {
                    var forwarder = hoverPopups[i].targetArea.GetComponent<PointerEventForwarder>();
                    if (forwarder != null)
                    {
                        forwarder.OnEnter -= HandlePopupHoverEntered;
                        forwarder.OnExit -= HandlePopupHoverExited;
                    }
                }
            }
        }
    }

    /// <summary>
    /// ポップアップ対象にカーソルが乗ったことを通知します。
    /// </summary>
    private void HandlePopupHoverEntered(int index) => OnPopupHoverEntered?.Invoke(index);

    /// <summary>
    /// ポップアップ対象からカーソルが外れたことを通知します。
    /// </summary>
    private void HandlePopupHoverExited(int index) => OnPopupHoverExited?.Invoke(index);

    /// <summary>
    /// 設定ボタンが押されたことを通知します。
    /// </summary>
    private void HandleSettingsClicked() => OnSettingsClicked?.Invoke();

    /// <summary>
    /// タイトルボタンが押されたことを通知します。
    /// </summary>
    private void HandleTitleClicked() => OnTitleClicked?.Invoke();

    /// <summary>
    /// エンディングボタンが押されたことを通知します。
    /// </summary>
    private void HandleEndingClicked() => OnEndingClicked?.Invoke();

    /// <summary>
    /// キャンセルボタンが押されたことを通知します。
    /// </summary>
    private void HandleCancelClicked() => OnCancelClicked?.Invoke();

    /// <summary>
    /// 会話ウィンドウがクリックされたことを通知します。
    /// </summary>
    private void HandleTalkWindowClicked() => OnTalkWindowClicked?.Invoke();

    /// <summary>
    /// 設定画面を開いているかどうか
    /// </summary>
    public bool IsSettingsPanelActive => _activeSettingsInstance != null;

    /// <summary>
    /// 会話ウィンドウを開いているかどうか
    /// </summary>
    public bool IsTalkWindowActive => _activeTalkWindowInstance != null;

    /// <summary>
    /// 設定画面を生成して開きます（すでに開いていれば何もしません）。
    /// </summary>
    public void OpenSettings()
    {
        if (_activeSettingsInstance != null) return;

        if (settingsPrefab != null && canvasTransform != null)
        {
            _activeSettingsInstance = Instantiate(settingsPrefab, canvasTransform);
            OnSettingsOpened?.Invoke(_activeSettingsInstance);
        }
        else
        {
            Debug.LogError("[MainView] settingsPrefab または canvasTransform がアタッチされていません。");
        }
    }

    /// <summary>
    /// 設定画面を破棄して閉じます。
    /// </summary>
    public void CloseSettings()
    {
        if (_activeSettingsInstance != null)
        {
            Destroy(_activeSettingsInstance.gameObject);
            _activeSettingsInstance = null;
        }
    }

    /// <summary>
    /// 会話ウィンドウを生成し、最初のセリフを表示します（すでに開いていれば何もしません）。
    /// </summary>
    public void OpenTalkWindow(string initialText)
    {
        if (_activeTalkWindowInstance != null) return;

        if (talkWindowPrefab != null && canvasTransform != null)
        {
            _activeTalkWindowInstance = Instantiate(talkWindowPrefab, canvasTransform);
            _activeTalkWindowInstance.SetText(initialText);
            _activeTalkWindowInstance.OnWindowClicked += HandleTalkWindowClicked;
        }
        else
        {
            Debug.LogError("[MainView] talkWindowPrefab または canvasTransform がアタッチされていません。");
        }
    }

    /// <summary>
    /// 会話ウィンドウのセリフを更新します。
    /// </summary>
    public void UpdateTalkText(string text)
    {
        if (_activeTalkWindowInstance != null)
        {
            _activeTalkWindowInstance.SetText(text);
        }
    }

    /// <summary>
    /// 会話ウィンドウを破棄して閉じます。
    /// </summary>
    public void CloseTalkWindow()
    {
        if (_activeTalkWindowInstance != null)
        {
            _activeTalkWindowInstance.OnWindowClicked -= HandleTalkWindowClicked;
            Destroy(_activeTalkWindowInstance.gameObject);
            _activeTalkWindowInstance = null;
        }
    }

    /// <summary>
    /// 各会話ボタンの解放条件
    /// </summary>
    public TalkButtonCondition[] TalkButtonConditions => talkButtonConditions;

    /// <summary>
    /// 指定した会話ボタンを押せる状態・押せない状態にします。
    /// </summary>
    public void SetTalkButtonInteractable(int index, bool isInteractable)
    {
        if (talkButtons != null && index >= 0 && index < talkButtons.Length && talkButtons[index] != null)
        {
            talkButtons[index].interactable = isInteractable;
        }
    }

    /// <summary>
    /// 「着せる」ボタンが押されたことを通知します。
    /// </summary>
    private void HandleDressClicked() => OnDressClicked?.Invoke();

    /// <summary>
    /// 「脱がす」ボタンが押されたことを通知します。
    /// </summary>
    private void HandleStripClicked() => OnStripClicked?.Invoke();

    /// <summary>
    /// 上半身がクリックされたことを通知します。
    /// </summary>
    private void HandleUpperAreaClicked() => OnUpperAreaClicked?.Invoke();

    /// <summary>
    /// 下半身がクリックされたことを通知します。
    /// </summary>
    private void HandleLowerAreaClicked() => OnLowerAreaClicked?.Invoke();

    /// <summary>
    /// 服のレイヤーごとに表示・非表示を切り替えます。
    /// </summary>
    public void UpdateOutfitVisibility(bool upperBack, bool upperFront, bool lowerBack, bool lowerFront)
    {
        if (layerUpperBack != null) layerUpperBack.gameObject.SetActive(upperBack);
        if (layerUpperFront != null) layerUpperFront.gameObject.SetActive(upperFront);
        if (layerLowerBack != null) layerLowerBack.gameObject.SetActive(lowerBack);
        if (layerLowerFront != null) layerLowerFront.gameObject.SetActive(lowerFront);
    }

    /// <summary>
    /// 操作モードに合わせて、クリック判定と「着せる」「脱がす」ボタンの押せる状態を切り替えます。
    /// </summary>
    public void SetControlModeUI(OutfitControlMode mode)
    {
        bool isEnabled = mode != OutfitControlMode.None;
        if (upperAreaButton != null) upperAreaButton.interactable = isEnabled;
        if (lowerAreaButton != null) lowerAreaButton.interactable = isEnabled;

        // 現在選択されているモードのボタンを無効化（視覚的なフィードバック）
        if (dressButton != null) dressButton.interactable = (mode != OutfitControlMode.Dress);
        if (stripButton != null) stripButton.interactable = (mode != OutfitControlMode.Strip);

        Debug.Log($"[MainView] UI状態更新 - 操作有効: {isEnabled}, モード: {mode}");
    }

    /// <summary>
    /// アニメーションごとの画像番号の配列を、まだなければ作成します。
    /// </summary>
    private void InitializeAnimationIndicesIfNeeded()
    {
        if (_currentAnimationIndices == null && loopAnimations != null)
        {
            _currentAnimationIndices = new int[loopAnimations.Length];
        }
    }

    /// <summary>
    /// 指定したアニメーションを再生します。同じボタンをもう一度押すと、ループなしならコマ送りします。
    /// </summary>
    public void PlayLoopAnimation(int index)
    {
        InitializeAnimationIndicesIfNeeded();

        if (loopAnimations == null || index < 0 || index >= loopAnimations.Length) return;
        var animData = loopAnimations[index];
        if (animData.screenAnimations == null || animData.screenAnimations.Length == 0) return;

        // C#側の状態(OFF)とInspectorのイベント(ON)の競合を防ぐため、やめる状態の時は再生をブロックする
        if (animData.button == orderPenetrationButton && !_isPenetrationActiveView) return;
        if (animData.button == orderBlowjobButton && !_isBlowjobActiveView) return;

        // 別のボタンが押された（または新規起動）の場合
        if (index != _activeAnimationIndex)
        {
            StopLoopAnimation(); // 現在の再生を停止し非表示にする
            _activeAnimationIndex = index;
            _currentAnimationIndices[index] = 0; // 1枚目から開始

            foreach (var sa in animData.screenAnimations)
            {
                RawImage img = sa.targetScreen != null ? sa.targetScreen : (animationRawImage != null ? animationRawImage : characterRawImage);
                if (img != null && sa.textures != null && sa.textures.Length > 0)
                {
                    img.enabled = true; // GameObjectのActive切り替えから、RawImageのenabled切り替えに変更
                    img.texture = sa.textures[0];
                }
            }

            if (animData.isLoop)
            {
                // ループアニメーションなら自動再生を開始
                _activeLoopCoroutine = StartCoroutine(CoLoopAnimation(animData.screenAnimations));
            }
        }
        else
        {
            // 同じボタンが再度押された場合
            if (!animData.isLoop)
            {
                // ループなしの場合は、手動で次の画像に進める（コマ送り）
                int nextIndex = _currentAnimationIndices[index] + 1;
                _currentAnimationIndices[index] = nextIndex;
                
                foreach (var sa in animData.screenAnimations)
                {
                    RawImage img = sa.targetScreen != null ? sa.targetScreen : (animationRawImage != null ? animationRawImage : characterRawImage);
                    if (img != null && sa.textures != null && sa.textures.Length > 0)
                    {
                        img.texture = sa.textures[nextIndex % sa.textures.Length];
                    }
                }
            }
            // ループありの場合は、既に自動再生コルーチンが走っているので何もしない
        }
    }

    /// <summary>
    /// animationFrameRate 秒ごとに画像を切り替えてアニメーションをループ再生します。
    /// </summary>
    private IEnumerator CoLoopAnimation(ScreenAnimationData[] screenAnimations)
    {
        int frameIndex = 0;
        while (true)
        {
            yield return new WaitForSeconds(animationFrameRate);
            frameIndex++;
            
            if (screenAnimations != null)
            {
                foreach (var sa in screenAnimations)
                {
                    RawImage img = sa.targetScreen != null ? sa.targetScreen : (animationRawImage != null ? animationRawImage : characterRawImage);
                    if (img != null && sa.textures != null && sa.textures.Length > 0)
                    {
                        img.texture = sa.textures[frameIndex % sa.textures.Length];
                    }
                }
            }
            
            if (_activeAnimationIndex >= 0 && _currentAnimationIndices != null && _activeAnimationIndex < _currentAnimationIndices.Length)
            {
                _currentAnimationIndices[_activeAnimationIndex] = frameIndex;
            }
        }
    }

    /// <summary>
    /// ループアニメーションを止め、アニメーション用のスクリーンをすべて非表示にします。
    /// </summary>
    public void StopLoopAnimation()
    {
        if (_activeLoopCoroutine != null)
        {
            StopCoroutine(_activeLoopCoroutine);
            _activeLoopCoroutine = null;
        }

        // 明示的に登録されたサブスクリーンをすべて確実に非表示（排他表示）にする
        if (managedSubScreens != null)
        {
            foreach (var screen in managedSubScreens)
            {
                if (screen != null) screen.enabled = false;
            }
        }

        // すべてのアニメーションスクリーンを一旦確実に非表示にする
        if (loopAnimations != null)
        {
            foreach (var animData in loopAnimations)
            {
                if (animData.screenAnimations != null)
                {
                    foreach (var sa in animData.screenAnimations)
                    {
                        if (sa.targetScreen != null) sa.targetScreen.enabled = false;
                    }
                }
            }
        }

        _activeAnimationIndex = -1; // アクティブ状態をリセット

        // 強制トグルで表示されたサブスクリーンも非表示にする
        if (subScreenToggles != null)
        {
            foreach (var toggle in subScreenToggles)
            {
                if (toggle.targetToEnable != null) toggle.targetToEnable.enabled = false;
            }
        }

        // アニメーション専用のRawImageがある場合のみ非表示にする
        if (animationRawImage != null)
        {
            animationRawImage.enabled = false;
        }
    }

    /// <summary>
    /// 挿入時(Order_Penetration)のアニメーション再生・停止を制御します。
    /// </summary>
    public void PlayPenetrationAnimation(bool isPlay)
    {
        if (isPlay)
        {
            if (_penetrationAnimationCoroutine == null && penetrationTextures != null && penetrationTextures.Length > 0)
            {
                // 他のループアニメーションと競合しないよう停止させる
                StopLoopAnimation();
                _penetrationAnimationCoroutine = StartCoroutine(CoPenetrationAnimation());
                Debug.Log("[MainView] 挿入アニメーションの再生を開始します。");
            }
        }
        else
        {
            if (_penetrationAnimationCoroutine != null)
            {
                StopCoroutine(_penetrationAnimationCoroutine);
                _penetrationAnimationCoroutine = null;
                Debug.Log("[MainView] 挿入アニメーションを停止しました。");
            }

            // 元のベース画像に戻す
            if (characterRawImage != null && _defaultCharacterTexture != null)
            {
                characterRawImage.texture = _defaultCharacterTexture;
            }
        }
    }

    /// <summary>
    /// 挿入中のアニメーションをベース画像でループ再生します。
    /// </summary>
    private IEnumerator CoPenetrationAnimation()
    {
        int frameIndex = 0;
        while (true)
        {
            if (characterRawImage != null && penetrationTextures != null && penetrationTextures.Length > 0)
            {
                characterRawImage.texture = penetrationTextures[frameIndex % penetrationTextures.Length];
            }
            yield return new WaitForSeconds(animationFrameRate);
            frameIndex++;
        }
    }

    /// <summary>
    /// おさわりパネルのボタンと個別設定のボタンに、ゲージを増やすイベントを登録します。
    /// </summary>
    private void RegisterContOpeButtons()
    {
        if (contOpeTransform != null)
        {
            Button[] buttons = contOpeTransform.GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                float amount = defaultGaugeIncreaseAmount;
                if (customGaugeIncreaseButtons != null)
                {
                    foreach (var customData in customGaugeIncreaseButtons)
                    {
                        if (customData.button == btn)
                        {
                            amount = customData.increaseAmount;
                            break;
                        }
                    }
                }
                var currentAmount = amount;
                btn.onClick.AddListener(() => OnOpeButtonClicked?.Invoke(currentAmount));
            }
            Debug.Log($"[MainView] Cont_ope内の {buttons.Length} 個のボタンにゲージ増加イベントを登録しました。");
        }

        if (customGaugeIncreaseButtons != null)
        {
            foreach (var customData in customGaugeIncreaseButtons)
            {
                if (customData.button != null && (contOpeTransform == null || !customData.button.transform.IsChildOf(contOpeTransform)))
                {
                    var currentAmount = customData.increaseAmount;
                    customData.button.onClick.AddListener(() => OnOpeButtonClicked?.Invoke(currentAmount));
                }
            }
        }

        if (customGauge2IncreaseButtons != null)
        {
            foreach (var customData in customGauge2IncreaseButtons)
            {
                if (customData.button != null)
                {
                    var currentAmount = customData.increaseAmount;
                    customData.button.onClick.AddListener(() => OnGauge2IncreaseButtonClicked?.Invoke(currentAmount));
                }
            }
        }
    }

    /// <summary>
    /// ゲージを増やすボタンのイベントをすべて解除します。
    /// </summary>
    private void UnregisterContOpeButtons()
    {
        if (contOpeTransform != null)
        {
            Button[] buttons = contOpeTransform.GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                btn.onClick.RemoveAllListeners();
            }
        }

        if (customGaugeIncreaseButtons != null)
        {
            foreach (var customData in customGaugeIncreaseButtons)
            {
                if (customData.button != null)
                {
                    customData.button.onClick.RemoveAllListeners();
                }
            }
        }

        if (customGauge2IncreaseButtons != null)
        {
            foreach (var customData in customGauge2IncreaseButtons)
            {
                if (customData.button != null)
                {
                    customData.button.onClick.RemoveAllListeners();
                }
            }
        }
    }

    /// <summary>
    /// ゲージの塗りつぶし量（0〜1）を更新します。
    /// </summary>
    public void UpdateGaugeFill(float gauge1FillAmount, float gauge2FillAmount)
    {
        if (gaugeInside1 != null)
        {
            gaugeInside1.fillAmount = gauge1FillAmount;
        }
        if (gaugeInside2 != null)
        {
            gaugeInside2.fillAmount = gauge2FillAmount;
        }
        Debug.Log($"[MainView] ゲージ表示更新 - Gauge1: {gauge1FillAmount}, Gauge2: {gauge2FillAmount}");
    }

    /// <summary>
    /// Order_Penetrationボタン（挿入ボタン）の有効/無効を切り替えます
    /// </summary>
    /// <param name="isInteractable">有効にする場合は true</param>
    public void SetOrderPenetrationInteractable(bool isInteractable)
    {
        if (orderPenetrationButton != null)
        {
            orderPenetrationButton.interactable = isInteractable;
        }
    }

    /// <summary>
    /// Order_Blowjobボタン（フェラチオボタン）の有効/無効を切り替えます
    /// </summary>
    public void SetOrderBlowjobInteractable(bool isInteractable)
    {
        if (orderBlowjobButton != null)
        {
            orderBlowjobButton.interactable = isInteractable;
        }
    }

    /// <summary>
    /// オーダーボタンのテキスト表示を更新します
    /// </summary>
    public void UpdateOrderButtonText(bool isPenetrationActive, bool isBlowjobActive)
    {
        _isPenetrationActiveView = isPenetrationActive;
        _isBlowjobActiveView = isBlowjobActive;

        if (orderPenetrationText != null)
        {
            orderPenetrationText.text = isPenetrationActive ? "やめる" : "挿入";
        }
        if (orderBlowjobText != null)
        {
            orderBlowjobText.text = isBlowjobActive ? "やめる" : "フェラチオ";
        }
    }

    /// <summary>
    /// ポップアップの表示・非表示を切り替えます
    /// </summary>
    public void ShowHoverPopup(int index, bool isVisible)
    {
        if (hoverPopups != null && index >= 0 && index < hoverPopups.Length)
        {
            if (hoverPopups[index].popupObject != null)
            {
                hoverPopups[index].popupObject.SetActive(isVisible);
                Debug.Log($"[MainView] ポップアップ[{index}]を {(isVisible ? "表示" : "非表示")} にしました。");
            }
        }
    }

    /// <summary>
    /// キャラクターの表情を指定した番号の画像に切り替えます。
    /// </summary>
    public void UpdateFace(int faceIndex)
    {
        if (faceTextures == null || faceTextures.Length == 0)
        {
            Debug.LogWarning("[MainView] 表情テクスチャ(faceTextures)が設定されていません。");
            return;
        }

        if (faceIndex < 0 || faceIndex >= faceTextures.Length)
        {
            Debug.LogWarning($"[MainView] 表情インデックスが範囲外です: {faceIndex} (配列数: {faceTextures.Length})");
            return;
        }

        Texture2D nextTexture = faceTextures[faceIndex];
        if (nextTexture == null)
        {
            Debug.LogWarning($"[MainView] 表情インデックス {faceIndex} のテクスチャが null です。");
            return;
        }

        // 表情表示用RawImageがあればそれを更新、なければベース画像を更新
        RawImage targetImage = (characterFaceImage != null) ? characterFaceImage : characterRawImage;
        if (targetImage != null)
        {
            // 非表示状態（Inspectorでのチェック外れなど）を考慮し、明示的に表示をオンにする
            targetImage.gameObject.SetActive(true);
            targetImage.enabled = true;

            targetImage.texture = nextTexture;
            Debug.Log($"[MainView] 表情更新: インデックス {faceIndex} (適用先: {targetImage.gameObject.name})");
        }
        else
        {
            Debug.LogWarning("[MainView] 表情適用先(characterFaceImage/characterRawImage)がありません。");
        }
    }
}
