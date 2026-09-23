using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 【View】着替え等の「アクション実行時」のセリフ設定をインスペクターから登録し、Presenterへ提供するコンポーネントです。
/// </summary>
public class ReactionOutfitActionView : MonoBehaviour
{
    /// <summary>
    /// 各アクション種別とそれに対するセリフ条件の設定リスト
    /// </summary>
    [Tooltip("アクション種別ごとのセリフ設定リスト")]
    [SerializeField] private List<ReactionActionConfig> actionConfigs = new List<ReactionActionConfig>();

    /// <summary>
    /// 外部（Presenterなど）から設定リストを取得するためのプロパティ
    /// </summary>
    public IReadOnlyList<ReactionActionConfig> ActionConfigs => actionConfigs;
}
