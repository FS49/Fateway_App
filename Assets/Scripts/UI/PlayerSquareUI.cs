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

    private int cachedPosition = -1;
    private int cachedStars = -1;
    private bool cachedActive;
    private int cachedYellow = -1, cachedGreen = -1, cachedBlue = -1;
    private int cachedPurple = -1, cachedPink = -1, cachedOrange = -1;

    public void Init(PlayerData playerData, int playerIndex, bool isActive)
    {
        if (nameText != null)
            nameText.text = playerData.playerName;

        if (backgroundImage != null)
            backgroundImage.color = PassionColorUtils.GetColor(playerData.passion);

        cachedPosition = -1;
        cachedStars = -1;
        cachedYellow = cachedGreen = cachedBlue = cachedPurple = cachedPink = cachedOrange = -1;

        UpdateVisuals(playerData, isActive);

        gameObject.name = $"PlayerSquare_{playerIndex}_{playerData.playerName}";
    }

    public void UpdateVisuals(PlayerData playerData, bool isActive)
    {
        if (playerData == null) return;

        if (fieldText != null && cachedPosition != playerData.boardPosition)
        {
            cachedPosition = playerData.boardPosition;
            fieldText.text = $"Field: {cachedPosition}";
        }

        if (starsText != null && cachedStars != playerData.starCount)
        {
            cachedStars = playerData.starCount;
            starsText.text = $"★ {cachedStars}";
        }

        if (activeHighlightImage != null && cachedActive != isActive)
        {
            cachedActive = isActive;
            activeHighlightImage.enabled = isActive;
        }

        var scores = playerData.passionScores;
        SetBarCached(yellowBar, yellowScoreText, scores.yellow, ref cachedYellow, "Yellow: ");
        SetBarCached(greenBar, greenScoreText, scores.green, ref cachedGreen, "Green: ");
        SetBarCached(blueBar, blueScoreText, scores.blue, ref cachedBlue, "Blue: ");
        SetBarCached(purpleBar, purpleScoreText, scores.purple, ref cachedPurple, "Purple: ");
        SetBarCached(pinkBar, pinkScoreText, scores.pink, ref cachedPink, "Pink: ");
        SetBarCached(orangeBar, orangeScoreText, scores.orange, ref cachedOrange, "Orange: ");
    }

    private void SetBarCached(Image barImage, TextMeshProUGUI scoreText, int score, ref int cached, string labelPrefix)
    {
        if (cached == score) return;
        cached = score;

        if (barImage != null)
            barImage.fillAmount = (score % 100) * 0.01f;

        if (scoreText != null)
            scoreText.text = $"{labelPrefix}{score}";
    }
}
