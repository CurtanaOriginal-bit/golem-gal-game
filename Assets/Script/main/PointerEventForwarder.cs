using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// マウスホバーイベント（IPointerEnterHandler / IPointerExitHandler）を検知して通知するユーティリティクラス
/// </summary>
public class PointerEventForwarder : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// カーソルが乗ったときに呼ばれる処理
    /// </summary>
    public Action<int> OnEnter { get; set; }

    /// <summary>
    /// カーソルが外れたときに呼ばれる処理
    /// </summary>
    public Action<int> OnExit { get; set; }

    /// <summary>
    /// 通知に使う番号
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// カーソルが乗ったことを番号付きで通知します。
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        OnEnter?.Invoke(Index);
    }

    /// <summary>
    /// カーソルが外れたことを番号付きで通知します。
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        OnExit?.Invoke(Index);
    }
}
