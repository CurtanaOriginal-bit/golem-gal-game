using UnityEngine;

public class MainPresenter : MonoBehaviour
{
    [SerializeField] private MainView mainView;
    [SerializeField] private MainModel mainModel;

    private void OnEnable()
    {
        CustomLogger.LogDebug("OnEnable() が呼び出されました。");
        if (mainView == null || mainModel == null)
        {
            CustomLogger.LogError("MainView または MainModel がアタッチされていません。");
            return;
        }

        // データのロード
        mainModel.LoadSettings();

        // ViewのイベントをPresenter経由でModelやシステムに繋ぐ (+=で購読)
        mainView.OnSettingsClicked += HandleSettingsClicked;
        mainView.OnSettingsOpened += HandleSettingsOpened;
        mainView.OnTitleClicked += HandleTitleClicked;
        mainView.OnEndingClicked += HandleEndingClicked;
        mainView.OnCancelClicked += HandleCancelClicked;
        mainView.OnTalkButtonClicked += HandleTalkButtonClicked;
        mainView.OnTalkWindowClicked += HandleTalkWindowClicked;
        mainView.OnAutoTalkToggleChanged += HandleAutoTalkToggleChanged;
        mainView.OnDressClicked += HandleDressClicked;
        mainView.OnStripClicked += HandleStripClicked;
        mainView.OnUpperAreaClicked += HandleUpperAreaClicked;
        mainView.OnLowerAreaClicked += HandleLowerAreaClicked;
        mainView.OnLoopAnimationButtonClicked += HandleLoopAnimationButtonClicked;
        mainView.OnOrderPenetrationClicked += HandleOrderPenetrationClicked;
        mainView.OnOrderBlowjobClicked += HandleOrderBlowjobClicked;

        // 初期衣装状態の反映
        mainView.UpdateOutfitVisibility(mainModel.UpperBackVisible, mainModel.UpperFrontVisible, mainModel.LowerBackVisible, mainModel.LowerFrontVisible);
        mainView.SetControlModeUI(mainModel.CurrentMode);

        mainModel.OnOutfitStateChanged += HandleOutfitStateChanged;
        mainModel.OnControlModeChanged += HandleControlModeChanged;
        mainModel.OnOrderStateChanged += HandleOrderStateChanged;

        // ゲージの購読と初期化
        mainModel.OnGaugeChanged += HandleGaugeChanged;
        mainModel.OnFaceChanged += HandleFaceChanged;
        mainView.OnOpeButtonClicked += HandleOpeButtonClicked;
        mainView.OnGauge2IncreaseButtonClicked += HandleGauge2IncreaseButtonClicked;
        mainView.OnPopupHoverEntered += HandlePopupHoverEntered;
        mainView.OnPopupHoverExited += HandlePopupHoverExited;
        mainModel.InitializeGauges();
        
        UpdateTalkButtonStates();
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
            mainView.OnCancelClicked -= HandleCancelClicked;
            mainView.OnTalkButtonClicked -= HandleTalkButtonClicked;
            mainView.OnTalkWindowClicked -= HandleTalkWindowClicked;
            mainView.OnAutoTalkToggleChanged -= HandleAutoTalkToggleChanged;
            mainView.OnDressClicked -= HandleDressClicked;
            mainView.OnStripClicked -= HandleStripClicked;
            mainView.OnUpperAreaClicked -= HandleUpperAreaClicked;
            mainView.OnLowerAreaClicked -= HandleLowerAreaClicked;
            mainView.OnLoopAnimationButtonClicked -= HandleLoopAnimationButtonClicked;
            mainView.OnOrderPenetrationClicked -= HandleOrderPenetrationClicked;
            mainView.OnOrderBlowjobClicked -= HandleOrderBlowjobClicked;
            mainView.OnOpeButtonClicked -= HandleOpeButtonClicked;
            mainView.OnGauge2IncreaseButtonClicked -= HandleGauge2IncreaseButtonClicked;
            mainView.OnPopupHoverEntered -= HandlePopupHoverEntered;
            mainView.OnPopupHoverExited -= HandlePopupHoverExited;
        }

