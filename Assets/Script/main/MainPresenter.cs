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
        mainView.OnEndingClicked += HandleEndingClicked;
        mainView.OnTalkButtonClicked += HandleTalkButtonClicked;
        mainView.OnTalkWindowClicked += HandleTalkWindowClicked;
        mainView.OnDressClicked += HandleDressClicked;
        mainView.OnStripClicked += HandleStripClicked;
        mainView.OnUpperAreaClicked += HandleUpperAreaClicked;
        mainView.OnLowerAreaClicked += HandleLowerAreaClicked;
        mainView.OnLoopAnimationButtonClicked += HandleLoopAnimationButtonClicked;

        // 初期衣装状態の反映
        mainView.UpdateOutfitVisibility(mainModel.UpperBackVisible, mainModel.UpperFrontVisible, mainModel.LowerBackVisible, mainModel.LowerFrontVisible);
        mainView.SetControlModeUI(mainModel.CurrentMode);

        mainModel.OnOutfitStateChanged += HandleOutfitStateChanged;
        mainModel.OnControlModeChanged += HandleControlModeChanged;

        // ゲージの購読と初期化
        mainModel.OnGaugeChanged += HandleGaugeChanged;
        mainView.OnAnyOpeButtonClicked += HandleAnyOpeButtonClicked;
        mainModel.InitializeGauges();
    }

    private void OnDisable()
    {
        if (mainView != null)
        {
            // イベントの解除 (-=で解除)
            mainView.OnSettingsClicked -= HandleSettingsClicked;
            mainView.OnSettingsOpened -= HandleSettingsOpened;
            mainView.OnTitleClicked -= HandleTitleClicked;
            mainView.OnEndingClicked -= HandleEndingClicked;
            mainView.OnTalkButtonClicked -= HandleTalkButtonClicked;
            mainView.OnTalkWindowClicked -= HandleTalkWindowClicked;
            mainView.OnDressClicked -= HandleDressClicked;
            mainView.OnStripClicked -= HandleStripClicked;
            mainView.OnUpperAreaClicked -= HandleUpperAreaClicked;
            mainView.OnLowerAreaClicked -= HandleLowerAreaClicked;
            mainView.OnLoopAnimationButtonClicked -= HandleLoopAnimationButtonClicked;
            mainView.OnAnyOpeButtonClicked -= HandleAnyOpeButtonClicked;
        }

        if (mainModel != null)
        {
            mainModel.OnGaugeChanged -= HandleGaugeChanged;
            mainModel.OnOutfitStateChanged -= HandleOutfitStateChanged;
            mainModel.OnControlModeChanged -= HandleControlModeChanged;
        }
    }

    private void HandleSettingsClicked()
    {
        mainView.StopLoopAnimation();
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
        mainView.StopLoopAnimation();
        mainModel.LoadTitleScene();
    }

    private void HandleEndingClicked()
    {
        mainView.StopLoopAnimation();
        mainModel.LoadEndingScene();
    }

    private void HandleTalkButtonClicked(int index)
    {
        mainView.StopLoopAnimation();
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

    private void HandleDressClicked()
    {
        mainView.StopLoopAnimation();
        mainModel.SetControlMode(OutfitControlMode.Dress);
    }

    private void HandleStripClicked()
    {
        mainView.StopLoopAnimation();
        mainModel.SetControlMode(OutfitControlMode.Strip);
    }

    private void HandleUpperAreaClicked()
    {
        mainModel.ClickUpperArea();
    }

    private void HandleLowerAreaClicked()
    {
        mainModel.ClickLowerArea();
    }

    private void HandleOutfitStateChanged()
    {
        mainView.UpdateOutfitVisibility(
            mainModel.UpperBackVisible,
            mainModel.UpperFrontVisible,
            mainModel.LowerBackVisible,
            mainModel.LowerFrontVisible
        );
    }

    private void HandleControlModeChanged(OutfitControlMode mode)
    {
        mainView.SetControlModeUI(mode);
    }

    private void HandleLoopAnimationButtonClicked(int index)
    {
        mainView.PlayLoopAnimation(index);
    }

    private void HandleAnyOpeButtonClicked()
    {
        mainModel.IncreaseGauge1(10f);
    }

    private void HandleGaugeChanged(float gauge1Value, float gauge2Value)
    {
        mainView.UpdateGaugeFill(gauge1Value / MainModel.MaxGaugeValue, gauge2Value / MainModel.MaxGaugeValue);
    }
}
