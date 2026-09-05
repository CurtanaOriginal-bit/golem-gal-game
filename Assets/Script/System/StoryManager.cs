using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// MVP形式を使用せず、単一でシナリオの進行とUI表示制御を担うマネージャークラスです。
/// </summary>
public class StoryManager : MonoBehaviour
{
    /// <summary>ストーリーデータの配列</summary>
    [SerializeField] private StoryData[] storyDatas;
    /// <summary>背景画像を表示するImage</summary>
    [SerializeField] private Image background;
    /// <summary>キャラクター画像を表示するImage</summary>
    [SerializeField] private Image characterImage;
    /// <summary>シナリオテキストを表示するTextMeshProUGUI</summary>
    [SerializeField] private TextMeshProUGUI storyText;
    /// <summary>キャラクター名を表示するTextMeshProUGUI</summary>
    [SerializeField] private TextMeshProUGUI characterName;

    /// <summary>スキップ用ボタン</summary>
    [SerializeField] private Button skipButton;

    // 暗転UI設定
    [Header("Fade Settings")]
    /// <summary>フェード演出用CanvasGroup</summary>
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    /// <summary>シーン遷移時のフェード演出にかかる全体の時間(秒)</summary>
    [SerializeField] private float sceneChangeFadeDuration = 1.0f;
    /// <summary>セリフ間の暗転演出にかかる全体の時間(秒)</summary>
    [SerializeField] private float storyElementFadeDuration = 1.0f;

    /// <summary>現在のストーリーデータ配列のインデックス</summary>
    public int storyIndex { get; private set; }
    /// <summary>現在のストーリー内のテキストインデックス</summary>
    public int textIndex { get; private set; }

    private bool finishText = false;
    private bool isTransitioning = false;
    private IEnumerator _typeSentence;

    /// <summary>次に遷移するシーン名</summary>
    [SerializeField] private string nextSceneName = "main_1";

    private void Start()
    {
        // 1. 暗転パネルの初期化（非表示・透明にしておく）
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }

        // 2. 画像の初期化（Inspectorにセットされているデフォルト画像を非表示にする）
        if (background != null)
        {
            background.enabled = false;
        }
        if (characterImage != null)
        {
            characterImage.enabled = false;
        }

