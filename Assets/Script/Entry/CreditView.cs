using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// クレジット画面の View です。閉じるボタンの操作を通知します。
/// </summary>
public class CreditView : MonoBehaviour
{
    /// <summary>
    /// クレジット画面を閉じるボタン
    /// </summary>
    [SerializeField] private Button closeCreditButton;

    /// <summary>
    /// 閉じるボタンが押されたときに呼ばれる処理
    /// </summary>
    public Action OnCloseCreditClicked { get; set; }

    /// <summary>
    /// 有効化時に閉じるボタンのクリックイベントを購読します。
    /// </summary>
    private void OnEnable()
    {
        if (closeCreditButton != null)
        {
            closeCreditButton.onClick.AddListener(HandleCloseCreditClicked);
        }
    }

    /// <summary>
    /// 無効化時に閉じるボタンのクリックイベントの購読を解除します。
    /// </summary>
    private void OnDisable()
    {
        if (closeCreditButton != null)
        {
            closeCreditButton.onClick.RemoveListener(HandleCloseCreditClicked);
        }
    }

    /// <summary>
    /// 閉じるボタンが押されたことを通知します。
    /// </summary>
    private void HandleCloseCreditClicked() => OnCloseCreditClicked?.Invoke();
}
