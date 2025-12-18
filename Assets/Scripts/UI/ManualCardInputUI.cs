using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum ManualInputMode
{
    Receive,
    GiveToPlayer,
    TakeFromPlayer,
    ReturnToStack
}

public class ManualCardInputUI : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;
    public CardManager cardManager;
    public GameObject panelRoot;

    [Header("Tabs")]
    public Button receiveTabButton;
    public Button giveTabButton;
    public Button takeTabButton;
    public Button returnTabButton;

    [Header("Tab Highlight Colors")]
    public Color activeTabColor = new Color(0.53f, 0.81f, 0.92f, 1f);
    public Color inactiveTabColor = Color.white;

    [Header("Input Section")]
    public TextMeshProUGUI promptText;
    public TMP_InputField cardIdInput;
    public Button confirmButton;
    public Button cancelButton;

    [Header("Player Selection (for Give/Take)")]
    public GameObject playerSelectionContainer;
    public RectTransform playersListContainer;
    public GameObject playerRowPrefab;
    public TextMeshProUGUI selectedPlayerText;

    [Header("Feedback")]
    public TextMeshProUGUI feedbackText;

    [Header("Card Popup")]
    public CardDescriptionPopup cardDescriptionPopup;

    private PlayerData targetPlayer;
    private PlayerData selectedOtherPlayer;
    private ManualInputMode currentMode = ManualInputMode.Receive;
    private readonly List<ManualInputPlayerRow> spawnedRows = new List<ManualInputPlayerRow>();

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        if (cardManager == null)
            cardManager = FindObjectOfType<CardManager>();

        if (cardDescriptionPopup == null)
            cardDescriptionPopup = FindObjectOfType<CardDescriptionPopup>(true);

        if (receiveTabButton != null)
            receiveTabButton.onClick.AddListener(() => SetMode(ManualInputMode.Receive));

        if (giveTabButton != null)
            giveTabButton.onClick.AddListener(() => SetMode(ManualInputMode.GiveToPlayer));

        if (takeTabButton != null)
            takeTabButton.onClick.AddListener(() => SetMode(ManualInputMode.TakeFromPlayer));

        if (returnTabButton != null)
            returnTabButton.onClick.AddListener(() => SetMode(ManualInputMode.ReturnToStack));

        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmClicked);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelClicked);

        Hide();
    }

    public void ShowForPlayer(PlayerData player)
    {
        targetPlayer = player;

        if (panelRoot != null)
            panelRoot.SetActive(true);

        SetMode(ManualInputMode.Receive);
        ClearFeedback();

        if (cardIdInput != null)
        {
            cardIdInput.text = string.Empty;
            cardIdInput.ActivateInputField();
        }
    }

    private void SetMode(ManualInputMode mode)
    {
        currentMode = mode;
        selectedOtherPlayer = null;

        UpdateTabHighlights();
        UpdatePromptText();
        UpdatePlayerSelection();
        ClearFeedback();

        if (cardIdInput != null)
        {
            cardIdInput.text = string.Empty;
            cardIdInput.ActivateInputField();
        }
    }

    private void UpdateTabHighlights()
    {
        SetTabColor(receiveTabButton, currentMode == ManualInputMode.Receive);
        SetTabColor(giveTabButton, currentMode == ManualInputMode.GiveToPlayer);
        SetTabColor(takeTabButton, currentMode == ManualInputMode.TakeFromPlayer);
        SetTabColor(returnTabButton, currentMode == ManualInputMode.ReturnToStack);
    }

    private void SetTabColor(Button tabButton, bool isActive)
    {
        if (tabButton == null) return;

        Color targetColor = isActive ? activeTabColor : inactiveTabColor;

        var image = tabButton.GetComponent<Image>();
        if (image != null)
            image.color = targetColor;

        var colors = tabButton.colors;
        colors.normalColor = targetColor;
        colors.highlightedColor = targetColor;
        colors.pressedColor = targetColor * 0.9f;
        colors.selectedColor = targetColor;
        tabButton.colors = colors;
    }

    private void UpdatePromptText()
    {
        if (promptText == null || targetPlayer == null) return;

        switch (currentMode)
        {
            case ManualInputMode.Receive:
                promptText.text = $"Enter card ID for {targetPlayer.playerName} to receive:";
                break;
            case ManualInputMode.GiveToPlayer:
                promptText.text = $"Enter card ID from {targetPlayer.playerName}'s inventory to give:";
                break;
            case ManualInputMode.TakeFromPlayer:
                promptText.text = $"Enter card ID to take from selected player:";
                break;
            case ManualInputMode.ReturnToStack:
                promptText.text = $"Enter card ID from {targetPlayer.playerName}'s inventory to return:";
                break;
        }
    }

    private void UpdatePlayerSelection()
    {
        bool needsSelection = currentMode == ManualInputMode.GiveToPlayer || currentMode == ManualInputMode.TakeFromPlayer;

        if (playerSelectionContainer != null)
            playerSelectionContainer.SetActive(needsSelection);

        ClearPlayerRows();

        if (needsSelection)
        {
            BuildPlayerRows();
        }

        UpdateSelectedPlayerText();
    }

    private void ClearPlayerRows()
    {
        for (int i = 0; i < spawnedRows.Count; i++)
        {
            if (spawnedRows[i] != null)
                Destroy(spawnedRows[i].gameObject);
        }
        spawnedRows.Clear();
    }

    private void BuildPlayerRows()
    {
        if (gameManager == null || playersListContainer == null || playerRowPrefab == null)
            return;

        var players = gameManager.players;
        if (players == null) return;

        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            if (player == null) continue;

            if (player == targetPlayer)
                continue;

            GameObject rowGO = Instantiate(playerRowPrefab, playersListContainer);
            var row = rowGO.GetComponent<ManualInputPlayerRow>();

            if (row == null)
            {
                Destroy(rowGO);
                continue;
            }

            row.Initialize(player, i, OnPlayerRowSelectionChanged);
            spawnedRows.Add(row);
        }
    }

    private void OnPlayerRowSelectionChanged(ManualInputPlayerRow changedRow, bool isSelected)
    {
        if (isSelected)
        {
            for (int i = 0; i < spawnedRows.Count; i++)
            {
                var row = spawnedRows[i];
                if (row != changedRow && row.IsSelected)
                {
                    row.SetSelected(false);
                }
            }

            selectedOtherPlayer = changedRow.Player;
        }
        else
        {
            if (selectedOtherPlayer == changedRow.Player)
            {
                selectedOtherPlayer = null;
            }
        }

        UpdateSelectedPlayerText();
    }

    private void UpdateSelectedPlayerText()
    {
        if (selectedPlayerText == null) return;

        if (currentMode == ManualInputMode.GiveToPlayer)
        {
            selectedPlayerText.text = selectedOtherPlayer != null
                ? $"Give to: {selectedOtherPlayer.playerName}"
                : "Select a player to give the card to";
        }
        else if (currentMode == ManualInputMode.TakeFromPlayer)
        {
            selectedPlayerText.text = selectedOtherPlayer != null
                ? $"Take from: {selectedOtherPlayer.playerName}"
                : "Select a player to take a card from";
        }
        else
        {
            selectedPlayerText.text = string.Empty;
        }
    }

    private void Hide()
    {
        ClearPlayerRows();

        if (panelRoot != null)
            panelRoot.SetActive(false);

        targetPlayer = null;
        selectedOtherPlayer = null;

        if (gameManager != null)
            gameManager.OnManualCardInputClosed();
    }

    private void OnConfirmClicked()
    {
        if (targetPlayer == null || cardIdInput == null)
        {
            Hide();
            return;
        }

        string rawId = cardIdInput.text;
        if (string.IsNullOrWhiteSpace(rawId))
        {
            ShowFeedback("Please enter a card ID.", Color.yellow);
            return;
        }

        string cardId = rawId.Trim();

        switch (currentMode)
        {
            case ManualInputMode.Receive:
                ExecuteReceive(cardId);
                break;
            case ManualInputMode.GiveToPlayer:
                ExecuteGive(cardId);
                break;
            case ManualInputMode.TakeFromPlayer:
                ExecuteTake(cardId);
                break;
            case ManualInputMode.ReturnToStack:
                ExecuteReturn(cardId);
                break;
        }
    }

    private void ExecuteReceive(string cardId)
    {
        BaseCardDefinition cardDef = null;

        if (cardManager != null)
        {
            cardDef = cardManager.GetCardById(cardId);
        }

        if (gameManager != null)
        {
            gameManager.ApplyCardFromId(cardId, targetPlayer, null);
        }

        Hide();

        if (cardDef != null && cardDescriptionPopup != null)
        {
            Debug.Log($"[ManualCardInputUI] Showing popup for card: {cardDef.title}");
            cardDescriptionPopup.Show(cardDef);
        }
        else
        {
            Debug.LogWarning($"[ManualCardInputUI] Could not show popup. cardDef={cardDef != null}, popup={cardDescriptionPopup != null}");
        }
    }

    private void ExecuteGive(string cardId)
    {
        if (cardManager != null && cardManager.GetCardById(cardId) is EventCardDefinition)
        {
            ShowFeedback("Event cards can only be used in the 'Receive' tab.", Color.red);
            return;
        }

        if (selectedOtherPlayer == null)
        {
            ShowFeedback("Please select a player to give the card to.", Color.yellow);
            return;
        }

        if (!targetPlayer.inventory.Contains(cardId))
        {
            ShowFeedback($"Card '{cardId}' not found in {targetPlayer.playerName}'s inventory.", Color.red);
            return;
        }

        var cardDef = cardManager?.GetItemById(cardId);
        if (cardDef == null)
        {
            ShowFeedback($"Card '{cardId}' is not a valid item card.", Color.red);
            return;
        }

        if (cardDef.uniquePerPlayer && selectedOtherPlayer.inventory.Contains(cardId))
        {
            ShowFeedback($"{selectedOtherPlayer.playerName} already has this unique item.", Color.red);
            return;
        }

        targetPlayer.inventory.Remove(cardId);
        selectedOtherPlayer.inventory.Add(cardId);

        Debug.Log($"[ManualCardInputUI] {targetPlayer.playerName} gave '{cardId}' to {selectedOtherPlayer.playerName}.");
        ShowFeedback($"Gave '{cardDef.title}' to {selectedOtherPlayer.playerName}.", Color.green);

        Hide();
    }

    private void ExecuteTake(string cardId)
    {
        if (cardManager != null && cardManager.GetCardById(cardId) is EventCardDefinition)
        {
            ShowFeedback("Event cards can only be used in the 'Receive' tab.", Color.red);
            return;
        }

        if (selectedOtherPlayer == null)
        {
            ShowFeedback("Please select a player to take the card from.", Color.yellow);
            return;
        }

        if (!selectedOtherPlayer.inventory.Contains(cardId))
        {
            ShowFeedback($"Card '{cardId}' not found in {selectedOtherPlayer.playerName}'s inventory.", Color.red);
            return;
        }

        var cardDef = cardManager?.GetItemById(cardId);
        if (cardDef == null)
        {
            ShowFeedback($"Card '{cardId}' is not a valid item card.", Color.red);
            return;
        }

        if (cardDef.uniquePerPlayer && targetPlayer.inventory.Contains(cardId))
        {
            ShowFeedback($"{targetPlayer.playerName} already has this unique item.", Color.red);
            return;
        }

        selectedOtherPlayer.inventory.Remove(cardId);
        targetPlayer.inventory.Add(cardId);

        Debug.Log($"[ManualCardInputUI] {targetPlayer.playerName} took '{cardId}' from {selectedOtherPlayer.playerName}.");
        ShowFeedback($"Took '{cardDef.title}' from {selectedOtherPlayer.playerName}.", Color.green);

        Hide();
    }

    private void ExecuteReturn(string cardId)
    {
        if (cardManager != null && cardManager.GetCardById(cardId) is EventCardDefinition)
        {
            ShowFeedback("Event cards can only be used in the 'Receive' tab.", Color.red);
            return;
        }

        if (!targetPlayer.inventory.Contains(cardId))
        {
            ShowFeedback($"Card '{cardId}' not found in {targetPlayer.playerName}'s inventory.", Color.red);
            return;
        }

        var cardDef = cardManager?.GetItemById(cardId);
        if (cardDef == null)
        {
            ShowFeedback($"Card '{cardId}' is not a valid item card.", Color.red);
            return;
        }

        targetPlayer.inventory.Remove(cardId);

        if (gameManager != null)
            gameManager.RemoveStatusEffect(targetPlayer, cardId);

        Debug.Log($"[ManualCardInputUI] {targetPlayer.playerName} returned '{cardId}' to the stack.");
        ShowFeedback($"Returned '{cardDef.title}' to the stack.", Color.green);

        Hide();
    }

    private void ShowFeedback(string message, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;
        }
    }

    private void ClearFeedback()
    {
        if (feedbackText != null)
            feedbackText.text = string.Empty;
    }

    private void OnCancelClicked()
    {
        Hide();
    }
}
