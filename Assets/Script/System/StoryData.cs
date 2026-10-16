using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 導入ストーリーのセリフ一覧を保持する ScriptableObject です。
/// </summary>
[CreateAssetMenu(fileName = "New Data", menuName = "StoryData")]
public class StoryData : ScriptableObject
{
    /// <summary>
    /// 表示順に並んだセリフのリスト
    /// </summary>
    public List<Story> stories = new List<Story>();
}

/// <summary>
/// ストーリーの 1 セリフ分のデータです。
/// </summary>
[Serializable]
public class Story
{
    /// <summary>
    /// このセリフで表示する背景画像
    /// </summary>
    public Sprite Background;

    /// <summary>
    /// このセリフで表示するキャラクター画像
    /// </summary>
    public Sprite CharacterImage;

    /// <summary>
    /// 表示するセリフ本文
    /// </summary>
    [TextArea]
    public string StoryText;

    /// <summary>
    /// 話しているキャラクターの名前
    /// </summary>
    public string CharacterName;

    /// <summary>
    /// このセリフを表示する直前に画面を暗転させるかどうか
    /// </summary>
    [Header("演出設定")]
    [Tooltip("チェックを入れると、このセリフを表示する直前に画面を暗転させます")]
    public bool isFadeIn;
}
