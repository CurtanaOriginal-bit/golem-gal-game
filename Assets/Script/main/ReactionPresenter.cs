using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 【Presenter】MainModel の興奮度（Gauge1Value）と各種 View（ボタン、セリフフェード表示）を仲介するプレゼンタークラスです。
/// </summary>
public class ReactionPresenter : MonoBehaviour
{
    /// <summary>
    /// 興奮度データを管理するメインモデルへの参照
    /// </summary>
    [Tooltip("興奮度データ（Gauge1Value）を参照するMainModel")]
    [SerializeField] private MainModel mainModel;

    /// <summary>
    /// デフォルトのセリフテキストを表示・フェードアウトさせるViewへの参照
    /// </summary>
    [Tooltip("デフォルトのセリフを表示するReactionTextView")]
    [SerializeField] private ReactionTextView reactionTextView;

    /// <summary>
    /// 別の場所（フェラチオ時のサブスクリーンなど）にセリフを表示・フェードアウトさせるViewへの参照
    /// </summary>
    [Tooltip("別の場所にセリフを表示するReactionTextView")]
    [SerializeField] private ReactionTextView alternateTextView;

    /// <summary>
    /// ユーザー入力を受け付けるリアクションボタンViewのリスト
    /// </summary>
    [Tooltip("監視対象となるReactionButtonViewの配列")]
    [SerializeField] private ReactionButtonView[] reactionButtonViews;

    /// <summary>
    /// 有効化時に各種イベントの購読を開始します。
    /// </summary>
    private void OnEnable()
    {
        SubscribeEvents();
    }

    /// <summary>
    /// 無効化時に各種イベントの購読を解除します。
    /// </summary>
    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    /// <summary>
    /// 破棄時に各種イベントの購読を解除します。
    /// </summary>
    private void OnDestroy()
    {
        // 念のため破棄時にも確実に購読解除を実施してメモリリークを防止
        UnsubscribeEvents();
    }

    /// <summary>
    /// 各種ボタンViewのクリックイベントを購読します。
    /// </summary>
    private void SubscribeEvents()
    {
        if (reactionButtonViews == null) return;

        for (int i = 0; i < reactionButtonViews.Length; i++)
        {
            var buttonView = reactionButtonViews[i];
            if (buttonView != null)
            {
                // 重複登録防止のため一度解除してから購読
                buttonView.OnClicked -= HandleButtonClicked;
                buttonView.OnClicked += HandleButtonClicked;
            }
        }

        // 着替えアクション実行時のイベントを購読
        if (mainModel != null)
        {
            mainModel.OnOutfitActionExecuted -= HandleOutfitActionExecuted;
            mainModel.OnOutfitActionExecuted += HandleOutfitActionExecuted;

            // トグル状態の変更イベントを購読
            mainModel.OnOrderStateChanged -= HandleOrderStateChanged;
            mainModel.OnOrderStateChanged += HandleOrderStateChanged;
        }
    }

    /// <summary>
    /// 各種ボタンViewのクリックイベントの購読を解除します。
    /// </summary>
    private void UnsubscribeEvents()
    {
        if (reactionButtonViews == null) return;

        for (int i = 0; i < reactionButtonViews.Length; i++)
        {
            var buttonView = reactionButtonViews[i];
            if (buttonView != null)
            {
                buttonView.OnClicked -= HandleButtonClicked;
            }
        }

        if (mainModel != null)
        {
            mainModel.OnOutfitActionExecuted -= HandleOutfitActionExecuted;
            mainModel.OnOrderStateChanged -= HandleOrderStateChanged;
        }
    }

    /// <summary>
    /// ボタンがクリックされた時のハンドラー。即時発火か遅延発火（モード切替）かを判定します。
    /// </summary>
    /// <param name="buttonView">クリックされたボタンのView</param>
    private void HandleButtonClicked(ReactionButtonView buttonView)
    {
        if (buttonView == null) return;
        
        // モード切替用のボタン（TargetControlModeがNone以外）の場合、
        // クリック時ではなく、実際にアクションが実行されたタイミングでセリフを表示する
        if (buttonView.TargetControlMode != OutfitControlMode.None)
        {
            Debug.Log($"[ReactionPresenter] ボタンクリック検知。ただしモード切替設定({buttonView.TargetControlMode})のためセリフ表示を保留します。");
            return;
        }

        // トグル機能ボタン（ON/OFF切り替え）の場合、無効化時のクリック発火を防ぐため、
        // クリックイベントではなく OnOrderStateChanged 側で発火させるのでここでは無視する
        if (buttonView.FeatureType != ReactionFeatureType.None)
        {
            Debug.Log($"[ReactionPresenter] ボタンクリック検知。ただしトグル機能({buttonView.FeatureType})のため状態変更イベントからの発火へ委譲します。");
            return;
        }

        // 即時発火のボタンならそのまま実行
        ExecuteReaction(buttonView);
    }

