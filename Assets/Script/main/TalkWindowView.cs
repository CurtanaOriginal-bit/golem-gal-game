using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// 会話ウィンドウの View です。会話テキストの表示とクリックの通知を行います。
/// </summary>
public class TalkWindowView : MonoBehaviour
{
    /// <summary>
    /// 会話テキストを表示する TextMeshProUGUI
    /// </summary>
    [SerializeField] private TextMeshProUGUI talkText;

    /// <summary>
    /// 会話ウィンドウ全体のクリックを受け付けるボタン
    /// </summary>
    [SerializeField] private Button clickAreaButton;

    /// <summary>
    /// 会話ウィンドウがクリックされたときに発火するイベント
    /// </summary>
    public event Action OnWindowClicked;

    /// <summary>
    /// 有効化時にクリックイベントを購読します。
    /// </summary>
    private void OnEnable()
    {
        if (clickAreaButton != null) clickAreaButton.onClick.AddListener(HandleWindowClicked);
    }

    /// <summary>
    /// 無効化時にクリックイベントの購読を解除します。
    /// </summary>
    private void OnDisable()
    {
        if (clickAreaButton != null) clickAreaButton.onClick.RemoveListener(HandleWindowClicked);
    }

    /// <summary>
    /// 会話テキストを表示します。
    /// </summary>
    public void SetText(string text)
    {
        if (talkText != null)
        {
            talkText.text = text;
        }
    }

    /// <summary>
    /// 会話ウィンドウがクリックされたことを通知します。
    /// </summary>
    private void HandleWindowClicked() => OnWindowClicked?.Invoke();
}
