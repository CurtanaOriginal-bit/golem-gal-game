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

    //テキストがすべて表示されたかどうか
    private bool finishText = false;

    private void Start()
    {
        SetStoryElement(storyIndex, textIndex);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            textIndex++; //インデックスを増やす

            ProgressionStory(storyIndex);
        }
    }

    //呼び出しメソッド
    private void SetStoryElement(int _storyIndex, int _textIndex)
    {
        //同じ言葉をまとめておくためのvar
        var storyElement = storyDatas[_storyIndex].stories[_textIndex];

        //どのストーリーデータの、どのバックグランドか
        background.sprite = storyElement.Background;

        //どのストーリーデータの、どのキャラクタか
        characterImage.sprite = storyElement.CharacterImage;

        //どのストーリーデータの、どのテキストか
        //1文字づつ表示するコルーチン
        StartCoroutine(TypeSentence(_storyIndex, _textIndex));

        //どのストーリーデータの、どのキャラ名か
        characterName.text = storyElement.CharacterName;
    }

    private void ProgressionStory(int _storyIndex)
    {
        characterName.text = "";

        //ストーリーインデックスよりも大きいテキストは存在しないのでチェックして対応
        //最後まで行ったなら、次のお話などに進めたいですよね
        if (textIndex < storyDatas[_storyIndex].stories.Count)
        {
            //まだ大きくないなら次のインデックスを表示
            SetStoryElement(_storyIndex, textIndex);
        }
        else
        {
            //シーンチェンジや選択肢の表示。スクリプタブルオブジェクトを呼んだり。
            textIndex = 0;
            storyIndex++;//次のシーンへ
            SetStoryElement(storyIndex, textIndex);
        }
    }

    //文字を1文字づつ表示するコルーチン
    private IEnumerator TypeSentence(int _storyIndex, int _textIndex)
    {       
        //テキスト部を初期化
        storyText.text = "";

        //１文字づつ文字を分割した状態にする
        foreach (var letter in storyDatas[_storyIndex].stories[_textIndex].StoryText.ToCharArray())
        {
            storyText.text += letter;//1文字表示
            yield return new WaitForSeconds(0.05f);
        }
    }
}