    /// <summary>
    /// 実際のアクションが実行された時に呼ばれるハンドラー。
    /// 現在のモードと一致する待機中のボタンを探して実行します。
    /// </summary>
    private void HandleOutfitActionExecuted(OutfitActionType actionType)
    {
        if (mainModel == null || reactionButtonViews == null) return;

        OutfitControlMode currentMode = mainModel.CurrentMode;
        Debug.Log($"[ReactionPresenter] 着替えアクション実行: {actionType}, 現在のモード: {currentMode}");

        for (int i = 0; i < reactionButtonViews.Length; i++)
        {
            var buttonView = reactionButtonViews[i];
            if (buttonView != null && buttonView.TargetControlMode == currentMode)
            {
                Debug.Log($"[ReactionPresenter] 保留されていたモード切替設定({currentMode})のセリフを実行します。");
                ExecuteReaction(buttonView);
                break; // 同じモードのボタンは1つと仮定して終了
            }
        }
    }

    /// <summary>
    /// オーダー状態（トグル）が変更された時に呼ばれるハンドラー。
    /// ONになった時は対応するセリフを実行し、OFFになった時は即座に非表示にします。
    /// </summary>
    private void HandleOrderStateChanged(bool isPenetrationActive, bool isBlowjobActive)
    {
        if (reactionButtonViews == null) return;

        // 挿入トグルの処理
        if (isPenetrationActive)
        {
            ExecuteReactionForFeature(ReactionFeatureType.PenetrationToggle);
        }
        else
        {
            HideDialogueForFeature(ReactionFeatureType.PenetrationToggle);
        }

        // フェラチオトグルの処理
        if (isBlowjobActive)
        {
            ExecuteReactionForFeature(ReactionFeatureType.BlowjobToggle);
        }
        else
        {
            HideDialogueForFeature(ReactionFeatureType.BlowjobToggle);
        }

        // ONになっているトグルのループ音に切り替える（どちらもOFFなら停止）
        UpdateLoopSE(isPenetrationActive ? ReactionFeatureType.PenetrationToggle
                   : isBlowjobActive ? ReactionFeatureType.BlowjobToggle
                   : ReactionFeatureType.None);
    }

    /// <summary>
    /// 指定されたFeatureTypeを持つボタンのループ音を再生します。Noneの場合はループ音を止めます。
    /// </summary>
    private void UpdateLoopSE(ReactionFeatureType activeFeature)
    {
        if (SEPlayer.Instance == null) return;

        var buttonView = FindButtonForFeature(activeFeature);
        if (buttonView == null || buttonView.LoopSE == null)
        {
            SEPlayer.Instance.StopLoop();
            return;
        }

        // ONにした時の効果音が鳴り終わってからループを始める
        float delay = buttonView.ActionSE != null ? buttonView.ActionSEDuration : 0f;
        SEPlayer.Instance.PlayLoop(buttonView.LoopSE, delay);
    }

    /// <summary>
    /// 指定されたFeatureTypeを持つボタンを探します（Noneや見つからない場合はnull）。
    /// </summary>
    private ReactionButtonView FindButtonForFeature(ReactionFeatureType featureType)
    {
        if (featureType == ReactionFeatureType.None || reactionButtonViews == null) return null;

        for (int i = 0; i < reactionButtonViews.Length; i++)
        {
            var buttonView = reactionButtonViews[i];
            if (buttonView != null && buttonView.FeatureType == featureType) return buttonView;
        }
        return null;
    }

    /// <summary>
    /// 指定されたFeatureTypeを持つボタンを探してリアクションを実行します。
    /// </summary>
    private void ExecuteReactionForFeature(ReactionFeatureType featureType)
    {
        if (reactionButtonViews == null) return;

        for (int i = 0; i < reactionButtonViews.Length; i++)
        {
            var buttonView = reactionButtonViews[i];
            if (buttonView != null && buttonView.FeatureType == featureType)
            {
                Debug.Log($"[ReactionPresenter] トグル機能({featureType})がONになったため、保留されていたセリフを実行します。");
                ExecuteReaction(buttonView);
                break; // 同じ機能のボタンは1つと仮定
            }
        }
    }

    /// <summary>
    /// 指定されたFeatureTypeを持つボタンを探し、その表示先のセリフを即座に消去します。
    /// </summary>
    private void HideDialogueForFeature(ReactionFeatureType featureType)
    {
        if (reactionButtonViews == null) return;

        for (int i = 0; i < reactionButtonViews.Length; i++)
        {
            var buttonView = reactionButtonViews[i];
            if (buttonView != null && buttonView.FeatureType == featureType)
            {
                ReactionTextView targetView = (buttonView.DisplayTarget == ReactionDisplayTarget.AlternateScreen && alternateTextView != null) 
                                              ? alternateTextView 
                                              : reactionTextView;
                
                if (targetView != null)
                {
                    Debug.Log($"[ReactionPresenter] トグル機能({featureType})がOFFになったため、テキスト({targetView.gameObject.name})を非表示にします。");
                    targetView.HideText();
                }
                break; // 同じ機能のボタンは1つと仮定
            }
        }
    }

