using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;
    public CardManager cardManager;

    public GameObject panelRoot;

    [Tooltip("Container that holds one column per player (same layout/position idea as PlayerSquaresPanel).")]
    public RectTransform playersContainer;

    [Tooltip("Prefab for each player's column (name + Status + Items).")]
    public GameObject playerColumnPrefab;

    [Tooltip("Prefab for each card button (shows title, opens popup).")]
    public GameObject cardButtonPrefab;

    [Tooltip("Popup that shows card title/description/tags.")]
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
        cardPopup.Hide();
        
        ClearColumns();

        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (gameManager != null)
            gameManager.OnInventoryClosed();
    }

    // Use this when starting to avoid firing OnInventoryClosed at boot
    private void HideImmediate()
    {
        ClearColumns();
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void ClearColumns()
    {
        foreach (var go in spawnedColumns)
        {
            if (go != null)
                Destroy(go);
        }
        spawnedColumns.Clear();
    }

    private void Rebuild()
    {
        ClearColumns();

        if (gameManager == null || cardManager == null || playersContainer == null || playerColumnPrefab == null || cardButtonPrefab == null)
        {
            Debug.LogWarning("[InventoryUI] Missing references.");
            return;
        }

        var players = gameManager.players;
        if (players == null) return;

        foreach (var player in players)
        {
            if (player == null) continue;

            GameObject colGO = Instantiate(playerColumnPrefab, playersContainer);
            var col = colGO.GetComponent<InventoryPlayerColumn>();
            if (col == null)
            {
                Debug.LogError("[InventoryUI] Player column prefab missing InventoryPlayerColumn component.");
                Destroy(colGO);
                continue;
            }

            // NEW: initialize with player name + passion color
            col.Init(player);

            // STATUS EFFECTS
            if (col.statusEffectsContainer != null && player.statusEffectCards != null)
            {
                foreach (var id in player.statusEffectCards)
                {
                    var card = cardManager.GetCardById(id);
                    if (card == null) continue;

                    CreateCardButton(card, col.statusEffectsContainer);
                }
            }

            // ITEM CARDS
            if (col.itemsContainer != null && player.inventory != null)
            {
                foreach (var itemId in player.inventory)
                {
                    var item = cardManager.GetItemById(itemId);
                    if (item == null) continue;

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
            Debug.LogError("[InventoryUI] Card button prefab missing InventoryCardButton component.");
            Destroy(btnGO);
            return;
        }

        btn.Init(card, cardPopup);
    }

    // Hook to the X button on the inventory overlay
    public void OnCloseButtonClicked()
    {
        Hide();
    }
}
