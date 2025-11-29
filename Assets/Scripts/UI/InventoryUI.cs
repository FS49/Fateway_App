using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;
    public CardManager cardManager;
    public GameObject panelRoot;
    public RectTransform playersContainer;
    public GameObject playerColumnPrefab;
    public GameObject cardButtonPrefab;
    public CardDescriptionPopup cardPopup;

    private readonly List<GameObject> spawnedColumns = new List<GameObject>();

    private void Start()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        if (cardManager == null)
            cardManager = FindObjectOfType<CardManager>();

        HideImmediate();
    }

    public void Show()
    {
        if (panelRoot != null)
            panelRoot.SetActive(true);

        Rebuild();

        if (gameManager != null)
            gameManager.OnInventoryOpened();
    }

    public void Hide()
    {
        if (cardPopup != null)
            cardPopup.Hide();

        ClearColumns();

        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (gameManager != null)
            gameManager.OnInventoryClosed();
    }

    private void HideImmediate()
    {
        ClearColumns();
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void ClearColumns()
    {
        for (int i = 0; i < spawnedColumns.Count; i++)
        {
            if (spawnedColumns[i] != null)
                Destroy(spawnedColumns[i]);
        }
        spawnedColumns.Clear();
    }

    private void Rebuild()
    {
        ClearColumns();

        if (gameManager == null || cardManager == null || playersContainer == null || 
            playerColumnPrefab == null || cardButtonPrefab == null)
            return;

        var players = gameManager.players;
        if (players == null) return;

        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            if (player == null) continue;

            GameObject colGO = Instantiate(playerColumnPrefab, playersContainer);
            var col = colGO.GetComponent<InventoryPlayerColumn>();
            if (col == null)
            {
                Destroy(colGO);
                continue;
            }

            col.Init(player);

            if (col.statusEffectsContainer != null && player.statusEffectCards != null)
            {
                for (int j = 0; j < player.statusEffectCards.Count; j++)
                {
                    var card = cardManager.GetCardById(player.statusEffectCards[j]);
                    if (card != null)
                        CreateCardButton(card, col.statusEffectsContainer);
                }
            }

            if (col.itemsContainer != null && player.inventory != null)
            {
                for (int j = 0; j < player.inventory.Count; j++)
                {
                    var item = cardManager.GetItemById(player.inventory[j]);
                    if (item != null)
                        CreateCardButton(item, col.itemsContainer);
                }
            }

            spawnedColumns.Add(colGO);
        }
    }

    private void CreateCardButton(BaseCardDefinition card, Transform parent)
    {
        GameObject btnGO = Instantiate(cardButtonPrefab, parent);
        var btn = btnGO.GetComponent<InventoryCardButton>();
        if (btn == null)
        {
            Destroy(btnGO);
            return;
        }

        btn.Init(card, cardPopup);
    }

    public void OnCloseButtonClicked()
    {
        Hide();
    }
}
