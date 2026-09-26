using System;
using UnityEngine;

/// <summary>
/// 【DTO / Data】特定のセリフを表示するために必要な「アクション（ボタンなど）」の実行回数条件を定義します。
/// </summary>
[Serializable]
public class RequiredActionCount
{
    /// <summary>
    /// 対象となるアクションのキー文字列（例: "Insert", "Fellatio"）
    /// </summary>
    [Tooltip("判定対象のアクションキー（ReactionButtonViewで設定したものと一致させる必要があります）")]
    public string ActionKey;

    /// <summary>
    /// セリフ表示に必要な最低実行回数
    /// </summary>
    [Tooltip("セリフ表示に必要な最低実行回数")]
    public int MinCount;
}
