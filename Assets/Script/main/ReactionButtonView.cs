using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 【View インターフェース】リアクションボタンのUI操作イベントを公開するインターフェースです。
/// </summary>
public interface IReactionButtonView
{
    /// <summary>
    /// ボタンがクリックされた際に、自身の情報を通知するイベント
    /// </summary>
    event Action<ReactionButtonView> OnClicked;
}

/// <summary>
/// リアクション判定に使用するゲージの種類
/// </summary>
public enum TargetGaugeType
{
    Gauge1, // 興奮度ゲージ (Outside1)
    Gauge2  // 射精感ゲージ (Inside2)
}

/// <summary>
/// 【View】クリック操作を検知し、インスペクターで設定されたセリフ条件リストをPresenterへ通知するViewコンポーネントです。
/// </summary>
[RequireComponent(typeof(Button))]
public class ReactionButtonView : MonoBehaviour, IReactionButtonView
{
    /// <summary>
    /// このボタンに関連付けられた興奮度とセリフの条件リスト
    /// </summary>
    [Tooltip("興奮度に応じたセリフ条件の設定リスト")]
    [SerializeField] private List<ReactionCondition> reactionConditions = new List<ReactionCondition>();

    /// <summary>
    /// 判定対象のゲージ
    /// </summary>
    [Tooltip("このボタンのセリフ判定に使用するゲージ")]
    [SerializeField] private TargetGaugeType targetGauge = TargetGaugeType.Gauge1;

    /// <summary>
    /// ボタンが押された時に回数を加算する対象のアクションキー（空なら何もしない）
    /// </summary>
    [Tooltip("押下時に回数を増やすアクションのキー（例: Insert, Fellatio）。不要なら空欄。")]
    [SerializeField] private string actionKeyToIncrement;

    /// <summary>
    /// このボタンが特定のモード切替（脱がす・着せるなど）と連動する場合、その対象モードを設定します。
    /// （None以外の場合、クリック時にはセリフを出さず、アクション実行時に表示されます）
    /// </summary>
    [Tooltip("特定のアクション実行時（脱ぐ・着るなど）に発火させたい場合は、そのモードを設定します。即時発火の場合はNoneにします。")]
    [SerializeField] private OutfitControlMode targetControlMode = OutfitControlMode.None;

    /// <summary>
    /// 設定されたセリフ条件リストを取得します。
    /// </summary>
    public IReadOnlyList<ReactionCondition> ReactionConditions => reactionConditions;

    /// <summary>
    /// 設定された対象ゲージを取得します。
    /// </summary>
    public TargetGaugeType TargetGauge => targetGauge;

    /// <summary>
    /// 設定されたアクション加算キーを取得します。
    /// </summary>
    public string ActionKeyToIncrement => actionKeyToIncrement;

    /// <summary>
    /// アクション実行時発火の対象となるモードを取得します。
    /// </summary>
    public OutfitControlMode TargetControlMode => targetControlMode;

    private Button _button;

    /// <summary>
    /// ボタンがクリックされた際に発火するイベント
    /// </summary>
    public event Action<ReactionButtonView> OnClicked;

    private void Awake()
    {
        // パフォーマンス向上のためコンポーネントをキャッシュ
        _button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (_button != null)
        {
            _button.onClick.AddListener(HandleButtonClick);
        }
    }

    private void OnDisable()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(HandleButtonClick);
        }
    }

    /// <summary>
    /// ボタンクリック時のハンドラー。Presenterへ自身を通知します。
    /// </summary>
    private void HandleButtonClick()
    {
        // Presenterへ通知
        OnClicked?.Invoke(this);
    }
}
