using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Data", menuName = "StoryData")]
public class StoryData : ScriptableObject
{
    public List<Story> stories = new List<Story>();
}
[System.Serializable]
public class Story
{
    public Sprite Background;
    public Sprite CharacterImage;
    [TextArea]
    public string StoryText;
    public string CharacterName;

    // --- 追加：このセリフの前に暗転を入れるかのフラグ ---
    [Header("演出設定")]
    [Tooltip("チェックを入れると、このセリフを表示する直前に画面を暗転させます")]
    public bool isFadeIn;
}