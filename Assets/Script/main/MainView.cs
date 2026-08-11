using UnityEngine;
using UnityEngine.UI;
using System;

public class MainView : ViewBase
{
    [Header("Settings UI")]
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button titleButton;
    [SerializeField] private Button endingButton;

    [Header("Cancel UI")]
    [SerializeField] private Button cancelButton;

    [Header("Talk UI")]
    [SerializeField] private UnityEngine.UI.Toggle autoTalkToggle;
    [SerializeField] private Button[] talkButtons;
    [SerializeField] private TalkWindowView talkWindowPrefab;

    [Header("Settings Prefab UI")]
    [SerializeField] private SettingsView settingsPrefab;
    [SerializeField] private RectTransform canvasTransform;

    [System.Serializable]
    public struct ScreenAnimationData
    {
        public RawImage targetScreen; // 表示先スクリーン
        public Texture2D[] textures; // そのスクリーンで再生する画像
    }

    [System.Serializable]
    public struct LoopAnimationData
    {
        public Button button;
        public ScreenAnimationData[] screenAnimations; // スクリーンと画像のペア
        public bool isLoop; // ループ再生するかどうか
    }

    [Header("Loop Animation UI")]
    [SerializeField] private RawImage animationRawImage;
    [SerializeField] private RawImage[] managedSubScreens; // 排他表示制御するためのサブスクリーン一覧
    [SerializeField] private LoopAnimationData[] loopAnimations;
    [SerializeField] private float animationFrameRate = 0.5f;

    [System.Serializable]
    public struct SubScreenToggleData
    {
        public Button triggerButton;     // 押下するボタン
        public RawImage targetToEnable;  // オン(チェックを入れる)にするRawImage
        public RawImage targetToDisable; // オフ(チェックを外す)にするRawImage
    }

    [Header("Exclusive Sub Screen Toggles")]
    [SerializeField] private SubScreenToggleData[] subScreenToggles; // ボタン押下時の強制トグル設定

    [Header("Dressing/Stripping UI")]
    [SerializeField] private Button dressButton;
    [SerializeField] private Button stripButton;
    [SerializeField] private Button upperAreaButton;
    [SerializeField] private Button lowerAreaButton;
    [SerializeField] private RawImage characterRawImage; // ベース画像 (画像1)
    [SerializeField] private RawImage layerUpperBack;    // 画像3 (上半分・背面)
    [SerializeField] private RawImage layerUpperFront;   // 画像2 (上半分・前面)
    [SerializeField] private RawImage layerLowerBack;    // 画像5 (下半分・背面)
    [SerializeField] private RawImage layerLowerFront;   // 画像4 (下半分・前面)

    [Header("Face Expression UI")]
    [SerializeField] private RawImage characterFaceImage; // 表情画像 (顔部分の重ね合わせ、未設定の場合は characterRawImage を差し替える)
    [SerializeField] private Texture2D[] faceTextures;   // 表情差分テクスチャ (0:元の表情, 1:差分1, 2:差分2, 3:差分3)

    private SettingsView activeSettingsInstance;
    private TalkWindowView activeTalkWindowInstance;

    [System.Serializable]
    public struct GaugeIncreaseData
    {
        public Button button;
        public float increaseAmount;
    }

    [Header("Order Penetration UI")]
    [SerializeField] private Button orderPenetrationButton;

    [Header("Gauge UI")]
    [SerializeField] private Image gaugeInside1;
    [SerializeField] private Image gaugeInside2;
    [SerializeField] private Transform contOpeTransform;
    [SerializeField] private float defaultGaugeIncreaseAmount = 10f;
    [SerializeField] private GaugeIncreaseData[] customGaugeIncreaseButtons;

    public event Action<float> OnOpeButtonClicked;

    // Presenterが登録するコールバック
    public event Action OnSettingsClicked;
    public event Action OnTitleClicked;
    public event Action OnEndingClicked;
    public event Action OnCancelClicked;
    public event Action<int> OnTalkButtonClicked;
    public event Action OnTalkWindowClicked;
    public event Action<bool> OnAutoTalkToggleChanged;
    public event Action OnDressClicked;
    public event Action OnStripClicked;
    public event Action OnUpperAreaClicked;
    public event Action OnLowerAreaClicked;
    public event Action<int> OnLoopAnimationButtonClicked;
    
    // 設定画面が生成されたときにPresenterに通知するコールバック
    public event Action<SettingsView> OnSettingsOpened;

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

    private void SetupGaugeImage(Image img)
    {
        if (img != null)
        {
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            img.fillOrigin = (int)Image.OriginHorizontal.Left;
            Debug.Log($"[MainView] ゲージImage設定自動適用: {img.gameObject.name}");
        }
    }

    private void Start()
    {
        ValidateAttachments();

        // UIのToggle初期値をイベントとして通知し、Modelと同期させる
        if (autoTalkToggle != null)
        {
            OnAutoTalkToggleChanged?.Invoke(autoTalkToggle.isOn);
        }
    }

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

