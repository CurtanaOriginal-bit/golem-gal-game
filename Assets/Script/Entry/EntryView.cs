using UnityEngine;
using UnityEngine.UI;
using System;

public class EntryView : ViewBase
{
    [Header("Main Menu UI")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button creditButton;

    [Header("Settings Prefab UI")]
    [SerializeField] private SettingsView settingsPrefab;
    [SerializeField] private RectTransform canvasTransform;
    [SerializeField] private GameObject creditPrefab;

    private SettingsView activeSettingsInstance;
    private CreditView activeCreditInstance;

    // Presenterが登録するコールバック
    public event Action OnStartClicked;
    public event Action OnSettingsClicked;
    public event Action OnExitClicked;
    public event Action OnCreditClicked;

    // 設定画面が生成されたときにPresenterに通知するコールバック
    public event Action<SettingsView> OnSettingsOpened;

    // クレジット画面が生成されたときにPresenterに通知するコールバック
    public event Action<CreditView> OnCreditOpened;

    private void OnEnable()
    {
        // UIのイベント登録
        if (startButton != null) startButton.onClick.AddListener(HandleStartClicked);
        if (settingsButton != null) settingsButton.onClick.AddListener(HandleSettingsClicked);
        if (exitButton != null) exitButton.onClick.AddListener(HandleExitClicked);
        if (creditButton != null) creditButton.onClick.AddListener(HandleCreditClicked);
    }

    private void OnDisable()
    {
        // UIのイベント解除
        if (startButton != null) startButton.onClick.RemoveListener(HandleStartClicked);
        if (settingsButton != null) settingsButton.onClick.RemoveListener(HandleSettingsClicked);
        if (exitButton != null) exitButton.onClick.RemoveListener(HandleExitClicked);
        if (creditButton != null) creditButton.onClick.RemoveListener(HandleCreditClicked);
    }

    private void HandleStartClicked() => OnStartClicked?.Invoke();
    private void HandleSettingsClicked() => OnSettingsClicked?.Invoke();
    private void HandleExitClicked() => OnExitClicked?.Invoke();
    private void HandleCreditClicked() => OnCreditClicked?.Invoke();

    public bool IsSettingsPanelActive => activeSettingsInstance != null;

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
            Debug.LogError("settingsPrefab または canvasTransform がアタッチされていません。");
        }
    }

    public void OpenCredit()
    {
        if (activeCreditInstance != null)
        {
            return;
        }

        if (creditPrefab != null && canvasTransform != null)
        {
            var go = Instantiate(creditPrefab, canvasTransform);
            activeCreditInstance = go.GetComponent<CreditView>();
            if (activeCreditInstance != null)
            {
                OnCreditOpened?.Invoke(activeCreditInstance);
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

    public void CloseSettings()
    {
        if (activeSettingsInstance != null)
        {
            Destroy(activeSettingsInstance.gameObject);
            activeSettingsInstance = null;
        }
    }
    public void CloseCredit()
    {
        if (activeCreditInstance != null)
        {
            Destroy(activeCreditInstance.gameObject);
            activeCreditInstance = null;
        }
    }
}
