using System.Collections.Generic;
using UnityEngine;

public class PlayersUIController : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;
    public RectTransform playersContainer;
    public GameObject playerSquarePrefab;

    private readonly List<PlayerSquareUI> spawnedSquares = new List<PlayerSquareUI>();
    private int lastPlayerCount = -1;

    private void Start()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        RefreshUI();
    }

    private void Update()
    {
        if (gameManager?.players == null) return;

        int currentCount = gameManager.players.Count;
        if (currentCount != lastPlayerCount)
        {
            RefreshUI();
            return;
        }

        int currentIdx = gameManager.currentPlayerIndex;
        for (int i = 0; i < spawnedSquares.Count && i < gameManager.players.Count; i++)
        {
            var square = spawnedSquares[i];
            var player = gameManager.players[i];

            if (square != null && player != null)
                square.UpdateVisuals(player, i == currentIdx);
        }
    }

    public void RefreshUI()
    {
        if (gameManager == null || playersContainer == null || playerSquarePrefab == null)
            return;

        for (int i = 0; i < spawnedSquares.Count; i++)
        {
            if (spawnedSquares[i] != null)
                Destroy(spawnedSquares[i].gameObject);
        }
        spawnedSquares.Clear();

        var players = gameManager.players;
        if (players == null) return;

        int currentIdx = gameManager.currentPlayerIndex;
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            if (player == null) continue;

            GameObject squareGO = Instantiate(playerSquarePrefab, playersContainer);
            var ui = squareGO.GetComponent<PlayerSquareUI>();
            if (ui != null)
            {
                ui.Init(player, i, i == currentIdx);
                spawnedSquares.Add(ui);
            }
        }

        lastPlayerCount = players.Count;
    }
}
