using UnityEngine;
using System.Collections;

/// <summary>
/// 表示した UI をしばらくかけてフェードアウトさせるエフェクトです。
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class UIFadeEffect : MonoBehaviour
{
    /// <summary>
    /// 表示されてから完全に消えるまでの秒数
    /// </summary>
    [Tooltip("表示されてから完全に消えるまでの秒数")]
    [SerializeField] private float fadeDuration = 2.0f;
    
    /// <summary>
    /// 表示・非表示を切り替える CanvasGroup
    /// </summary>
    private CanvasGroup _canvasGroup;

    /// <summary>
    /// 実行中のフェードアウト処理（未実行なら null）
    /// </summary>
    private Coroutine _fadeCoroutine;

    /// <summary>
    /// CanvasGroup を取得し、初期状態を非表示（クリックも透過）にします。
    /// </summary>
    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        
        // 最初は非表示にしておく
        _canvasGroup.alpha = 0f;
        // マウスのクリック等の邪魔にならないようにブロックを無効化
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// エフェクトを表示し、フェードアウトを開始する
    /// （ボタンのOnClickなどから呼び出します）
    /// </summary>
    public void ShowEffect()
    {
        // もし既にフェード中の場合は一度ストップする
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }
        
        // 新しくフェードアウト処理を開始
        _fadeCoroutine = StartCoroutine(FadeOutRoutine());
    }

    /// <summary>
    /// 一瞬で表示し、fadeDuration 秒かけて透明にします。
    /// </summary>
    private IEnumerator FadeOutRoutine()
    {
        // 表示開始時は一瞬で不透明(1.0)にする
        _canvasGroup.alpha = 1f;

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            // 経過時間に応じてアルファ値を 1.0 から 0.0 へ徐々に下げる
            _canvasGroup.alpha = 1f - (timer / fadeDuration);
            
            // 次のフレームまで待機
            yield return null;
        }

        // 最後に完全に透明(0.0)にして終了
        _canvasGroup.alpha = 0f;
        _fadeCoroutine = null;
    }
}
