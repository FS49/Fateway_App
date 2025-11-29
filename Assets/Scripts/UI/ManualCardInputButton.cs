using UnityEngine;

public class ManualCardInputButton : MonoBehaviour
{
    [Tooltip("Reference to the GameManager in the scene.")]
    public GameManager gameManager;

    /// <summary>
    /// Called from the UI Button OnClick.
    /// Opens the manual card input for the CURRENT player,
    /// if no blocking UI is open.
    /// </summary>
    public void OpenManualInputForCurrentPlayer()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogError("[ManualCardInputButton] No GameManager found in scene.");
                return;
            }
        }

        // Optional: Respektiere den bestehenden Input-Lock
        if (gameManager.IsInputLocked)
        {
            Debug.Log("[ManualCardInputButton] Cannot open manual card input while another blocking UI is open.");
            return;
        }

        var player = gameManager.GetCurrentPlayer();
        if (player == null)
        {
            Debug.LogWarning("[ManualCardInputButton] No current player to open manual input for.");
            return;
        }

        gameManager.StartManualCardInput(player);
    }
}