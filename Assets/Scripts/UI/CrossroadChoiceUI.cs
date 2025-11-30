using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CrossroadChoiceUI : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;
    public GameObject panelRoot;

    [Header("UI Elements")]
    public TextMeshProUGUI promptText;
    public TextMeshProUGUI remainingMovementText;
    public Button safeButton;
    public Button riskButton;

    private PlayerData currentPlayer;
    private Action<bool> onChoiceMade;

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        if (safeButton != null)
            safeButton.onClick.AddListener(OnSafeClicked);

        if (riskButton != null)
            riskButton.onClick.AddListener(OnRiskClicked);

        Hide();
    }

    public void Show(PlayerData player, int remainingMovement, Action<bool> choiceCallback)
    {
        Debug.Log($"[CrossroadChoiceUI] Show called for {player?.playerName} with remaining movement {remainingMovement}");

        currentPlayer = player;
        onChoiceMade = choiceCallback;

        if (promptText != null)
            promptText.text = $"{player.playerName}, choose your path:";

        if (remainingMovementText != null)
            remainingMovementText.text = $"Remaining movement: {remainingMovement}";

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
            Debug.Log($"[CrossroadChoiceUI] Panel activated");
        }
        else
        {
            Debug.LogError("[CrossroadChoiceUI] panelRoot is NULL! Cannot show panel.");
        }

        if (gameManager != null)
            gameManager.OnCrossroadChoiceOpened();
    }

    public void Hide()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        currentPlayer = null;

        if (gameManager != null)
            gameManager.OnCrossroadChoiceClosed();
    }

    private void OnSafeClicked()
    {
        MakeChoice(false);
    }

    private void OnRiskClicked()
    {
        MakeChoice(true);
    }

    private void MakeChoice(bool choseRisk)
    {
        var callback = onChoiceMade;
        onChoiceMade = null;

        Hide();

        callback?.Invoke(choseRisk);
    }

    public bool IsVisible => panelRoot != null && panelRoot.activeSelf;
}

