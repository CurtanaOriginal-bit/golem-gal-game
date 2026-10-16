using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// タイトル画面の View です。メニューボタンの操作の通知と、設定・クレジット画面の生成・破棄を行います。
/// </summary>
public class EntryView : ViewBase
{
    /// <summary>
    /// ゲーム開始ボタン
    /// </summary>
    [Header("Main Menu UI")]
    [SerializeField] private Button startButton;

    /// <summary>
    /// 設定画面を開くボタン
    /// </summary>
    [SerializeField] private Button settingsButton;

    /// <summary>
    /// ゲーム終了ボタン
    /// </summary>
    [SerializeField] private Button exitButton;

    /// <summary>
    /// クレジット画面を開くボタン
    /// </summary>
    [SerializeField] private Button creditButton;

    /// <summary>
    /// 設定画面のプレハブ
    /// </summary>
    [Header("Settings Prefab UI")]
    [SerializeField] private SettingsView settingsPrefab;

    /// <summary>
    /// 設定・クレジット画面を生成する親の Canvas
    /// </summary>
    [SerializeField] private RectTransform canvasTransform;

    /// <summary>
    /// クレジット画面のプレハブ
    /// </summary>
    [SerializeField] private GameObject creditPrefab;

    /// <summary>
    /// 開いている設定画面（開いていなければ null）
    /// </summary>
    private SettingsView _activeSettingsInstance;

    /// <summary>
    /// 開いているクレジット画面（開いていなければ null）
    /// </summary>
    private CreditView _activeCreditInstance;

    // Presenterが登録するコールバック
    /// <summary>
    /// ゲーム開始ボタンが押されたときに発火するイベント
    /// </summary>
    public event Action OnStartClicked;

    /// <summary>
    /// 設定ボタンが押されたときに発火するイベント
    /// </summary>
    public event Action OnSettingsClicked;

    /// <summary>
    /// 終了ボタンが押されたときに発火するイベント
    /// </summary>
    public event Action OnExitClicked;

    /// <summary>
    /// クレジットボタンが押されたときに発火するイベント
    /// </summary>
    public event Action OnCreditClicked;

    // 設定画面が生成されたときにPresenterに通知するコールバック
    /// <summary>
    /// 設定画面を生成したときに発火するイベント
    /// </summary>
    public event Action<SettingsView> OnSettingsOpened;

    // クレジット画面が生成されたときにPresenterに通知するコールバック
    /// <summary>
    /// クレジット画面を生成したときに発火するイベント
    /// </summary>
    public event Action<CreditView> OnCreditOpened;

    /// <summary>
    /// 有効化時にメニューボタンのクリックイベントを購読します。
    /// </summary>
    private void OnEnable()
    {
        // UIのイベント登録
        if (startButton != null) startButton.onClick.AddListener(HandleStartClicked);
        if (settingsButton != null) settingsButton.onClick.AddListener(HandleSettingsClicked);
        if (exitButton != null) exitButton.onClick.AddListener(HandleExitClicked);
        if (creditButton != null) creditButton.onClick.AddListener(HandleCreditClicked);
    }

    /// <summary>
    /// 無効化時にメニューボタンのクリックイベントの購読を解除します。
    /// </summary>
    private void OnDisable()
    {
        // UIのイベント解除
        if (startButton != null) startButton.onClick.RemoveListener(HandleStartClicked);
        if (settingsButton != null) settingsButton.onClick.RemoveListener(HandleSettingsClicked);
        if (exitButton != null) exitButton.onClick.RemoveListener(HandleExitClicked);
        if (creditButton != null) creditButton.onClick.RemoveListener(HandleCreditClicked);
    }

    /// <summary>
    /// ゲーム開始ボタンが押されたことを通知します。
    /// </summary>
    private void HandleStartClicked() => OnStartClicked?.Invoke();

    /// <summary>
    /// 設定ボタンが押されたことを通知します。
    /// </summary>
    private void HandleSettingsClicked() => OnSettingsClicked?.Invoke();

    /// <summary>
    /// 終了ボタンが押されたことを通知します。
    /// </summary>
    private void HandleExitClicked() => OnExitClicked?.Invoke();

    /// <summary>
    /// クレジットボタンが押されたことを通知します。
    /// </summary>
    private void HandleCreditClicked() => OnCreditClicked?.Invoke();

    /// <summary>
    /// 設定画面を開いているかどうか
    /// </summary>
    public bool IsSettingsPanelActive => _activeSettingsInstance != null;

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
            Debug.LogError("settingsPrefab または canvasTransform がアタッチされていません。");
        }
    }

    /// <summary>
    /// クレジット画面を生成して開きます（すでに開いていれば何もしません）。
    /// </summary>
    public void OpenCredit()
    {
        if (_activeCreditInstance != null)
        {
            return;
        }

        if (creditPrefab != null && canvasTransform != null)
        {
            var go = Instantiate(creditPrefab, canvasTransform);
            _activeCreditInstance = go.GetComponent<CreditView>();
            if (_activeCreditInstance != null)
            {
                OnCreditOpened?.Invoke(_activeCreditInstance);
            }
            else
            {
                Debug.LogError("creditPrefab に CreditView コンポーネントがアタッチされていません。");
                Destroy(go);
            }
        }
        else
        {
            Debug.LogError("creditPrefab または canvasTransform がアタッチされていません。");
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
    /// クレジット画面を破棄して閉じます。
    /// </summary>
    public void CloseCredit()
    {
        if (_activeCreditInstance != null)
        {
            Destroy(_activeCreditInstance.gameObject);
            _activeCreditInstance = null;
        }
    }
}
