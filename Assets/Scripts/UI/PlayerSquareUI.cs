using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerSquareUI : MonoBehaviour
{
    [Header("UI References")]
    public Image backgroundImage;
    public Image activeHighlightImage;
    public UIImageGlow activeHighlightGlow;

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI fieldText;
    public TextMeshProUGUI starsText;

    [Header("Player Avatar")]
    public PlayerAnimatedImage playerAvatar;

    [Header("Relationship Heart")]
    public CombinedHeartDisplay relationshipHeart;
    public GameObject relationshipHeartContainer;

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
    private GameManager gameManager;

    private int cachedPosition = -1;
    private int cachedStars = -1;
    private bool cachedActive;
    private int cachedYellow = -1, cachedGreen = -1, cachedBlue = -1;
    private int cachedPurple = -1, cachedPink = -1, cachedOrange = -1;
    private int cachedPartnerIndex = -2;

    private Color Brighten(Color c, float amount)
    {
        return new Color(
            Mathf.Clamp01(c.r * amount),
            Mathf.Clamp01(c.g * amount),
            Mathf.Clamp01(c.b * amount),
            c.a
        );
    }

    public void Init(PlayerData playerData, int playerIndex, bool isActive, GameManager manager = null)
    {
        boundPlayer = playerData;
        gameManager = manager;

        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        if (nameText != null)
            nameText.text = playerData.playerName;

        if (backgroundImage != null)
            backgroundImage.color = PassionColorUtils.GetColor(playerData.passion);

        if (playerAvatar != null)
            playerAvatar.Initialize(playerData);

        if (activeHighlightImage != null)
        {
            activeHighlightImage.enabled = isActive;
            Color baseColor = PassionColorUtils.GetColor(playerData.passion);
            activeHighlightImage.color = Brighten(baseColor, 1.20f);
        }

        if (activeHighlightGlow != null)
        {
            if (isActive)
                activeHighlightGlow.StartGlow();
            else
                activeHighlightGlow.StopGlow(true);
        }

        cachedPosition = -1;
        cachedStars = -1;
        cachedActive = isActive;
        cachedYellow = cachedGreen = cachedBlue = cachedPurple = cachedPink = cachedOrange = -1;
        cachedPartnerIndex = -2;

        UpdateVisuals(playerData, isActive);

        gameObject.name = $"PlayerSquare_{playerIndex}_{playerData.playerName}";
    }

    public void UpdateVisuals(PlayerData playerData, bool isActive)
    {
        if (playerData == null) return;

        boundPlayer = playerData;

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
            activeHighlightImage.transform.SetAsFirstSibling();

            Color baseColor = PassionColorUtils.GetColor(playerData.passion);
            activeHighlightImage.color = Brighten(baseColor, 1.20f);

            if (activeHighlightGlow != null)
            {
                if (isActive)
                    activeHighlightGlow.StartGlow();
                else
                    activeHighlightGlow.StopGlow(true);
            }
        }

        var scores = playerData.passionScores;
        SetBarCached(yellowBar, yellowScoreText, scores.yellow, ref cachedYellow, "Yellow: ");
        SetBarCached(greenBar, greenScoreText, scores.green, ref cachedGreen, "Green: ");
        SetBarCached(blueBar, blueScoreText, scores.blue, ref cachedBlue, "Blue: ");
        SetBarCached(purpleBar, purpleScoreText, scores.purple, ref cachedPurple, "Purple: ");
        SetBarCached(pinkBar, pinkScoreText, scores.pink, ref cachedPink, "Pink: ");
        SetBarCached(orangeBar, orangeScoreText, scores.orange, ref cachedOrange, "Orange: ");

        UpdateRelationshipHeart(playerData);
    }

    private void UpdateRelationshipHeart(PlayerData playerData)
    {
        if (relationshipHeart == null && relationshipHeartContainer == null)
            return;

        if (cachedPartnerIndex == playerData.partnerIndex)
            return;

        cachedPartnerIndex = playerData.partnerIndex;

        bool hasPartner = playerData.HasPartner && gameManager != null;
        PlayerData partner = hasPartner ? gameManager.GetPartner(playerData) : null;
        bool showHeart = partner != null;

        if (relationshipHeartContainer != null)
            relationshipHeartContainer.SetActive(showHeart);

        if (relationshipHeart != null)
        {
            if (showHeart)
            {
                relationshipHeart.gameObject.SetActive(true);
                relationshipHeart.Initialize(playerData, partner);
            }
            else
            {
                relationshipHeart.Clear();
                relationshipHeart.gameObject.SetActive(false);
            }
        }
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

    public PlayerData BoundPlayer => boundPlayer;
}
