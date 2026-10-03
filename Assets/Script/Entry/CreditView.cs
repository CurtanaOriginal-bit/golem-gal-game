using System;
using UnityEngine;
using UnityEngine.UI;

public class CreditView : MonoBehaviour
{
    [SerializeField] private Button closeCreditButton;

    public Action OnCloseCreditClicked;

    private bool _isInitializing = false;

    private void OnEnable()
    {
        if (closeCreditButton != null)
        {
            closeCreditButton.onClick.AddListener(HandleCloseCreditClicked);
        }
    }

    private void OnDisable()
    {
        if (closeCreditButton != null)
        {
            closeCreditButton.onClick.RemoveListener(HandleCloseCreditClicked);
        }
    }


    private void HandleCloseCreditClicked() => OnCloseCreditClicked?.Invoke();
}
