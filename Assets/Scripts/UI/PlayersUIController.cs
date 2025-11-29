using System.Collections.Generic;
using UnityEngine;

public class PlayersUIController : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;
    public RectTransform playersContainer;   // Panel under Canvas
    public GameObject playerSquarePrefab;    // The prefab

    private readonly List<PlayerSquareUI> spawnedSquares = new List<PlayerSquareUI>();
    private int lastPlayerCount = -1;

    private void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        RefreshUI();
    }

    private void Update()
    {
        if (gameManager == null || gameManager.players == null)
            return;

        int currentCount = gameManager.players.Count;
        if (currentCount != lastPlayerCount)
        {
            RefreshUI();
            return;
        }

        // Update visuals every frame (or you can call this manually after each turn)
        for (int i = 0; i < spawnedSquares.Count && i < gameManager.players.Count; i++)
        {
            var square = spawnedSquares[i];
            var player = gameManager.players[i];
            bool isActive = (i == gameManager.currentPlayerIndex);

            if (square != null && player != null)
            {
                square.UpdateVisuals(player, isActive);
            }
        }
    }

    public void RefreshUI()
    {
        if (gameManager == null || playersContainer == null || playerSquarePrefab == null)
        {
            Debug.LogWarning("[PlayersUIController] Missing references.");
            return;
        }

        foreach (var sq in spawnedSquares)
        {
            if (sq != null)
                Destroy(sq.gameObject);
        }
        spawnedSquares.Clear();

        var players = gameManager.players;
        if (players == null) return;

        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            if (player == null) continue;

            GameObject squareGO = Instantiate(playerSquarePrefab, playersContainer);
            var ui = squareGO.GetComponent<PlayerSquareUI>();
            if (ui != null)
            {
                bool isActive = (i == gameManager.currentPlayerIndex);
                ui.Init(player, i, isActive);
                spawnedSquares.Add(ui);
            }
        }

        lastPlayerCount = players.Count;
    }
}
