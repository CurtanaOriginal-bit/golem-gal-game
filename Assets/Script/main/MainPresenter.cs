using UnityEngine;

public class MainPresenter : MonoBehaviour
{
    [SerializeField] private MainView mainView;
    [SerializeField] private MainModel mainModel;

    private void OnEnable()
    {
        Debug.Log("[MainPresenter] OnEnable() が呼び出されました。");
        if (mainView == null || mainModel == null)
        {
            Debug.LogError("MainView または MainModel がアタッチされていません。");
            return;
        }

        // データのロード
        mainModel.LoadSettings();

        // ViewのイベントをPresenter経由でModelやシステムに繋ぐ (+=で購読)
        mainView.OnSettingsClicked += HandleSettingsClicked;
        mainView.OnSettingsOpened += HandleSettingsOpened;
        mainView.OnTitleClicked += HandleTitleClicked;
        mainView.OnTalkButtonClicked += HandleTalkButtonClicked;
        mainView.OnTalkWindowClicked += HandleTalkWindowClicked;
    }

    private void OnDisable()
    {
        if (mainView != null)
        {
            // イベントの解除 (-=で解除)
            mainView.OnSettingsClicked -= HandleSettingsClicked;
            mainView.OnSettingsOpened -= HandleSettingsOpened;
            mainView.OnTitleClicked -= HandleTitleClicked;
            mainView.OnTalkButtonClicked -= HandleTalkButtonClicked;
            mainView.OnTalkWindowClicked -= HandleTalkWindowClicked;
        }
    }

    private void HandleSettingsClicked()
    {
        mainView.OpenSettings();
    }

    private void HandleSettingsOpened(SettingsView settingsView)
    {
        // 動的生成された設定画面のスライダーに現在の音量値を反映
        settingsView.InitializeSliders(mainModel.MasterVolume, mainModel.BGMVolume, mainModel.SEVolume);

        // 設定画面内のイベントをModelの保存処理に繋ぐ
        settingsView.OnMasterVolumeChanged = mainModel.SaveMasterVolume;
        settingsView.OnBgmVolumeChanged = mainModel.SaveBGMVolume;
        settingsView.OnSeVolumeChanged = mainModel.SaveSEVolume;

        // 設定画面内の閉じるボタンが押されたらView側で破棄する
        settingsView.OnCloseSettingsClicked = HandleCloseSettingsClicked;
    }

    private void HandleCloseSettingsClicked()
    {
        mainView.CloseSettings();
    }

    private void HandleTitleClicked()
    {
        mainModel.LoadTitleScene();
    }

    private void HandleTalkButtonClicked(int index)
    {
        mainModel.StartTalk(index);
        string text = mainModel.GetCurrentSentence();
        mainView.OpenTalkWindow(text);
    }

    private void HandleTalkWindowClicked()
    {
        if (mainModel.AdvanceSentence())
        {
            string text = mainModel.GetCurrentSentence();
            mainView.UpdateTalkText(text);
        }
        else
        {
            mainView.CloseTalkWindow();
        }
    }
}
