using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StoryManager : MonoBehaviour
{
    [SerializeField] private StoryData[] storyDatas;

    [SerializeField] private Image background;

    [SerializeField] private Image characterImage;

    [SerializeField] private TextMeshProUGUI storyText;

    [SerializeField] private TextMeshProUGUI characterName;

    //ストーリーのエレメント配列番号が必要なのでプロパティ
    public int storyIndex { get; private set; }

    public int textIndex { get; private set; }

    private void Start()
    {
        SetStoryElement(storyIndex, textIndex);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            textIndex++; //インデックスを増やす

            //テキスト部を初期化
            storyText.text = "";
            characterName.text = "";
            SetStoryElement(storyIndex, textIndex);
        }
    }

    //呼び出しメソッド
    private void SetStoryElement(int _storyIndex, int _textIndex)
    {
        if (_storyIndex < storyDatas.Length) {
            if (storyDatas[_storyIndex].stories.Count > _textIndex) {
                //同じ言葉をまとめておくためのvar
                var storyElement = storyDatas[_storyIndex].stories[_textIndex];
                //どのストーリーデータの、どのバックグランドか
                background.sprite = storyElement.Background;
                //どのストーリーデータの、どのキャラクタか
                characterImage.sprite = storyElement.CharacterImage;
                //どのストーリーデータの、どのテキストか
                storyText.text = storyElement.StoryText;
                //どのストーリーデータの、どのキャラ名か
                characterName.text = storyElement.CharacterName;
            }
            else
            {
                Debug.Log("out !!!");
            }
        }
        else
        {
            Debug.Log("out !!!");
        }
    }
}