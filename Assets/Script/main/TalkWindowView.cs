using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TalkWindowView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI talkText;
    [SerializeField] private Button clickAreaButton;

    public event Action OnWindowClicked;

    private void OnEnable()
    {
        if (clickAreaButton != null) clickAreaButton.onClick.AddListener(HandleWindowClicked);
    }

    private void OnDisable()
    {
        if (clickAreaButton != null) clickAreaButton.onClick.RemoveListener(HandleWindowClicked);
    }

    public void SetText(string text)
    {
        if (talkText != null)
        {
            talkText.text = text;
        }
    }

    private void HandleWindowClicked() => OnWindowClicked?.Invoke();
}
