using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StoryManager : MonoBehaviour
{
    [SerializeField] private StoryData[] storyDatas;
    [SerializeField] private Image background;
    [SerializeField] private Image characterImage;
    [SerializeField] private TextMeshProUGUI storyText;
    [SerializeField] private TextMeshProUGUI characterName;

    public int storyIndex { get; private set; }
    public int textIndex { get; private set; }

    // テキストがすべて表示されたかどうか
    private bool finishText = false;
    private IEnumerator _typeSentence;

    private void Start()
    {
        SetStoryElement(storyIndex, textIndex);
    }

    private void Update()
    {
        bool isSpace = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        bool isEnter = Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame;
        bool isNumpadEnter = Keyboard.current != null && Keyboard.current.numpadEnterKey.wasPressedThisFrame;
        bool isClick = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        if (isSpace || isEnter || isNumpadEnter || isClick)
        {
            if (finishText)
            {
                // 文字がすべて表示されているなら、次のセリフへ進む
                textIndex++;
                ProgressionStory(storyIndex);
            }
            else
            {
                // まだ文字がタイピング途中なら、一瞬で全文字を表示する（スキップ）
                SkipToFullText(storyIndex, textIndex);
            }
        }
    }

    private void SetStoryElement(int _storyIndex, int _textIndex)
    {
        var storyElement = storyDatas[_storyIndex].stories[_textIndex];

        background.sprite = storyElement.Background;
        characterImage.sprite = storyElement.CharacterImage;
        characterName.text = storyElement.CharacterName;

        // すでに動いているコルーチンがあれば、安全に停止させる
        if (_typeSentence != null)
        {
            StopCoroutine(_typeSentence);
        }

        _typeSentence = TypeSentence(_storyIndex, _textIndex);
        StartCoroutine(_typeSentence);
    }

    private void ProgressionStory(int _storyIndex)
    {
        characterName.text = "";

        // まだ現在のストーリーの中に次のセリフがある場合
        if (textIndex < storyDatas[_storyIndex].stories.Count)
        {
            SetStoryElement(_storyIndex, textIndex);
        }
        else
        {
            // 次のストーリーデータ（章）へ進む
            textIndex = 0;
            storyIndex++;

            // 次のストーリーデータが「まだ存在するか」をチェックする
            if (storyIndex < storyDatas.Length)
            {
                // 次のデータがあるなら再生
                SetStoryElement(storyIndex, textIndex);
            }
            else
            {
                // すべてのストーリーが完全に終了した場合の処理
                Debug.Log("【ゲーム終了】すべてのストーリーを読み終えました！");

                // エラーでフリーズするのを防ぐため、インデックスを最後のセリフで止めておく
                storyIndex = storyDatas.Length - 1;
                textIndex = storyDatas[storyIndex].stories.Count - 1;

                // TODO: ここに「タイトル画面に戻る」や「エンディングに移る」などの処理を書く
                SceneManager.LoadScene("main_1");
            }
        }
    }

    // 文字を1文字づつ表示するコルーチン
    private IEnumerator TypeSentence(int _storyIndex, int _textIndex)
    {
        finishText = false; // タイピング開始なのでフラグをfalseに
        storyText.text = "";

        foreach (var letter in storyDatas[_storyIndex].stories[_textIndex].StoryText.ToCharArray())
        {
            storyText.text += letter;
            yield return new WaitForSeconds(0.05f);
        }

        finishText = true; // 全部表示し終わったらtrueにする
    }

    // 文字を瞬間全表示するメソッド
    private void SkipToFullText(int _storyIndex, int _textIndex)
    {
        // 動いているコルーチンを止める
        if (_typeSentence != null)
        {
            StopCoroutine(_typeSentence);
        }

        // テキストデータの中身をそのまま全表示
        storyText.text = storyDatas[_storyIndex].stories[_textIndex].StoryText;

        finishText = true; // 表示完了フラグを立てる
    }
}