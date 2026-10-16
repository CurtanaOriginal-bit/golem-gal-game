using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 効果音（SE）を再生するクラス。シーンに1つ配置して使う。
/// 決定音・数秒だけ鳴らすアクション音・ループ音の3系統を、AudioMixer の SE グループに流す。
/// </summary>
public class SEPlayer : MonoBehaviour
{
    /// <summary>シーン内の SEPlayer（未配置なら null）</summary>
    public static SEPlayer Instance { get; private set; }

    /// <summary>
    /// 効果音の出力先となる AudioMixer グループ（SE）
    /// </summary>
    [Tooltip("出力先の AudioMixer グループ（SE）")]
    [SerializeField] private AudioMixerGroup outputGroup;

    /// <summary>
    /// ボタンを押したときの決定音
    /// </summary>
    [Tooltip("ボタンを押したときの決定音")]
    [SerializeField] private AudioClip clickClip;

    /// <summary>
    /// アクション音・ループ音を止めるときのフェードアウト時間(秒)
    /// </summary>
    [Tooltip("アクション音・ループ音を止めるときのフェードアウト時間(秒)")]
    [SerializeField] private float fadeOutDuration = 0.3f;

    /// <summary>
    /// 決定音を鳴らす AudioSource
    /// </summary>
    private AudioSource _clickSource;

    /// <summary>
    /// アクション音を鳴らす AudioSource
    /// </summary>
    private AudioSource _actionSource;

    /// <summary>
    /// ループ音を鳴らす AudioSource
    /// </summary>
    private AudioSource _loopSource;

    /// <summary>
    /// 実行中のアクション音の再生処理（未実行なら null）
    /// </summary>
    private Coroutine _actionCoroutine;

    /// <summary>
    /// ループ音の再生処理（ループ中でなければ null）
    /// </summary>
    private Coroutine _loopCoroutine;

    /// <summary>
    /// 実行中のループ音のフェードアウト処理（未実行なら null）
    /// </summary>
    private Coroutine _loopFadeCoroutine;

    /// <summary>
    /// 自身を Instance に登録し、3 系統の AudioSource を作成します。
    /// </summary>
    private void Awake()
    {
        Instance = this;
        _clickSource = CreateSource(false);
        _actionSource = CreateSource(false);
        _loopSource = CreateSource(true);
    }

    /// <summary>
    /// 破棄時に Instance の登録を解除します。
    /// </summary>
    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// 決定音を再生します。
    /// </summary>
    public void PlayClick()
    {
        if (clickClip != null) _clickSource.PlayOneShot(clickClip);
    }

    /// <summary>
    /// 指定した音を、先頭から指定秒数だけ再生します（最後はフェードアウト）。再生中のアクション音は止めて差し替えます。
    /// </summary>
    public void PlayAction(AudioClip clip, float duration)
    {
        if (clip == null) return;

        if (_actionCoroutine != null) StopCoroutine(_actionCoroutine);
        _actionCoroutine = StartCoroutine(PlayForSeconds(_actionSource, clip, duration));
    }

    /// <summary>
    /// 指定した音をループ再生します。同じ音がすでに鳴っている場合は何もしません。
    /// </summary>
    /// <param name="clip">ループさせる音（null なら停止）</param>
    /// <param name="delay">鳴らし始めるまでの待ち時間(秒)</param>
    public void PlayLoop(AudioClip clip, float delay)
    {
        if (clip == null)
        {
            StopLoop();
            return;
        }
        if (_loopCoroutine != null && _loopSource.clip == clip) return;

        if (_loopCoroutine != null) StopCoroutine(_loopCoroutine);
        // 停止中のフェードアウトが新しいループを止めないように打ち切る
        if (_loopFadeCoroutine != null)
        {
            StopCoroutine(_loopFadeCoroutine);
            _loopFadeCoroutine = null;
        }
        _loopSource.Stop();
        _loopSource.clip = clip;
        _loopCoroutine = StartCoroutine(StartLoopAfter(delay));
    }

    /// <summary>
    /// ループ音をフェードアウトして止めます。
    /// </summary>
    public void StopLoop()
    {
        if (_loopCoroutine == null) return;

        StopCoroutine(_loopCoroutine);
        _loopCoroutine = null;
        _loopFadeCoroutine = StartCoroutine(FadeOutAndStop(_loopSource));
    }

    /// <summary>
    /// 出力先を SE グループにした AudioSource を作成します。
    /// </summary>
    private AudioSource CreateSource(bool loop)
    {
        var source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = loop;
        source.outputAudioMixerGroup = outputGroup;
        return source;
    }

    /// <summary>
    /// 音を先頭から duration 秒だけ再生し、最後はフェードアウトして止めます。
    /// </summary>
    private IEnumerator PlayForSeconds(AudioSource source, AudioClip clip, float duration)
    {
        source.Stop();
        source.volume = 1f;
        source.clip = clip;
        source.Play();

        yield return new WaitForSeconds(Mathf.Max(0f, duration - fadeOutDuration));
        yield return FadeOutAndStop(source);
        _actionCoroutine = null;
    }

    /// <summary>
    /// delay 秒待ってからループ音の再生を始めます。
    /// </summary>
    private IEnumerator StartLoopAfter(float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        _loopSource.volume = 1f;
        _loopSource.Play();
    }

    /// <summary>
    /// 音量を fadeOutDuration 秒かけて 0 にしてから停止し、音量を元に戻します。
    /// </summary>
    private IEnumerator FadeOutAndStop(AudioSource source)
    {
        float startVolume = source.volume;
        for (float t = 0f; t < fadeOutDuration; t += Time.deltaTime)
        {
            source.volume = Mathf.Lerp(startVolume, 0f, t / fadeOutDuration);
            yield return null;
        }
        source.Stop();
        source.volume = 1f;
    }
}
