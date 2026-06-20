using UnityEngine;

public class EntryPresenter : MonoBehaviour
{
    [SerializeField] private EntryView entryView;
    [SerializeField] private EntryModel entryModel;

    void Start()
    {
        Debug.Log("[EntryPresenter] Start() が呼び出されました。");
        if (entryView == null || entryModel == null)
        {
            Debug.LogError("EntryView または EntryModel がアタッチされていません。");
            return;
        }

        // データのロード
        entryModel.LoadSettings();

        // ViewのイベントをPresenter経由でModelやシステムに繋ぐ
        entryView.OnStartClicked = HandleStartGame;
        entryView.OnExitClicked = HandleExitGame;

        // 設定ボタンがクリックされたら設定画面を開く
        entryView.OnSettingsClicked = () => entryView.OpenSettings();

        // 設定画面が生成されたタイミングでイベントを紐付ける
        entryView.OnSettingsOpened = (settingsView) =>
        {
            // 動的生成された設定画面のスライダーに現在の音量値を反映
            settingsView.InitializeSliders(entryModel.MasterVolume, entryModel.BGMVolume, entryModel.SEVolume);

            // 設定画面内のイベントをModelの保存処理に繋ぐ
            settingsView.OnMasterVolumeChanged = entryModel.SaveMasterVolume;
            settingsView.OnBgmVolumeChanged = entryModel.SaveBGMVolume;
            settingsView.OnSeVolumeChanged = entryModel.SaveSEVolume;

            // 設定画面内の閉じるボタンが押されたらView側で破棄する
            settingsView.OnCloseSettingsClicked = () => entryView.CloseSettings();
        };
    }

    private void HandleStartGame()
    {
        entryModel.LoadIntroductionScene();
    }

    private void HandleExitGame()
    {
        entryModel.QuitGame();
    }
}
