using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class FirstCrossroadsPopupUI : MonoBehaviour
{
    [Header("References")]
    public GameObject panelRoot;
    public Image displayImage;
    public TextMeshProUGUI outcomeText;
    public Button closeButton;
    public CanvasGroup imageCanvasGroup;
    public CanvasGroup textCanvasGroup;

    [Header("Registries")]
    public FirstCrossroadsImageRegistry imageRegistry;
    public FirstCrossroadsTextRegistry textRegistry;

    [Header("Fade Settings")]
    public float imageFadeDuration = 1f;
    public float delayBeforeText = 0.5f;
    public float textFadeDuration = 0.5f;

    private Action onClosedCallback;
    private bool isFadingImage;
    private bool isFadingText;
    private bool waitingForTextDelay;
    private float fadeTimer;
    private float delayTimer;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseClicked);

        Hide();
    }

    private void Update()
    {
        if (isFadingImage)
        {
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Clamp01(fadeTimer / imageFadeDuration);

            if (imageCanvasGroup != null)
                imageCanvasGroup.alpha = alpha;

            if (fadeTimer >= imageFadeDuration)
            {
                isFadingImage = false;
                waitingForTextDelay = true;
                delayTimer = 0f;
            }
        }

        if (waitingForTextDelay)
        {
            delayTimer += Time.deltaTime;
            if (delayTimer >= delayBeforeText)
            {
                waitingForTextDelay = false;
                isFadingText = true;
                fadeTimer = 0f;
            }
        }

        if (isFadingText)
        {
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Clamp01(fadeTimer / textFadeDuration);

            if (textCanvasGroup != null)
                textCanvasGroup.alpha = alpha;

            if (fadeTimer >= textFadeDuration)
            {
                isFadingText = false;
                ShowCloseButton();
            }
        }
    }

    public void Show(PlayerData player, bool isRisk, Action onClosed)
    {
        Debug.Log($"[FirstCrossroadsPopupUI] Show called. Player: {player.playerName}, IsRisk: {isRisk}");

        onClosedCallback = onClosed;
        isFadingImage = false;
        isFadingText = false;
        waitingForTextDelay = false;
        fadeTimer = 0f;
        delayTimer = 0f;

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
            Debug.Log("[FirstCrossroadsPopupUI] Panel activated.");
        }

        if (imageCanvasGroup != null)
            imageCanvasGroup.alpha = 0f;

        if (textCanvasGroup != null)
            textCanvasGroup.alpha = 0f;

        if (closeButton != null)
            closeButton.gameObject.SetActive(false);

        if (imageRegistry == null)
            imageRegistry = Resources.Load<FirstCrossroadsImageRegistry>("FirstCrossroadsImageRegistry");

        if (textRegistry == null)
            textRegistry = Resources.Load<FirstCrossroadsTextRegistry>("FirstCrossroadsTextRegistry");

        Sprite sprite = null;
        if (imageRegistry != null)
        {
            sprite = imageRegistry.GetImage(player.playerName, player.passion, player.gender, isRisk);
            Debug.Log($"[FirstCrossroadsPopupUI] Image: {(sprite != null ? sprite.name : "NULL")}");
        }
        else
        {
            Debug.LogWarning("[FirstCrossroadsPopupUI] Image registry not assigned and not found in Resources.");
        }

        string text = "";
        if (textRegistry != null)
        {
            text = textRegistry.GetText(player.playerName, player.passion, player.gender, isRisk);
            Debug.Log($"[FirstCrossroadsPopupUI] Text: {text}");
        }
        else
        {
            Debug.LogWarning("[FirstCrossroadsPopupUI] Text registry not assigned and not found in Resources.");
        }

        if (displayImage != null && sprite != null)
        {
            displayImage.sprite = sprite;
            displayImage.gameObject.SetActive(true);
        }

        if (outcomeText != null)
            outcomeText.text = text;

        if (sprite != null)
        {
            isFadingImage = true;
            fadeTimer = 0f;
            Debug.Log("[FirstCrossroadsPopupUI] Starting image fade in.");
        }
        else
        {
            Debug.Log("[FirstCrossroadsPopupUI] No image, showing text immediately.");
            if (imageCanvasGroup != null)
                imageCanvasGroup.alpha = 1f;
            if (textCanvasGroup != null)
                textCanvasGroup.alpha = 1f;
            ShowCloseButton();
        }
    }

    private void ShowCloseButton()
    {
        Debug.Log("[FirstCrossroadsPopupUI] Showing close button.");

        if (closeButton != null)
            closeButton.gameObject.SetActive(true);
    }

    private void OnCloseClicked()
    {
        Debug.Log("[FirstCrossroadsPopupUI] Close clicked.");
        Hide();
        onClosedCallback?.Invoke();
    }

    public void Hide()
    {
        isFadingImage = false;
        isFadingText = false;
        waitingForTextDelay = false;

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }
}
