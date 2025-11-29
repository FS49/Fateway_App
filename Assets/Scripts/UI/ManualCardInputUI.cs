using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManualCardInputUI : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;
    public GameObject panelRoot;
    public TextMeshProUGUI promptText;
    public TMP_InputField cardIdInput;
    public Button confirmButton;
    public Button cancelButton;

    private PlayerData targetPlayer;

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmClicked);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelClicked);

        Hide();
    }

    public void ShowForPlayer(PlayerData player)
    {
        targetPlayer = player;

        if (panelRoot != null)
            panelRoot.SetActive(true);

        if (promptText != null)
            promptText.text = $"Enter card ID for {player.playerName}:";

        if (cardIdInput != null)
        {
            cardIdInput.text = string.Empty;
            cardIdInput.ActivateInputField();
        }
    }

    private void Hide()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        targetPlayer = null;

        if (gameManager != null)
            gameManager.OnManualCardInputClosed();
    }

    private void OnConfirmClicked()
    {
        if (targetPlayer == null || cardIdInput == null)
        {
            Hide();
            return;
        }

        string rawId = cardIdInput.text;
        if (string.IsNullOrWhiteSpace(rawId))
            return;

        if (gameManager != null)
            gameManager.ApplyCardFromId(rawId.Trim(), targetPlayer, null);

        Hide();
    }

    private void OnCancelClicked()
    {
        Hide();
    }
}