        if (subScreenToggles != null)
        {
            foreach (var toggle in subScreenToggles)
            {
                if (toggle.triggerButton != null)
                {
                    var captureToggle = toggle;
                    captureToggle.triggerButton.onClick.AddListener(() =>
                    {
                        if (captureToggle.targetToEnable != null) captureToggle.targetToEnable.enabled = true;
                        if (captureToggle.targetToDisable != null) captureToggle.targetToDisable.enabled = false;
                        Debug.Log($"[MainView] サブスクリーンの強制トグルを実行: {captureToggle.targetToEnable?.gameObject.name} をON, {captureToggle.targetToDisable?.gameObject.name} をOFF");
                    });
                }
            }
        }
    }

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
    }

    private void HandleSettingsClicked() => OnSettingsClicked?.Invoke();

    private void HandleTitleClicked() => OnTitleClicked?.Invoke();

    private void HandleEndingClicked() => OnEndingClicked?.Invoke();

    private void HandleCancelClicked() => OnCancelClicked?.Invoke();

    private void HandleTalkWindowClicked() => OnTalkWindowClicked?.Invoke();

    public bool IsSettingsPanelActive => activeSettingsInstance != null;
    public bool IsTalkWindowActive => activeTalkWindowInstance != null;

    public void OpenSettings()
    {
        if (activeSettingsInstance != null) return;

        if (settingsPrefab != null && canvasTransform != null)
        {
            activeSettingsInstance = Instantiate(settingsPrefab, canvasTransform);
            OnSettingsOpened?.Invoke(activeSettingsInstance);
        }
        else
        {
            Debug.LogError("[MainView] settingsPrefab または canvasTransform がアタッチされていません。");
        }
    }

    public void CloseSettings()
    {
        if (activeSettingsInstance != null)
        {
            Destroy(activeSettingsInstance.gameObject);
            activeSettingsInstance = null;
        }
    }

    public void OpenTalkWindow(string initialText)
    {
        if (activeTalkWindowInstance != null) return;

        if (talkWindowPrefab != null && canvasTransform != null)
        {
            activeTalkWindowInstance = Instantiate(talkWindowPrefab, canvasTransform);
            activeTalkWindowInstance.SetText(initialText);
            activeTalkWindowInstance.OnWindowClicked += HandleTalkWindowClicked;
        }
        else
        {
            Debug.LogError("[MainView] talkWindowPrefab または canvasTransform がアタッチされていません。");
        }
    }

    public void UpdateTalkText(string text)
    {
        if (activeTalkWindowInstance != null)
        {
            activeTalkWindowInstance.SetText(text);
        }
    }

    public void CloseTalkWindow()
    {
        if (activeTalkWindowInstance != null)
        {
            activeTalkWindowInstance.OnWindowClicked -= HandleTalkWindowClicked;
            Destroy(activeTalkWindowInstance.gameObject);
            activeTalkWindowInstance = null;
        }
    }

    private void HandleDressClicked() => OnDressClicked?.Invoke();
    private void HandleStripClicked() => OnStripClicked?.Invoke();
    private void HandleUpperAreaClicked() => OnUpperAreaClicked?.Invoke();
    private void HandleLowerAreaClicked() => OnLowerAreaClicked?.Invoke();

    public void UpdateOutfitVisibility(bool upperBack, bool upperFront, bool lowerBack, bool lowerFront)
    {
        if (layerUpperBack != null) layerUpperBack.gameObject.SetActive(upperBack);
        if (layerUpperFront != null) layerUpperFront.gameObject.SetActive(upperFront);
        if (layerLowerBack != null) layerLowerBack.gameObject.SetActive(lowerBack);
        if (layerLowerFront != null) layerLowerFront.gameObject.SetActive(lowerFront);
    }

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

    private Coroutine _activeLoopCoroutine;
    private int _activeAnimationIndex = -1; // 現在アクティブなアニメーションボタンのインデックス
    private int[] _currentAnimationIndices; // 各アニメーションごとの現在の再生画像インデックス

    private void InitializeAnimationIndicesIfNeeded()
    {
        if (_currentAnimationIndices == null && loopAnimations != null)
        {
            _currentAnimationIndices = new int[loopAnimations.Length];
        }
    }

    public void PlayLoopAnimation(int index)
    {
        InitializeAnimationIndicesIfNeeded();

        if (loopAnimations == null || index < 0 || index >= loopAnimations.Length) return;
        var animData = loopAnimations[index];
        if (animData.screenAnimations == null || animData.screenAnimations.Length == 0) return;

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

    private System.Collections.IEnumerator CoLoopAnimation(ScreenAnimationData[] screenAnimations)
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

        // アニメーション専用のRawImageがある場合のみ非表示にする
        if (animationRawImage != null)
        {
            animationRawImage.enabled = false;
        }
    }

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
    }

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
    }

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
            targetImage.texture = nextTexture;
            Debug.Log($"[MainView] 表情更新: インデックス {faceIndex} (適用先: {targetImage.gameObject.name})");
        }
        else
        {
            Debug.LogWarning("[MainView] 表情適用先(characterFaceImage/characterRawImage)がありません。");
        }
    }
}
