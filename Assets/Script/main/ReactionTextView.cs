using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 【View インターフェース】セリフテキストの表示とフェードアウト演出を命令するためのインターフェースです。
/// </summary>
public interface IReactionTextView
{
    /// <summary>
    /// 指定されたテキストを表示し、フェードアウトを開始します。
    /// </summary>
    /// <param name="text">表示するセリフ</param>
    void ShowText(string text);
}

/// <summary>
/// 【View】セリフテキストを表示し、指定時間をかけて滑らかにフェードアウトさせるViewコンポーネントです。
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class ReactionTextView : MonoBehaviour, IReactionTextView
{
    /// <summary>
    /// セリフを表示するTextMeshProUGUIコンポーネント
    /// </summary>
    [Tooltip("セリフを表示するTextMeshProUGUI")]
    [SerializeField] private TextMeshProUGUI reactionText;

    /// <summary>
    /// 表示されてから完全に消えるまでの秒数
    /// </summary>
    [Tooltip("表示されてから完全に消えるまでの秒数")]
    [SerializeField] private float fadeDuration = 2.0f;

    private CanvasGroup _canvasGroup;
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        // パフォーマンス向上のためコンポーネントをキャッシュ
        _canvasGroup = GetComponent<CanvasGroup>();

        // 初期状態は完全に非表示にして、クリック等を透過する
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }

    private void OnDisable()
    {
        // 非アクティブ化時に実行中のコルーチンを停止
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = null;
        }
    }

    /// <summary>
    /// 指定されたテキストを設定し、フェードアウト処理を開始します。
    /// すでにフェード中の場合はリセットして最初から再生します。
    /// </summary>
    /// <param name="text">表示するセリフ内容</param>
    public void ShowText(string text)
    {
        if (reactionText != null)
        {
            reactionText.text = text;
        }

        // すでにフェード中の場合は一度中断して再開
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }

        _fadeCoroutine = StartCoroutine(FadeOutRoutine());
    }

    /// <summary>
    /// アルファ値を徐々に下げてフェードアウトさせるコルーチン
    /// </summary>
    private IEnumerator FadeOutRoutine()
    {
        if (_canvasGroup == null)
        {
            yield break;
        }

        // 表示開始時は瞬時に不透明にする
        _canvasGroup.alpha = 1f;

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            // 経過時間に応じてアルファ値を 1.0 から 0.0 へ線形補間
            _canvasGroup.alpha = Mathf.Clamp01(1f - (timer / fadeDuration));
            yield return null;
        }

        // 終了時は確実に完全に透明にする
        _canvasGroup.alpha = 0f;
        _fadeCoroutine = null;
    }
}
