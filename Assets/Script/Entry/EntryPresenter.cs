using UnityEngine;

public class EntryPresenter : MonoBehaviour
{
    [SerializeField] private EntryView entryView;
    [SerializeField] private EntryModel entryModel;

    private void OnEnable()
    {
        Debug.Log("[EntryPresenter] OnEnable() が呼び出されました。");
        if (entryView == null || entryModel == null)
        {
            Debug.LogError("EntryView または EntryModel がアタッチされていません。");
            return;
        }

        // データのロード
        entryModel.LoadSettings();

        // ViewのイベントをPresenter経由でModelやシステムに繋ぐ (+=で購読)
        entryView.OnStartClicked += HandleStartGame;
        entryView.OnExitClicked += HandleExitGame;
        entryView.OnSettingsClicked += HandleSettingsClicked;
        entryView.OnSettingsOpened += HandleSettingsOpened;
    }

    private void OnDisable()
    {
        if (entryView != null)
        {
            // イベントの解除 (-=で解除)
            entryView.OnStartClicked -= HandleStartGame;
            entryView.OnExitClicked -= HandleExitGame;
            entryView.OnSettingsClicked -= HandleSettingsClicked;
            entryView.OnSettingsOpened -= HandleSettingsOpened;
        }
    }

    private void HandleStartGame()
    {
        entryModel.LoadIntroductionScene();
    }

    private void HandleExitGame()
    {
        entryModel.QuitGame();
    }

    private void HandleSettingsClicked()
    {
        entryView.OpenSettings();
    }

    private void HandleSettingsOpened(SettingsView settingsView)
    {
        // 動的生成された設定画面のスライダーに現在の音量値を反映
        settingsView.InitializeSliders(entryModel.MasterVolume, entryModel.BGMVolume, entryModel.SEVolume);

        // 設定画面内のイベントをModelの保存処理に繋ぐ
        settingsView.OnMasterVolumeChanged = entryModel.SaveMasterVolume;
        settingsView.OnBgmVolumeChanged = entryModel.SaveBGMVolume;
        settingsView.OnSeVolumeChanged = entryModel.SaveSEVolume;

        // 設定画面内の閉じるボタンが押されたらView側で破棄する
        settingsView.OnCloseSettingsClicked = HandleCloseSettingsClicked;
    }

    private void HandleCloseSettingsClicked()
    {
        entryView.CloseSettings();
    }
}
