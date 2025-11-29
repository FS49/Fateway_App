using UnityEngine;
using TMPro;

public class GameHUDUI : MonoBehaviour
{
    public GameManager gameManager;

    [Header("UI References")]
    public TextMeshProUGUI availableRollsText;
    public TextMeshProUGUI lastRollText;

    private void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (gameManager == null || gameManager.players == null || gameManager.players.Count == 0)
            return;

        var player = gameManager.GetCurrentPlayer();
        if (player == null) return;

        // Available rolls
        if (availableRollsText != null)
        {
            availableRollsText.text = $"Available rolls: {player.availableRolls}";
        }

        // Last roll
        if (lastRollText != null)
        {
            if (gameManager.lastBaseRoll == 0 && gameManager.lastFinalRoll == 0 && gameManager.lastRollBonus == 0)
            {
                lastRollText.text = "Last roll: -";
            }
            else
            {
                lastRollText.text =
                    $"Last roll: {gameManager.lastFinalRoll} " +
                    $"(base {gameManager.lastBaseRoll} + bonus {gameManager.lastRollBonus})";
            }
        }
    }
}