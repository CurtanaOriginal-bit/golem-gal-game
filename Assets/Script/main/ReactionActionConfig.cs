using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 【DTO / Data】特定のアクション実行時に表示するセリフ設定をまとめたデータ構造です。
/// </summary>
[Serializable]
public class ReactionActionConfig
{
    /// <summary>
    /// 対象となる着替えアクションの種類
    /// </summary>
    [Tooltip("どのアクションが実行された時に発動するか")]
    public OutfitActionType ActionType;

    /// <summary>
    /// そのアクションに対する興奮度ごとのセリフ設定リスト
    /// </summary>
    [Tooltip("興奮度ごとのセリフリスト")]
    public List<ReactionCondition> Conditions = new List<ReactionCondition>();
}
