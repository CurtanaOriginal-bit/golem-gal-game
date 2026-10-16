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
    /// <summary>興奮度ゲージ (Outside1)</summary>
    Gauge1,

    /// <summary>射精感ゲージ (Inside2)</summary>
    Gauge2
}

/// <summary>
/// セリフを表示するターゲットの場所の指定用
/// </summary>
public enum ReactionDisplayTarget
{
    /// <summary>通常の表示先</summary>
    Default,

    /// <summary>別の場所（Order_Blowjobなどのサブスクリーン用）</summary>
    AlternateScreen
}

/// <summary>
/// ボタンが持つ特殊な役割（ON/OFF切り替えなど）を指定する列挙型
/// </summary>
public enum ReactionFeatureType
{
    /// <summary>通常のクリックボタン</summary>
    None,

    /// <summary>挿入トグルボタン</summary>
    PenetrationToggle,

    /// <summary>フェラチオトグルボタン</summary>
    BlowjobToggle
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
    /// このボタンによるセリフの表示場所
    /// </summary>
    [Tooltip("セリフを表示する場所（Default以外を指定すると、Presenterで設定した別枠に表示されます）")]
    [SerializeField] private ReactionDisplayTarget displayTarget = ReactionDisplayTarget.Default;

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
    /// このボタンがON/OFF状態を持つ機能のトグルボタンである場合、その種類を設定します。
    /// </summary>
    [Tooltip("ON/OFFを切り替えるトグルボタン（挿入・フェラチオ）の場合に設定します。無効化時にはセリフが表示されなくなります。")]
    [SerializeField] private ReactionFeatureType featureType = ReactionFeatureType.None;

    /// <summary>
    /// リアクション実行時に鳴らす効果音（トグルボタンの場合はONにした時に鳴らす）
    /// </summary>
    [Tooltip("リアクション実行時に鳴らす効果音。不要なら空欄。")]
    [SerializeField] private AudioClip actionSE;

    /// <summary>
    /// 効果音を鳴らす秒数（長い素材でも先頭からこの秒数だけ鳴らす）
    /// </summary>
    [Tooltip("効果音を鳴らす秒数（長い素材でも先頭からこの秒数だけ鳴らします）")]
    [SerializeField] private float actionSEDuration = 3f;

    /// <summary>
    /// トグルボタンがONの間ループ再生する効果音
    /// </summary>
    [Tooltip("トグルボタン（挿入・フェラチオ）がONの間ループ再生する効果音。ONにした時の効果音が鳴り終わってから始まります。")]
    [SerializeField] private AudioClip loopSE;

    /// <summary>
    /// 設定されたセリフ条件リストを取得します。
    /// </summary>
    public IReadOnlyList<ReactionCondition> ReactionConditions => reactionConditions;

    /// <summary>
    /// 設定された対象ゲージを取得します。
    /// </summary>
    public TargetGaugeType TargetGauge => targetGauge;

    /// <summary>
    /// セリフの表示先ターゲットを取得します。
    /// </summary>
    public ReactionDisplayTarget DisplayTarget => displayTarget;

    /// <summary>
    /// 設定されたアクション加算キーを取得します。
    /// </summary>
    public string ActionKeyToIncrement => actionKeyToIncrement;

    /// <summary>
    /// アクション実行時発火の対象となるモードを取得します。
    /// </summary>
    public OutfitControlMode TargetControlMode => targetControlMode;

    /// <summary>
    /// このボタンの特殊機能のタイプを取得します。
    /// </summary>
    public ReactionFeatureType FeatureType => featureType;

    /// <summary>
    /// リアクション実行時に鳴らす効果音を取得します。
    /// </summary>
    public AudioClip ActionSE => actionSE;

    /// <summary>
    /// 効果音を鳴らす秒数を取得します。
    /// </summary>
    public float ActionSEDuration => actionSEDuration;

    /// <summary>
    /// トグルON中にループ再生する効果音を取得します。
    /// </summary>
    public AudioClip LoopSE => loopSE;

    /// <summary>
    /// クリックを検知する Button コンポーネント
    /// </summary>
    private Button _button;

    /// <summary>
    /// ボタンがクリックされた際に発火するイベント
    /// </summary>
    public event Action<ReactionButtonView> OnClicked;

    /// <summary>
    /// Button コンポーネントを取得してキャッシュします。
    /// </summary>
    private void Awake()
    {
        // パフォーマンス向上のためコンポーネントをキャッシュ
        _button = GetComponent<Button>();
    }

    /// <summary>
    /// 有効化時にボタンのクリックイベントを購読します。
    /// </summary>
    private void OnEnable()
    {
        if (_button != null)
        {
            _button.onClick.AddListener(HandleButtonClick);
        }
    }

    /// <summary>
    /// 無効化時にボタンのクリックイベントの購読を解除します。
    /// </summary>
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
