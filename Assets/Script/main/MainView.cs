using UnityEngine;
using UnityEngine.UI;
using System;

public class MainView : ViewBase
{
    [Header("Settings UI")]
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button titleButton;
    [SerializeField] private Button endingButton;

    [Header("Talk UI")]
    [SerializeField] private Button[] talkButtons;
    [SerializeField] private TalkWindowView talkWindowPrefab;

    [Header("Settings Prefab UI")]
    [SerializeField] private SettingsView settingsPrefab;
    [SerializeField] private RectTransform canvasTransform;

    [System.Serializable]
    public struct LoopAnimationData
    {
        public Button button;
        public Texture2D[] textures;
        public bool isLoop; // ループ再生するかどうか
    }

    [Header("Loop Animation UI")]
    [SerializeField] private RawImage animationRawImage;
    [SerializeField] private LoopAnimationData[] loopAnimations;
    [SerializeField] private float animationFrameRate = 0.5f;

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

    private SettingsView activeSettingsInstance;
    private TalkWindowView activeTalkWindowInstance;

    [Header("Gauge UI")]
    [SerializeField] private Image gaugeInside1;
    [SerializeField] private Image gaugeInside2;
    [SerializeField] private Transform contOpeTransform;

    public event Action OnAnyOpeButtonClicked;

    // Presenterが登録するコールバック
    public event Action OnSettingsClicked;
    public event Action OnTitleClicked;
    public event Action OnEndingClicked;
    public event Action<int> OnTalkButtonClicked;
    public event Action OnTalkWindowClicked;
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
    }

    private void OnDisable()
    {
        UnregisterContOpeButtons();
        if (settingsButton != null) settingsButton.onClick.RemoveListener(HandleSettingsClicked);
        if (titleButton != null) titleButton.onClick.RemoveListener(HandleTitleClicked);
        if (endingButton != null) endingButton.onClick.RemoveListener(HandleEndingClicked);
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
    }

    private void HandleSettingsClicked() => OnSettingsClicked?.Invoke();

    private void HandleTitleClicked() => OnTitleClicked?.Invoke();

    private void HandleEndingClicked() => OnEndingClicked?.Invoke();

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
        if (animData.textures == null || animData.textures.Length == 0) return;

        RawImage targetImage = animationRawImage != null ? animationRawImage : characterRawImage;
        if (targetImage == null) return;

        // 別のボタンが押された（または新規起動）の場合
        if (index != _activeAnimationIndex)
        {
            StopLoopAnimation(); // 現在の再生を停止し非表示にする
            _activeAnimationIndex = index;
            _currentAnimationIndices[index] = 0; // 1枚目から開始

            targetImage.gameObject.SetActive(true);
            targetImage.texture = animData.textures[0];

            if (animData.isLoop)
            {
                // ループアニメーションなら自動再生を開始
                _activeLoopCoroutine = StartCoroutine(CoLoopAnimation(animData.textures, targetImage));
            }
        }
        else
        {
            // 同じボタンが再度押された場合
            if (!animData.isLoop)
            {
                // ループなしの場合は、手動で次の画像に進める（コマ送り）
                int nextIndex = (_currentAnimationIndices[index] + 1) % animData.textures.Length;
                _currentAnimationIndices[index] = nextIndex;
                targetImage.texture = animData.textures[nextIndex];
            }
            // ループありの場合は、既に自動再生コルーチンが走っているので何もしない
        }
    }

    private System.Collections.IEnumerator CoLoopAnimation(Texture2D[] textures, RawImage targetImage)
    {
        int index = 0;
        while (true)
        {
            yield return new WaitForSeconds(animationFrameRate);
            index = (index + 1) % textures.Length;
            if (targetImage != null)
            {
                targetImage.texture = textures[index];
            }
            
            if (_activeAnimationIndex >= 0 && _currentAnimationIndices != null && _activeAnimationIndex < _currentAnimationIndices.Length)
            {
                _currentAnimationIndices[_activeAnimationIndex] = index;
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

        _activeAnimationIndex = -1; // アクティブ状態をリセット

        // アニメーション専用のRawImageがある場合のみ非表示にする
        if (animationRawImage != null)
        {
            animationRawImage.gameObject.SetActive(false);
        }
    }

    private void RegisterContOpeButtons()
    {
        if (contOpeTransform == null)
        {
            Debug.LogWarning("[MainView] contOpeTransform がアタッチされていないため、ボタンの監視ができません。");
            return;
        }

        Button[] buttons = contOpeTransform.GetComponentsInChildren<Button>(true);
        foreach (var btn in buttons)
        {
            btn.onClick.AddListener(HandleAnyOpeButtonClicked);
        }
        Debug.Log($"[MainView] Cont_ope内の {buttons.Length} 個のボタンにゲージ増加イベントを登録しました。");
    }

    private void UnregisterContOpeButtons()
    {
        if (contOpeTransform == null) return;
        Button[] buttons = contOpeTransform.GetComponentsInChildren<Button>(true);
        foreach (var btn in buttons)
        {
            btn.onClick.RemoveListener(HandleAnyOpeButtonClicked);
        }
    }

    private void HandleAnyOpeButtonClicked()
    {
        OnAnyOpeButtonClicked?.Invoke();
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
}
