using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PartnerConfirmationPopup : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panelRoot;
    public TextMeshProUGUI confirmationText;
    public CombinedHeartDisplay heartDisplay;
    public Button okButton;
    public Button cancelButton;

    [Header("Registry")]
    public HeartSpriteRegistry heartRegistry;

    private PlayerData playerA;
    private PlayerData playerB;
    private Action onConfirm;
    private Action onCancel;

    private void Awake()
    {
        if (okButton != null)
            okButton.onClick.AddListener(OnOkClicked);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelClicked);

        Hide();
    }

    public void Show(PlayerData firstPlayer, PlayerData secondPlayer, Action confirmCallback, Action cancelCallback = null)
    {
        playerA = firstPlayer;
        playerB = secondPlayer;
        onConfirm = confirmCallback;
        onCancel = cancelCallback;

        if (confirmationText != null)
        {
            string nameA = playerA?.playerName ?? "???";
            string nameB = playerB?.playerName ?? "???";
            confirmationText.text = $"{nameA} + {nameB} =";
        }

        if (heartDisplay != null)
        {
            if (heartRegistry != null)
                heartDisplay.Initialize(playerA, playerB, heartRegistry);
            else
                heartDisplay.Initialize(playerA, playerB);
        }

        if (panelRoot != null)
            panelRoot.SetActive(true);
    }

    public void Hide()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (heartDisplay != null)
            heartDisplay.Clear();

        playerA = null;
        playerB = null;
        onConfirm = null;
        onCancel = null;
    }

    private void OnOkClicked()
    {
        var callback = onConfirm;
        Hide();
        callback?.Invoke();
    }

    private void OnCancelClicked()
    {
        var callback = onCancel;
        Hide();
        callback?.Invoke();
    }

    public bool IsVisible => panelRoot != null && panelRoot.activeSelf;
}

