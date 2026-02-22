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
}