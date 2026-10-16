using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 配下のボタンすべてに、押したときの決定音を付けるクラス。Canvas やプレハブのルートに付けて使う。
/// 音はシーンの SEPlayer から鳴らす（SEPlayer がないシーンでは鳴らない）。
/// </summary>
public class ButtonClickSE : MonoBehaviour
{
    [Tooltip("決定音を付けないボタン（キャラクターのクリック判定など）")]
    [SerializeField] private Button[] excludedButtons;

    private Button[] _buttons;

    private void Awake()
    {
        // 非表示のボタンも、あとで表示されたときに鳴るよう含めて登録する
        _buttons = GetComponentsInChildren<Button>(true);
        foreach (var button in _buttons)
        {
            if (excludedButtons != null && System.Array.IndexOf(excludedButtons, button) >= 0) continue;
            // 内側に別の ButtonClickSE があるボタン（設定画面のプレハブなど）はそちらに任せ、二重に鳴らさない
            if (button.GetComponentInParent<ButtonClickSE>(true) != this) continue;
            button.onClick.AddListener(PlayClick);
        }
    }

    private void OnDestroy()
    {
        if (_buttons == null) return;

        foreach (var button in _buttons)
        {
            if (button != null) button.onClick.RemoveListener(PlayClick);
        }
    }

    private static void PlayClick()
    {
        if (SEPlayer.Instance != null) SEPlayer.Instance.PlayClick();
    }
}
