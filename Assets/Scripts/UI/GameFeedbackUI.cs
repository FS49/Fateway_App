using UnityEngine;
using TMPro;

public class GameFeedbackUI : MonoBehaviour
{
    public static GameFeedbackUI Instance { get; private set; }

    [Header("References")]
    public GameObject feedbackContainer;
    public TextMeshProUGUI currentFeedbackText;
    public TextMeshProUGUI previousFeedbackText;
    public CanvasGroup canvasGroup;

    [Header("Settings")]
    public float displayDuration = 10f;
    public float fadeDuration = 2f;

    private float displayTimer;
    private float fadeTimer;
    private bool isShowing;
    private bool isFading;

    private void Awake()
    {
        Instance = this;

        if (canvasGroup == null && feedbackContainer != null)
            canvasGroup = feedbackContainer.GetComponent<CanvasGroup>();

        Hide();
    }

    private void Update()
    {
        if (isShowing && !isFading)
        {
            displayTimer -= Time.deltaTime;
            if (displayTimer <= 0f)
            {
                StartFade();
            }
        }
        else if (isFading)
        {
            fadeTimer -= Time.deltaTime;
            float alpha = Mathf.Clamp01(fadeTimer / fadeDuration);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
            }
            else
            {
                SetTextAlpha(currentFeedbackText, alpha);
                SetTextAlpha(previousFeedbackText, alpha);
            }

            if (fadeTimer <= 0f)
            {
                Hide();
            }
        }
    }

    private void SetTextAlpha(TextMeshProUGUI text, float alpha)
    {
        if (text == null) return;
        Color c = text.color;
        c.a = alpha;
        text.color = c;
    }

    private string ColorizePlayerName(string playerName, PassionColor passion)
    {
        Color color = PassionColorUtils.GetColor(passion);
        string hex = ColorUtility.ToHtmlStringRGB(color);
        return $"<color=#{hex}>{playerName}</color>";
    }

    public void ShowEventCardFeedback(string playerName, PassionColor passion, string eventCardTitle)
    {
        string coloredName = ColorizePlayerName(playerName, passion);
        ShowMessage($"{coloredName} hat {eventCardTitle} ausgelöst!");
    }

    public void ShowItemCardFeedback(string playerName, PassionColor passion, string itemCardTitle)
    {
        string coloredName = ColorizePlayerName(playerName, passion);
        ShowMessage($"{coloredName} hat {itemCardTitle} bekommen!");
    }

    public void ShowPointsFeedback(string playerName, PassionColor passion, int points)
    {
        string coloredName = ColorizePlayerName(playerName, passion);
        string prefix = points >= 0 ? "+" : "-";
        ShowMessage($"{prefix}Punkte für {coloredName}");
    }

    public void ShowFieldFeedback(string playerName, PassionColor passion, string fieldDescription)
    {
        if (string.IsNullOrEmpty(fieldDescription))
            return;

        string coloredName = ColorizePlayerName(playerName, passion);
        ShowMessage($"{coloredName} erreicht {fieldDescription}");
    }

    public void ShowTurnEndedFeedback(string endingPlayerName, PassionColor endingPassion, string nextPlayerName, PassionColor nextPassion)
    {
        string coloredEnding = ColorizePlayerName(endingPlayerName, endingPassion);
        string coloredNext = ColorizePlayerName(nextPlayerName, nextPassion);
        ShowMessage($"{coloredEnding} hat den Zug beendet! {coloredNext} ist jetzt dran!");
    }

    public void ShowTurnSkippedFeedback(string skippedPlayerName, PassionColor skippedPassion, string nextPlayerName, PassionColor nextPassion)
    {
        string coloredSkipped = ColorizePlayerName(skippedPlayerName, skippedPassion);
        string coloredNext = ColorizePlayerName(nextPlayerName, nextPassion);
        ShowMessage($"{coloredSkipped} musste die Runde aussetzen, {coloredNext} ist jetzt dran!");
    }

    public void ShowMessage(string message)
    {
        isFading = false;

        if (feedbackContainer != null)
            feedbackContainer.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
        else
        {
            SetTextAlpha(currentFeedbackText, 1f);
            SetTextAlpha(previousFeedbackText, 1f);
        }

        if (previousFeedbackText != null && currentFeedbackText != null)
        {
            previousFeedbackText.text = currentFeedbackText.text;
        }

        if (currentFeedbackText != null)
        {
            currentFeedbackText.text = message;
        }

        displayTimer = displayDuration;
        isShowing = true;
    }

    private void StartFade()
    {
        isFading = true;
        fadeTimer = fadeDuration;
    }

    public void Hide()
    {
        if (feedbackContainer != null)
            feedbackContainer.SetActive(false);

        if (currentFeedbackText != null)
            currentFeedbackText.text = string.Empty;

        if (previousFeedbackText != null)
            previousFeedbackText.text = string.Empty;

        isShowing = false;
        isFading = false;
    }
}
