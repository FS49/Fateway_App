using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System;

public class LastCrossroadsPopupUI : MonoBehaviour
{
    [Header("References")]
    public GameObject panelRoot;
    public VideoPlayer videoPlayer;
    public RawImage videoDisplay;
    public TextMeshProUGUI outcomeText;
    public Button closeButton;
    public CanvasGroup textCanvasGroup;

    [Header("Registries")]
    public LastCrossroadsAnimationRegistry animationRegistry;
    public LastCrossroadsTextRegistry textRegistry;

    [Header("Settings")]
    public float videoTimeout = 5f;

    private Action onClosedCallback;
    private RenderTexture renderTexture;
    private VideoClip pendingClip;
    private bool needsToStartVideo;
    private bool waitingForVideo;
    private float videoWaitTimer;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseClicked);

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.prepareCompleted += OnVideoPrepared;
            videoPlayer.errorReceived += OnVideoError;
        }

        Hide();
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.errorReceived -= OnVideoError;
        }

        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }

    private void Update()
    {
        if (needsToStartVideo && pendingClip != null)
        {
            needsToStartVideo = false;
            ActuallyPrepareVideo();
        }

        if (waitingForVideo)
        {
            videoWaitTimer -= Time.deltaTime;
            if (videoWaitTimer <= 0f)
            {
                Debug.LogWarning("[LastCrossroadsPopupUI] Video timeout, showing text immediately.");
                waitingForVideo = false;
                ShowTextAndCloseButton();
            }
        }
    }

    public void Show(PlayerData player, bool isSuccess, Action onClosed)
    {
        Debug.Log($"[LastCrossroadsPopupUI] Show called. Player: {player.playerName}, Success: {isSuccess}");

        onClosedCallback = onClosed;
        needsToStartVideo = false;
        waitingForVideo = false;
        pendingClip = null;

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
            Debug.Log("[LastCrossroadsPopupUI] Panel activated.");
        }

        if (videoPlayer != null)
            videoPlayer.gameObject.SetActive(true);

        if (textCanvasGroup != null)
            textCanvasGroup.alpha = 0f;

        if (closeButton != null)
            closeButton.gameObject.SetActive(false);

        VideoClip clip = null;
        if (animationRegistry != null)
        {
            clip = animationRegistry.GetClip(player.playerName, player.passion, player.gender, isSuccess);
            Debug.Log($"[LastCrossroadsPopupUI] Animation clip: {(clip != null ? clip.name : "NULL")}");
        }
        else
        {
            Debug.LogWarning("[LastCrossroadsPopupUI] Animation registry is null!");
        }

        string text = "";
        if (textRegistry != null)
        {
            text = textRegistry.GetText(player.playerName, player.passion, player.gender, isSuccess);
            Debug.Log($"[LastCrossroadsPopupUI] Text: {text}");
        }
        else
        {
            Debug.LogWarning("[LastCrossroadsPopupUI] Text registry is null!");
        }

        if (outcomeText != null)
            outcomeText.text = text;

        if (clip != null && videoPlayer != null)
        {
            pendingClip = clip;
            needsToStartVideo = true;
            waitingForVideo = true;
            videoWaitTimer = videoTimeout;
            Debug.Log("[LastCrossroadsPopupUI] Will start video next frame.");
        }
        else
        {
            Debug.Log("[LastCrossroadsPopupUI] No video clip, showing text immediately.");
            ShowTextAndCloseButton();
        }
    }

    private void ActuallyPrepareVideo()
    {
        Debug.Log("[LastCrossroadsPopupUI] Preparing video...");

        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(1920, 1080, 0);
            renderTexture.Create();
        }

        videoPlayer.enabled = true;
        videoPlayer.targetTexture = renderTexture;

        if (videoDisplay != null)
            videoDisplay.texture = renderTexture;

        videoPlayer.clip = pendingClip;
        videoPlayer.Prepare();
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        Debug.Log("[LastCrossroadsPopupUI] Video prepared, playing...");
        waitingForVideo = false;
        videoPlayer.Play();
    }

    private void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError($"[LastCrossroadsPopupUI] Video error: {message}");
        waitingForVideo = false;
        ShowTextAndCloseButton();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("[LastCrossroadsPopupUI] Video finished.");
        ShowTextAndCloseButton();
    }

    private void ShowTextAndCloseButton()
    {
        Debug.Log("[LastCrossroadsPopupUI] Showing text and close button.");

        if (textCanvasGroup != null)
            textCanvasGroup.alpha = 1f;

        if (closeButton != null)
            closeButton.gameObject.SetActive(true);
    }

    private void OnCloseClicked()
    {
        Debug.Log("[LastCrossroadsPopupUI] Close clicked.");
        Hide();
        onClosedCallback?.Invoke();
    }

    public void Hide()
    {
        needsToStartVideo = false;
        waitingForVideo = false;
        pendingClip = null;

        if (videoPlayer != null && videoPlayer.isPlaying)
            videoPlayer.Stop();

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }
}
