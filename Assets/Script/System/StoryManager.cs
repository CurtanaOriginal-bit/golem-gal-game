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

    [SerializeField] private Button skipButton;

    // シーン遷移用の暗転UI設定
    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float sceneChangeFadeDuration = 1.0f;

    public int storyIndex { get; private set; }
    public int textIndex { get; private set; }

    private bool finishText = false;
    private bool isTransitioning = false;
    private IEnumerator _typeSentence;

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
        SetStoryElement(storyIndex, textIndex);
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

    private void SetStoryElement(int _storyIndex, int _textIndex)
    {
        var storyElement = storyDatas[_storyIndex].stories[_textIndex];

        // --- 背景画像の処理 ---
        // データに新しい画像がある場合のみ更新（null なら前の背景画像をそのまま表示維持）
        if (storyElement.Background != null)
        {
            background.enabled = true;
            background.sprite = storyElement.Background;
        }

        // --- キャラクター画像の処理 ---
        // データに新しい画像がある場合のみ更新（null なら前のキャラ画像をそのまま表示維持）
        if (storyElement.CharacterImage != null)
        {
            characterImage.enabled = true;
            characterImage.sprite = storyElement.CharacterImage;
        }

        characterName.text = storyElement.CharacterName;

        // テキストのタイピング表示
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

        if (textIndex < storyDatas[_storyIndex].stories.Count)
        {
            SetStoryElement(_storyIndex, textIndex);
        }
        else
        {
            textIndex = 0;
            storyIndex++;

            if (storyIndex < storyDatas.Length)
            {
                SetStoryElement(storyIndex, textIndex);
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

    private IEnumerator TypeSentence(int _storyIndex, int _textIndex)
    {
        finishText = false;
        storyText.text = "";

        foreach (var letter in storyDatas[_storyIndex].stories[_textIndex].StoryText.ToCharArray())
        {
            storyText.text += letter;
            yield return new WaitForSeconds(0.05f);
        }

        finishText = true;
    }

    private void SkipToFullText(int _storyIndex, int _textIndex)
    {
        if (_typeSentence != null)
        {
            StopCoroutine(_typeSentence);
        }

        storyText.text = storyDatas[_storyIndex].stories[_textIndex].StoryText;
        finishText = true;
    }

    public void OnClickSkipButton()
    {
        if (isTransitioning) return;

        if (_typeSentence != null)
        {
            StopCoroutine(_typeSentence);
        }

        StartCoroutine(FadeAndLoadScene());
    }

    // シーン遷移用の暗転処理
    private IEnumerator FadeAndLoadScene()
    {
        isTransitioning = true;

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.blocksRaycasts = true;
            float elapsedTime = 0f;

            while (elapsedTime < sceneChangeFadeDuration)
            {
                elapsedTime += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / sceneChangeFadeDuration);
                yield return null;
            }

            fadeCanvasGroup.alpha = 1f;
        }

        SceneManager.LoadScene(nextSceneName);
    }
}