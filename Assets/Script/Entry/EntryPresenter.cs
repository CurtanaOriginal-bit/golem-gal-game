using UnityEngine;

public class EntryPresenter : MonoBehaviour
{
    [SerializeField] private EntryView entryView;
    [SerializeField] private EntryModel entryModel;

    void Start()
    {
        if (entryView == null || entryModel == null)
        {
            Debug.LogError("EntryView または EntryModel がアタッチされていません。");
            return;
        }

        // データのロードと初期値設定
        entryModel.LoadSettings();
        entryView.InitializeSliders(entryModel.MasterVolume, entryModel.BGMVolume, entryModel.SEVolume);

        // ViewのイベントをPresenter経由でModelやシステムに繋ぐ
        entryView.OnStartClicked = HandleStartGame;
        entryView.OnSettingsClicked = () => entryView.SetSettingsPanelActive(true);
        entryView.OnCloseSettingsClicked = () => entryView.SetSettingsPanelActive(false);
        entryView.OnExitClicked = HandleExitGame;

        entryView.OnMasterVolumeChanged = entryModel.SaveMasterVolume;
        entryView.OnBgmVolumeChanged = entryModel.SaveBGMVolume;
        entryView.OnSeVolumeChanged = entryModel.SaveSEVolume;
    }

    private void HandleStartGame()
    {
        entryView.StartGame();
    }

    private void HandleExitGame()
    {
        entryModel.QuitGame();
    }
}
