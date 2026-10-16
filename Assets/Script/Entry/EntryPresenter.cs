using UnityEngine;

/// <summary>
/// タイトル画面の Presenter です。EntryView の操作を EntryModel の処理につなぎます。
/// </summary>
public class EntryPresenter : MonoBehaviour
{
    /// <summary>
    /// タイトル画面の View
    /// </summary>
    [SerializeField] private EntryView entryView;

    /// <summary>
    /// タイトル画面の Model（設定値の保存・シーン遷移）
    /// </summary>
    [SerializeField] private EntryModel entryModel;

    /// <summary>
    /// 保存済みの設定を読み込み、View のイベントを購読します。
    /// </summary>
    private void OnEnable()
    {
        Debug.Log("[EntryPresenter] OnEnable() が呼び出されました。");
        if (entryView == null || entryModel == null)
        {
            Debug.LogError("[EntryPresenter] EntryView または EntryModel がアタッチされていません。");
            return;
        }

        // データのロード
        entryModel.LoadSettings();

        // ViewのイベントをPresenter経由でModelやシステムに繋ぐ (+=で購読)
        entryView.OnStartClicked += HandleStartGame;
        entryView.OnExitClicked += HandleExitGame;
        entryView.OnSettingsClicked += HandleSettingsClicked;
        entryView.OnSettingsOpened += HandleSettingsOpened;
        entryView.OnCreditClicked += HandleCreditClicked;
        entryView.OnCreditOpened += HandleCreditOpened;
    }

    /// <summary>
    /// View のイベントの購読を解除します。
    /// </summary>
    private void OnDisable()
    {
        if (entryView != null)
        {
            // イベントの解除 (-=で解除)
            entryView.OnStartClicked -= HandleStartGame;
            entryView.OnExitClicked -= HandleExitGame;
            entryView.OnSettingsClicked -= HandleSettingsClicked;
            entryView.OnSettingsOpened -= HandleSettingsOpened;
            entryView.OnCreditClicked -= HandleCreditClicked;
            entryView.OnCreditOpened -= HandleCreditOpened;
        }
    }

    /// <summary>
    /// ゲームを開始し、導入シーンへ遷移します。
    /// </summary>
    private void HandleStartGame()
    {
        entryModel.LoadIntroductionScene();
    }

    /// <summary>
    /// ゲームを終了します。
    /// </summary>
    private void HandleExitGame()
    {
        entryModel.QuitGame();
    }

    /// <summary>
    /// 設定画面を開きます。
    /// </summary>
    private void HandleSettingsClicked()
    {
        entryView.OpenSettings();
    }

    /// <summary>
    /// クレジット画面を開きます。
    /// </summary>
    private void HandleCreditClicked()
    {
        entryView.OpenCredit();
    }

    /// <summary>
    /// 開いた設定画面に現在の設定値を反映し、変更時の保存処理をつなぎます。
    /// </summary>
    private void HandleSettingsOpened(SettingsView settingsView)
    {
        // 動的生成された設定画面のスライダーに現在の音量値を反映
        settingsView.InitializeSliders(entryModel.MasterVolume, entryModel.BGMVolume, entryModel.SEVolume);
        settingsView.InitializeFullScreenToggle(entryModel.IsFullScreen);

        // 設定画面内のイベントをModelの保存処理に繋ぐ
        settingsView.OnMasterVolumeChanged = entryModel.SaveMasterVolume;
        settingsView.OnBgmVolumeChanged = entryModel.SaveBGMVolume;
        settingsView.OnSeVolumeChanged = entryModel.SaveSEVolume;
        settingsView.OnFullScreenChanged = entryModel.SaveFullScreen;

        // 設定画面内の閉じるボタンが押されたらView側で破棄する
        settingsView.OnCloseSettingsClicked = HandleCloseSettingsClicked;
    }

    /// <summary>
    /// 開いたクレジット画面の閉じるボタンに処理をつなぎます。
    /// </summary>
    private void HandleCreditOpened(CreditView creditView)
    {
        // クレジット画面内の閉じるボタンが押されたらView側で破棄する
        creditView.OnCloseCreditClicked = HandleCloseCreditClicked;
    }

    /// <summary>
    /// 設定画面を閉じます。
    /// </summary>
    private void HandleCloseSettingsClicked()
    {
        entryView.CloseSettings();
    }

    /// <summary>
    /// クレジット画面を閉じます。
    /// </summary>
    private void HandleCloseCreditClicked()
    {
        entryView.CloseCredit();
    }
}