        // 3. 最初のセリフの読み込み（ここでデータがあれば表示される）
        StartCoroutine(PlayStoryElement(storyIndex, textIndex));
    }

    private void OnEnable()
    {
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(OnClickSkipButton);
        }
    }

    private void OnDisable()
    {
        if (skipButton != null)
        {
            skipButton.onClick.RemoveListener(OnClickSkipButton);
        }
    }

    private void Update()
    {
        if (isTransitioning) return;

        bool isSpace = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        bool isEnter = Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame;
        bool isNumpadEnter = Keyboard.current != null && Keyboard.current.numpadEnterKey.wasPressedThisFrame;
        bool isClick = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        if (isSpace || isEnter || isNumpadEnter || isClick)
        {
            if (finishText)
            {
                textIndex++;
                ProgressionStory(storyIndex);
            }
            else
            {
                SkipToFullText(storyIndex, textIndex);
            }
        }
    }

    /// <summary>
    /// ストーリー要素を再生し、必要であれば暗転演出を挟んでUIを更新します。
    /// </summary>
    private IEnumerator PlayStoryElement(int _storyIndex, int _textIndex)
    {
        var storyElement = storyDatas[_storyIndex].stories[_textIndex];

        // 演出フラグによるフェードアウト・インの処理
        if (storyElement.isFadeIn && fadeCanvasGroup != null)
        {
            isTransitioning = true; // 演出中は入力をロック

            // 画面を暗転（フェードアウト）
            yield return StartCoroutine(FadeCanvas(fadeCanvasGroup.alpha, 1f, storyElementFadeDuration / 2f));
            
            // 暗転中にUIを更新
            UpdateUIElements(storyElement);
            storyText.text = ""; // タイピング開始前なのでテキストをクリア

            // 画面を明転（フェードイン）
            yield return StartCoroutine(FadeCanvas(1f, 0f, storyElementFadeDuration / 2f));
            
            isTransitioning = false; // 入力ロック解除
        }
        else
        {
            // フェードなしの場合は即座にUI更新
            UpdateUIElements(storyElement);
        }

        // テキストのタイピング表示
        if (_typeSentence != null)
        {
            StopCoroutine(_typeSentence);
        }

        _typeSentence = TypeSentence(_storyIndex, _textIndex);
        StartCoroutine(_typeSentence);
    }

    /// <summary>
    /// UI要素（背景、キャラクター画像、名前）を更新します。
    /// </summary>
    private void UpdateUIElements(Story storyElement)
    {
        // 背景：新しい画像がある場合のみ更新（未設定なら前の背景を維持）
        if (storyElement.Background != null)
        {
            background.enabled = true;
            background.sprite = storyElement.Background;
        }

        // キャラクター画像：設定されていれば表示、未設定(null)なら非表示にして消す
        if (storyElement.CharacterImage != null)
        {
            characterImage.enabled = true;
            characterImage.sprite = storyElement.CharacterImage;
        }
        else
        {
            characterImage.enabled = false;
        }

        characterName.text = storyElement.CharacterName;
    }

    /// <summary>
    /// 次のセリフや次のストーリーデータへ進行します。
    /// </summary>
    private void ProgressionStory(int _storyIndex)
    {
        characterName.text = "";

        if (textIndex < storyDatas[_storyIndex].stories.Count)
        {
            StartCoroutine(PlayStoryElement(_storyIndex, textIndex));
        }
        else
        {
            textIndex = 0;
            storyIndex++;

            if (storyIndex < storyDatas.Length)
            {
                StartCoroutine(PlayStoryElement(storyIndex, textIndex));
            }
            else
            {
                Debug.Log("【ゲーム終了】すべてのストーリーを読み終えました！");

                storyIndex = storyDatas.Length - 1;
                textIndex = storyDatas[storyIndex].stories.Count - 1;

                // 終了時は暗転を挟んでシーン遷移
                StartCoroutine(FadeAndLoadScene());
            }
        }
    }

    /// <summary>
    /// テキストを1文字ずつ表示するコルーチン
    /// </summary>
    private IEnumerator TypeSentence(int _storyIndex, int _textIndex)
    {
        finishText = false;
        storyText.text = "";

        // 文字列の1文字ずつを処理
        foreach (var letter in storyDatas[_storyIndex].stories[_textIndex].StoryText.ToCharArray())
        {
            storyText.text += letter;
            yield return new WaitForSeconds(0.05f);
        }

        finishText = true;
    }

    /// <summary>
    /// テキストを一気に全文表示します
    /// </summary>
    private void SkipToFullText(int _storyIndex, int _textIndex)
    {
        if (_typeSentence != null)
        {
            StopCoroutine(_typeSentence);
        }

        storyText.text = storyDatas[_storyIndex].stories[_textIndex].StoryText;
        finishText = true;
    }

    /// <summary>
    /// スキップボタンが押された時の処理
    /// </summary>
    public void OnClickSkipButton()
    {
        if (isTransitioning) return;

        if (_typeSentence != null)
        {
            StopCoroutine(_typeSentence);
        }

        StartCoroutine(FadeAndLoadScene());
    }

    /// <summary>
    /// 任意のアルファ値へCanvasGroupをフェードさせる汎用コルーチン
    /// </summary>
    private IEnumerator FadeCanvas(float startAlpha, float endAlpha, float duration)
    {
        if (fadeCanvasGroup == null) yield break;

        fadeCanvasGroup.blocksRaycasts = true;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            yield return null;
        }

        fadeCanvasGroup.alpha = endAlpha;
        
        // 完全に透明になった場合はクリックブロックを解除
        if (endAlpha <= 0f)
        {
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// シーン遷移用の暗転処理
    /// </summary>
    private IEnumerator FadeAndLoadScene()
    {
        isTransitioning = true;

        if (fadeCanvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvas(fadeCanvasGroup.alpha, 1f, sceneChangeFadeDuration));
        }

        SceneManager.LoadScene(nextSceneName);
    }
}