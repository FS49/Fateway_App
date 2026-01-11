using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ResultsScreenUI : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;
    public TextMeshProUGUI rankingsText;

    private void Awake()
    {
        if (rankingsText == null)
            rankingsText = GetComponent<TextMeshProUGUI>();

        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();
    }

    private void OnEnable()
    {
        RefreshRankings();
    }

    public void RefreshRankings()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        if (gameManager == null || rankingsText == null)
            return;

        List<PlayerData> rankings = gameManager.GetPlayerRankings();

        if (rankings == null || rankings.Count == 0)
        {
            rankingsText.text = "Keine Spieler gefunden.";
            return;
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("Plätze:");

        for (int i = 0; i < rankings.Count; i++)
        {
            PlayerData player = rankings[i];
            int place = i + 1;
            int score = player.GetTotalScore();

            if (place == 1)
                sb.AppendLine($"{place}. {player.playerName} mit {score} Punkten!");
            else
                sb.AppendLine($"{place}. {player.playerName} mit {score} Punkten");
        }

        rankingsText.text = sb.ToString();
    }
}
