using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PartnerPanelUI : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;
    public GameObject panelRoot;

    [Header("Player List")]
    public RectTransform playersContainer;
    public GameObject playerRowPrefab;

    [Header("Selection Display")]
    public TextMeshProUGUI selectionText;

    [Header("Buttons")]
    public Button closeButton;
    public Button resetAllButton;

    [Header("Confirmation Popup")]
    public PartnerConfirmationPopup confirmationPopup;

    private readonly List<PartnerPlayerRow> spawnedRows = new List<PartnerPlayerRow>();
    private readonly List<PlayerData> selectedPlayers = new List<PlayerData>();

    private void Start()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);

        if (resetAllButton != null)
            resetAllButton.onClick.AddListener(OnResetAllClicked);

        HideImmediate();
    }

    public void Show()
    {
        if (panelRoot != null)
            panelRoot.SetActive(true);

        ClearSelection();
        Rebuild();

        if (gameManager != null)
            gameManager.OnPartnerPanelOpened();
    }

    public void Hide()
    {
        if (confirmationPopup != null && confirmationPopup.IsVisible)
            confirmationPopup.Hide();

        ClearRows();

        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (gameManager != null)
            gameManager.OnPartnerPanelClosed();
    }

    private void HideImmediate()
    {
        ClearRows();
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void ClearRows()
    {
        for (int i = 0; i < spawnedRows.Count; i++)
        {
            if (spawnedRows[i] != null)
                Destroy(spawnedRows[i].gameObject);
        }
        spawnedRows.Clear();
    }

    private void ClearSelection()
    {
        selectedPlayers.Clear();
        UpdateSelectionText();
    }

    private void Rebuild()
    {
        ClearRows();

        if (gameManager == null || playersContainer == null || playerRowPrefab == null)
        {
            Debug.LogWarning("[PartnerPanelUI] Missing references.");
            return;
        }

        var players = gameManager.players;
        if (players == null) return;

        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            if (player == null) continue;

            GameObject rowGO = Instantiate(playerRowPrefab, playersContainer);
            var row = rowGO.GetComponent<PartnerPlayerRow>();

            if (row == null)
            {
                Destroy(rowGO);
                continue;
            }

            row.Initialize(player, i, OnPlayerSelectionChanged);
            spawnedRows.Add(row);
        }

        UpdateSelectionText();
    }

    private void OnPlayerSelectionChanged(PartnerPlayerRow row, bool isSelected)
    {
        if (isSelected)
        {
            if (!selectedPlayers.Contains(row.Player))
            {
                if (selectedPlayers.Count >= 2)
                {
                    PlayerData oldestSelected = selectedPlayers[0];
                    selectedPlayers.RemoveAt(0);

                    foreach (var r in spawnedRows)
                    {
                        if (r.Player == oldestSelected)
                        {
                            r.SetSelected(false);
                            break;
                        }
                    }
                }

                selectedPlayers.Add(row.Player);
            }
        }
        else
        {
            selectedPlayers.Remove(row.Player);
        }

        UpdateSelectionText();

        if (selectedPlayers.Count == 2)
        {
            ShowConfirmationPopup();
        }
    }

    private void UpdateSelectionText()
    {
        if (selectionText == null) return;

        if (selectedPlayers.Count == 0)
        {
            selectionText.text = "No players selected.";
        }
        else if (selectedPlayers.Count == 1)
        {
            selectionText.text = $"{selectedPlayers[0].playerName} + ???";
        }
        else if (selectedPlayers.Count >= 2)
        {
            selectionText.text = $"{selectedPlayers[0].playerName} + {selectedPlayers[1].playerName}";
        }
    }

    private void ShowConfirmationPopup()
    {
        if (confirmationPopup == null || selectedPlayers.Count < 2)
            return;

        confirmationPopup.Show(
            selectedPlayers[0],
            selectedPlayers[1],
            OnConfirmPartnership,
            OnCancelPartnership
        );
    }

    private void OnConfirmPartnership()
    {
        if (gameManager == null || selectedPlayers.Count < 2)
            return;

        gameManager.SetPartners(selectedPlayers[0], selectedPlayers[1]);

        ClearSelectionUI();
        UpdateSelectionText();
    }

    private void OnCancelPartnership()
    {
    }

    private void ClearSelectionUI()
    {
        selectedPlayers.Clear();

        foreach (var row in spawnedRows)
        {
            row.SetSelected(false);
        }

        UpdateSelectionText();
    }

    private void OnResetAllClicked()
    {
        if (gameManager != null)
            gameManager.ClearAllRelationships();

        ClearSelectionUI();
    }

    public void OnCloseButtonClicked()
    {
        Hide();
    }
}

