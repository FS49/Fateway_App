using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerSquareUI : MonoBehaviour
{
    [Header("UI References")]
    public Image backgroundImage;
    public Image activeHighlightImage;
    
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI fieldText;
    public TextMeshProUGUI starsText;

    [Header("Passion Bars")]
    public Image yellowBar;
    public Image greenBar;
    public Image blueBar;
    public Image purpleBar;
    public Image pinkBar;
    public Image orangeBar;

    public TextMeshProUGUI yellowScoreText;
    public TextMeshProUGUI greenScoreText;
    public TextMeshProUGUI blueScoreText;
    public TextMeshProUGUI purpleScoreText;
    public TextMeshProUGUI pinkScoreText;
    public TextMeshProUGUI orangeScoreText;

    private PlayerData boundPlayer;

    public void Init(PlayerData playerData, int playerIndex, bool isActive)
    {
        boundPlayer = playerData;

        if (nameText != null)
            nameText.text = playerData.playerName;

        if (backgroundImage != null)
            backgroundImage.color = PassionColorUtils.GetColor(playerData.passion);
        

        UpdateVisuals(playerData, isActive);

        gameObject.name = $"PlayerSquare_{playerIndex}_{playerData.playerName}";
    }

    public void UpdateVisuals(PlayerData playerData, bool isActive)
    {
        if (playerData == null) return;

        if (fieldText != null)
            fieldText.text = $"Field: {playerData.boardPosition}";

        if (starsText != null)
            starsText.text = $"★ {playerData.starCount}";

        if (activeHighlightImage != null)
            activeHighlightImage.enabled = isActive;

        // Bars: value is score % 100
        SetBar(yellowBar, yellowScoreText, playerData.passionScores.yellow, "Yellow: ");
        SetBar(greenBar,  greenScoreText,  playerData.passionScores.green,  "Green: ");
        SetBar(blueBar,   blueScoreText,   playerData.passionScores.blue,   "Blue: ");
        SetBar(purpleBar, purpleScoreText, playerData.passionScores.purple, "Purple: ");
        SetBar(pinkBar,   pinkScoreText,   playerData.passionScores.pink,   "Pink: ");
        SetBar(orangeBar, orangeScoreText, playerData.passionScores.orange, "Orange: ");

    }

    private void SetBar(Image barImage, TextMeshProUGUI scoreText, int score, string labelPrefix)
    {
        if (barImage != null)
        {
            float fill = Mathf.Clamp01((score % 100) / 100f);
            barImage.fillAmount = fill;
        }

        if (scoreText != null)
        {
            scoreText.text = $"{labelPrefix}{score}";
        }
    }
}