    /// <summary>
    /// アクション回数の増加とセリフの表示処理を行います。
    /// </summary>
    /// <param name="buttonView">実行対象のボタンView</param>
    private void ExecuteReaction(ReactionButtonView buttonView)
    {
        if (buttonView == null) return;

        // アクションキーが設定されていれば、そのアクションの実行回数を増やす
        if (!string.IsNullOrEmpty(buttonView.ActionKeyToIncrement) && mainModel != null)
        {
            mainModel.IncrementActionCount(buttonView.ActionKeyToIncrement);
        }

        // 効果音が設定されていれば鳴らす
        if (buttonView.ActionSE != null && SEPlayer.Instance != null)
        {
            SEPlayer.Instance.PlayAction(buttonView.ActionSE, buttonView.ActionSEDuration);
        }

        Debug.Log("[ReactionPresenter] リアクション実行。条件リストの数: " + (buttonView.ReactionConditions != null ? buttonView.ReactionConditions.Count : 0));
        
        float targetGaugeValue = buttonView.TargetGauge == TargetGaugeType.Gauge2 ? mainModel.Gauge2Value : mainModel.Gauge1Value;
        ShowDialogueIfConditionMet(buttonView.ReactionConditions, targetGaugeValue, buttonView.DisplayTarget);
    }

    /// <summary>
    /// 渡されたセリフ条件リストから、指定されたゲージ値およびアクション回数条件に応じた最適なセリフを表示します。
    /// </summary>
    /// <param name="conditions">判定対象のセリフ条件リスト</param>
    /// <param name="currentGauge">判定に使用する現在のゲージ値</param>
    /// <param name="displayTarget">セリフの表示先ターゲット</param>
    private void ShowDialogueIfConditionMet(IReadOnlyList<ReactionCondition> conditions, float currentGauge, ReactionDisplayTarget displayTarget = ReactionDisplayTarget.Default)
    {
        // ターゲット指定に応じて使用するテキストViewを切り替える（未設定ならデフォルトにフォールバック）
        ReactionTextView targetView = (displayTarget == ReactionDisplayTarget.AlternateScreen && alternateTextView != null) 
                                      ? alternateTextView 
                                      : reactionTextView;

        if (mainModel == null || targetView == null)
        {
            Debug.LogWarning("[ReactionPresenter] MainModel または 対象のReactionTextView への参照が未設定です。");
            return;
        }

        if (conditions == null || conditions.Count == 0)
        {
            Debug.LogWarning("[ReactionPresenter] セリフ条件（ReactionCondition）が1つも設定されていません。");
            return;
        }

        string selectedDialogue = null;
        float highestThreshold = float.MinValue;

        // LINQによるアロケーションを回避するためシンプルなforループで探索
        for (int i = 0; i < conditions.Count; i++)
        {
            var condition = conditions[i];
            if (condition == null) continue;

            // アクション回数条件の判定
            bool actionConditionsMet = true;
            if (condition.RequiredActionCounts != null)
            {
                for (int j = 0; j < condition.RequiredActionCounts.Count; j++)
                {
                    var req = condition.RequiredActionCounts[j];
                    if (req == null || string.IsNullOrEmpty(req.ActionKey)) continue;

                    int currentCount = mainModel.GetActionCount(req.ActionKey);
                    if (currentCount < req.MinCount)
                    {
                        actionConditionsMet = false;
                        break; // 1つでも満たしていない条件があれば、このセリフは不合格
                    }
                }
            }

            // 回数条件を満たしており、かつゲージ値条件を満たす中で、最も高いゲージ閾値のものを採用する
            if (actionConditionsMet && condition.MinGaugeValue <= currentGauge && condition.MinGaugeValue >= highestThreshold)
            {
                highestThreshold = condition.MinGaugeValue;
                selectedDialogue = condition.Dialogue;
            }
        }

        Debug.Log($"[ReactionPresenter] 現在のゲージ値: {currentGauge}, 算出された採用閾値: {(highestThreshold == float.MinValue ? "なし" : highestThreshold.ToString())}");

        // 条件を満たすセリフが見つかった場合に表示
        if (!string.IsNullOrEmpty(selectedDialogue))
        {
            Debug.Log($"[ReactionPresenter] セリフを表示します: {selectedDialogue} (表示先: {targetView.gameObject.name})");
            targetView.ShowText(selectedDialogue);
        }
        else
        {
            Debug.LogWarning($"[ReactionPresenter] 現在のゲージ値({currentGauge})を満たすセリフ条件が見つかりませんでした。最低閾値がゲージ値より高く設定されている可能性があります。");
        }
    }
}