        if (mainModel != null)
        {
            mainModel.OnGaugeChanged -= HandleGaugeChanged;
            mainModel.OnFaceChanged -= HandleFaceChanged;
            mainModel.OnOutfitStateChanged -= HandleOutfitStateChanged;
            mainModel.OnControlModeChanged -= HandleControlModeChanged;
            mainModel.OnOrderStateChanged -= HandleOrderStateChanged;
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

    private void HandleCancelClicked()
    {
        // 進行中のアニメーションのみを強制的に終了・非表示にする
        mainView.StopLoopAnimation();
    }

    private void HandleTalkButtonClicked(int index)
    {
        mainView.StopLoopAnimation();
        mainModel.StartTalk(index);
        
        // トークボタンのアクション実行回数を記録 (1始まりのボタン番号)
        mainModel.IncrementActionCount($"Conv_Button{index + 1}");
        
        // 条件が満たされたか状態を更新
        UpdateTalkButtonStates();

        _autoTalkTimer = 0f;
        string text = mainModel.GetCurrentSentence();
        mainView.OpenTalkWindow(text);

        // ボタンがフォーカスされたままだとEnter/Spaceキーで再クリックされてしまうためフォーカスを外す
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private float _autoTalkTimer = 0f;

    private void Update()
    {
        if (mainModel.IsTalking)
        {
            if (mainModel.IsAutoTalkMode)
            {
                _autoTalkTimer += Time.deltaTime;
                if (_autoTalkTimer >= mainModel.AutoTalkInterval)
                {
                    AdvanceTalk();
                }
            }

            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                if (UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame || 
                    UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame)
                {
                    AdvanceTalk();
                }
            }
        }
        else
        {
            _autoTalkTimer = 0f;
        }
    }

    private void HandleAutoTalkToggleChanged(bool isOn)
    {
        mainModel.IsAutoTalkMode = isOn;
    }

    private void HandleTalkWindowClicked()
    {
        AdvanceTalk();
    }

    private void AdvanceTalk()
    {
        _autoTalkTimer = 0f;
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

        // 服装が変更された際にも挿入ボタンの有効判定を更新する
        UpdateOrderPenetrationButtonState();
    }

    private void HandleControlModeChanged(OutfitControlMode mode)
    {
        mainView.SetControlModeUI(mode);
    }

    private void HandleOrderPenetrationClicked()
    {
        mainModel.TogglePenetration();
    }

    private void HandleOrderBlowjobClicked()
    {
        mainModel.ToggleBlowjob();
    }

    private void HandleOrderStateChanged(bool isPenetrationActive, bool isBlowjobActive)
    {
        mainView.UpdateOrderButtonText(isPenetrationActive, isBlowjobActive);
        
        // 挿入状態に応じてアニメーションを再生・停止
        mainView.PlayPenetrationAnimation(isPenetrationActive);

        // どちらもアクティブでない場合、現在進行中のアニメーションを強制停止する
        if (!isPenetrationActive && !isBlowjobActive)
        {
            mainView.StopLoopAnimation();
        }
    }

    private void HandleLoopAnimationButtonClicked(int index)
    {
        mainView.PlayLoopAnimation(index);
    }

    private void HandleOpeButtonClicked(float amount)
    {
        mainModel.IncreaseGauge1(amount);
    }

    private void HandleGauge2IncreaseButtonClicked(float amount)
    {
        mainModel.IncreaseGauge2(amount);
    }

    private void HandlePopupHoverEntered(int index)
    {
        mainView.ShowHoverPopup(index, true);
    }

    private void HandlePopupHoverExited(int index)
    {
        mainView.ShowHoverPopup(index, false);
    }

    private void HandleGaugeChanged(float gauge1Value, float gauge2Value)
    {
        mainView.UpdateGaugeFill(gauge1Value / MainModel.MaxGaugeValue, gauge2Value / MainModel.MaxGaugeValue);

        // ゲージ変動時にも挿入ボタンの有効判定を更新する
        UpdateOrderPenetrationButtonState();
        
        // 会話ボタンの解放状態を更新する
        UpdateTalkButtonStates();
    }

    private void HandleFaceChanged(int faceIndex)
    {
        mainView.UpdateFace(faceIndex);
    }

    /// <summary>
    /// 挿入(Order_Penetration)ボタンの有効/無効状態を判定し、Viewへ反映します。
    /// 条件1: 興奮度(Gauge1)が60以上であること
    /// 条件2: 下半身の服(LowerBack と LowerFront)が両方とも非表示(脱いでいる状態)であること
    /// </summary>
    private void UpdateOrderPenetrationButtonState()
    {
        if (mainModel == null || mainView == null) return;

        bool isGaugeEnough = mainModel.Gauge1Value >= 60f;
        bool isLowerStripped = !mainModel.LowerBackVisible && !mainModel.LowerFrontVisible;

        mainView.SetOrderPenetrationInteractable(isGaugeEnough && isLowerStripped);
    }

    private void UpdateTalkButtonStates()
    {
        var conditions = mainView.TalkButtonConditions;
        if (conditions == null) return;

        float currentGauge = mainModel.Gauge1Value;

        for (int i = 0; i < conditions.Length; i++)
        {
            var cond = conditions[i];
            bool isUnlocked = true;

            if (currentGauge < cond.MinGaugeValue)
            {
                isUnlocked = false;
            }
            else
            {
                if (cond.RequiredActionCounts != null)
                {
                    foreach (var req in cond.RequiredActionCounts)
                    {
                        if (mainModel.GetActionCount(req.ActionKey) < req.MinCount)
                        {
                            isUnlocked = false;
                            break;
                        }
                    }
                }
            }

            mainView.SetTalkButtonInteractable(i, isUnlocked);
        }
    }
}
