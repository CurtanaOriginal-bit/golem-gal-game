using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// メイン画面の Presenter です。MainView の操作を MainModel の処理につなぎ、Model の状態変化を View に反映します。
/// </summary>
public class MainPresenter : MonoBehaviour
{
    /// <summary>
    /// 挿入ボタンを押せるようになる興奮度（Gauge1）
    /// </summary>
    private const float PenetrationRequiredGauge = 60f;

    /// <summary>
    /// メイン画面の View
    /// </summary>
    [SerializeField] private MainView mainView;

    /// <summary>
    /// メイン画面の Model（ゲージ・服装・会話・設定値などの状態）
    /// </summary>
    [SerializeField] private MainModel mainModel;

    /// <summary>
    /// オート会話で次のセリフへ進めるまでの経過時間(秒)
    /// </summary>
    private float _autoTalkTimer;

    /// <summary>
    /// 保存済みの設定を読み込み、View・Model のイベントを購読して、ゲージと会話ボタンを初期化します。
    /// </summary>
    private void OnEnable()
    {
        Debug.Log("[MainPresenter] OnEnable() が呼び出されました。");
        if (mainView == null || mainModel == null)
        {
            Debug.LogError("[MainPresenter] MainView または MainModel がアタッチされていません。");
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

    /// <summary>
    /// View・Model のイベントの購読を解除します。
    /// </summary>
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

    /// <summary>
    /// アニメーションを止めて設定画面を開きます。
    /// </summary>
    private void HandleSettingsClicked()
    {
        mainView.StopLoopAnimation();
        mainView.OpenSettings();
    }

    /// <summary>
    /// 開いた設定画面に現在の設定値を反映し、変更時の保存処理をつなぎます。
    /// </summary>
    private void HandleSettingsOpened(SettingsView settingsView)
    {
        // 動的生成された設定画面のスライダーに現在の音量値を反映
        settingsView.InitializeSliders(mainModel.MasterVolume, mainModel.BGMVolume, mainModel.SEVolume);
        settingsView.InitializeFullScreenToggle(mainModel.IsFullScreen);

        // 設定画面内のイベントをModelの保存処理に繋ぐ
        settingsView.OnMasterVolumeChanged = mainModel.SaveMasterVolume;
        settingsView.OnBgmVolumeChanged = mainModel.SaveBGMVolume;
        settingsView.OnSeVolumeChanged = mainModel.SaveSEVolume;
        settingsView.OnFullScreenChanged = mainModel.SaveFullScreen;

        // 設定画面内の閉じるボタンが押されたらView側で破棄する
        settingsView.OnCloseSettingsClicked = HandleCloseSettingsClicked;
    }

    /// <summary>
    /// 設定画面を閉じます。
    /// </summary>
    private void HandleCloseSettingsClicked()
    {
        mainView.CloseSettings();
    }

    /// <summary>
    /// アニメーションを止めてタイトル画面へ遷移します。
    /// </summary>
    private void HandleTitleClicked()
    {
        mainView.StopLoopAnimation();
        mainModel.LoadTitleScene();
    }

    /// <summary>
    /// アニメーションを止めてエンディング画面へ遷移します。
    /// </summary>
    private void HandleEndingClicked()
    {
        mainView.StopLoopAnimation();
        mainModel.LoadEndingScene();
    }

    /// <summary>
    /// 再生中のループアニメーションを止めます。
    /// </summary>
    private void HandleCancelClicked()
    {
        // 進行中のアニメーションのみを強制的に終了・非表示にする
        mainView.StopLoopAnimation();
    }

    /// <summary>
    /// 指定した会話を始め、会話ウィンドウを開きます（会話ボタンの押下回数も記録します）。
    /// </summary>
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
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    /// <summary>
    /// 会話中、オート会話の時間経過や Space・Enter キーでセリフを進めます。
    /// </summary>
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

            if (Keyboard.current != null)
            {
                if (Keyboard.current.spaceKey.wasPressedThisFrame || 
                    Keyboard.current.enterKey.wasPressedThisFrame)
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

    /// <summary>
    /// オート会話の ON/OFF を切り替えます。
    /// </summary>
    private void HandleAutoTalkToggleChanged(bool isOn)
    {
        mainModel.IsAutoTalkMode = isOn;
    }

    /// <summary>
    /// 会話ウィンドウがクリックされたらセリフを進めます。
    /// </summary>
    private void HandleTalkWindowClicked()
    {
        AdvanceTalk();
    }

    /// <summary>
    /// 次のセリフを表示します。最後のセリフだった場合は会話ウィンドウを閉じます。
    /// </summary>
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

    /// <summary>
    /// アニメーションを止めて「着せる」モードにします。
    /// </summary>
    private void HandleDressClicked()
    {
        mainView.StopLoopAnimation();
        mainModel.SetControlMode(OutfitControlMode.Dress);
    }

    /// <summary>
    /// アニメーションを止めて「脱がす」モードにします。
    /// </summary>
    private void HandleStripClicked()
    {
        mainView.StopLoopAnimation();
        mainModel.SetControlMode(OutfitControlMode.Strip);
    }

    /// <summary>
    /// キャラクターの上半身のクリックを Model に伝えます。
    /// </summary>
    private void HandleUpperAreaClicked()
    {
        mainModel.ClickUpperArea();
    }

    /// <summary>
    /// キャラクターの下半身のクリックを Model に伝えます。
    /// </summary>
    private void HandleLowerAreaClicked()
    {
        mainModel.ClickLowerArea();
    }

    /// <summary>
    /// 服装の表示を更新し、挿入ボタンの有効判定も更新します。
    /// </summary>
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

    /// <summary>
    /// 着せる・脱がすモードに合わせて UI を切り替えます。
    /// </summary>
    private void HandleControlModeChanged(OutfitControlMode mode)
    {
        mainView.SetControlModeUI(mode);
    }

    /// <summary>
    /// 挿入の ON/OFF を切り替えます。
    /// </summary>
    private void HandleOrderPenetrationClicked()
    {
        mainModel.TogglePenetration();
    }

    /// <summary>
    /// フェラの ON/OFF を切り替えます。
    /// </summary>
    private void HandleOrderBlowjobClicked()
    {
        mainModel.ToggleBlowjob();
    }

    /// <summary>
    /// 挿入・フェラの状態に合わせてボタン表示とアニメーションを更新します。
    /// </summary>
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

    /// <summary>
    /// 指定したループアニメーションを再生します。
    /// </summary>
    private void HandleLoopAnimationButtonClicked(int index)
    {
        mainView.PlayLoopAnimation(index);
    }

    /// <summary>
    /// 興奮度（Gauge1）を増やします。
    /// </summary>
    private void HandleOpeButtonClicked(float amount)
    {
        mainModel.IncreaseGauge1(amount);
    }

    /// <summary>
    /// 射精感（Gauge2）を増やします。
    /// </summary>
    private void HandleGauge2IncreaseButtonClicked(float amount)
    {
        mainModel.IncreaseGauge2(amount);
    }

    /// <summary>
    /// 指定したボタンのホバー時ポップアップを表示します。
    /// </summary>
    private void HandlePopupHoverEntered(int index)
    {
        mainView.ShowHoverPopup(index, true);
    }

    /// <summary>
    /// 指定したボタンのホバー時ポップアップを非表示にします。
    /// </summary>
    private void HandlePopupHoverExited(int index)
    {
        mainView.ShowHoverPopup(index, false);
    }

    /// <summary>
    /// ゲージ表示を更新し、挿入ボタンの有効判定と会話ボタンの解放状態も更新します。
    /// </summary>
    private void HandleGaugeChanged(float gauge1Value, float gauge2Value)
    {
        mainView.UpdateGaugeFill(gauge1Value / MainModel.MaxGaugeValue, gauge2Value / MainModel.MaxGaugeValue);

        // ゲージ変動時にも挿入ボタンの有効判定を更新する
        UpdateOrderPenetrationButtonState();
        
        // 会話ボタンの解放状態を更新する
        UpdateTalkButtonStates();
    }

    /// <summary>
    /// キャラクターの表情を切り替えます。
    /// </summary>
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

        bool isGaugeEnough = mainModel.Gauge1Value >= PenetrationRequiredGauge;
        bool isLowerStripped = !mainModel.LowerBackVisible && !mainModel.LowerFrontVisible;

        mainView.SetOrderPenetrationInteractable(isGaugeEnough && isLowerStripped);
    }

    /// <summary>
    /// 興奮度とアクション回数の条件から、各会話ボタンの解放状態を更新します。
    /// </summary>
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
