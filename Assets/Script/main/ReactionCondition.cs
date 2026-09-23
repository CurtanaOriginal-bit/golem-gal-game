using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 【DTO / Model】興奮度の閾値とアクション回数条件、およびそれに対応するセリフデータを保持するデータクラスです。
/// </summary>
[Serializable]
public class ReactionCondition
{
    /// <summary>
    /// セリフが表示される最小の興奮度値
    /// </summary>
    [Tooltip("セリフが表示される最小の興奮度（0〜100）")]
    [SerializeField] private float minGaugeValue;

    /// <summary>
    /// 特定のアクションが指定回数以上実行されているかどうかの条件リスト
    /// </summary>
    [Tooltip("特定のボタンが規定回数押されているか等の追加条件")]
    [SerializeField] private List<RequiredActionCount> requiredActionCounts = new List<RequiredActionCount>();

    /// <summary>
    /// 表示するセリフ内容
    /// </summary>
    [Tooltip("表示するセリフ")]
    [TextArea(2, 4)]
    [SerializeField] private string dialogue;

    /// <summary>
    /// セリフが表示される最小の興奮度値を取得します。
    /// </summary>
    public float MinGaugeValue => minGaugeValue;

    /// <summary>
    /// 特定のアクションの要求回数リストを取得します。
    /// </summary>
    public IReadOnlyList<RequiredActionCount> RequiredActionCounts => requiredActionCounts;

    /// <summary>
    /// 表示するセリフ内容を取得します。
    /// </summary>
    public string Dialogue => dialogue;

    /// <summary>
    /// ReactionCondition のコンストラクタです。
    /// </summary>
    /// <param name="minGaugeValue">最小興奮度</param>
    /// <param name="dialogue">セリフ内容</param>
    public ReactionCondition(float minGaugeValue, string dialogue)
    {
        this.minGaugeValue = minGaugeValue;
        this.dialogue = dialogue;
    }
}
