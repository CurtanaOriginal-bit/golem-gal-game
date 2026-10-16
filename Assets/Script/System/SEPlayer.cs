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

    [Tooltip("出力先の AudioMixer グループ（SE）")]
    [SerializeField] private AudioMixerGroup outputGroup;
    [Tooltip("ボタンを押したときの決定音")]
    [SerializeField] private AudioClip clickClip;
    [Tooltip("アクション音・ループ音を止めるときのフェードアウト時間(秒)")]
    [SerializeField] private float fadeOutDuration = 0.3f;

    private AudioSource _clickSource;
    private AudioSource _actionSource;
    private AudioSource _loopSource;
    private Coroutine _actionCoroutine;
    private Coroutine _loopCoroutine;
    private Coroutine _loopFadeCoroutine;

    private void Awake()
    {
        Instance = this;
        _clickSource = CreateSource(false);
        _actionSource = CreateSource(false);
        _loopSource = CreateSource(true);
    }

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

    private AudioSource CreateSource(bool loop)
    {
        var source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = loop;
        source.outputAudioMixerGroup = outputGroup;
        return source;
    }

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

    private IEnumerator StartLoopAfter(float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        _loopSource.volume = 1f;
        _loopSource.Play();
    }

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
