using UnityEngine;

public class ManualCardInputButton : MonoBehaviour
{
    public GameManager gameManager;

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();
    }

    public void OpenManualInputForCurrentPlayer()
    {
        if (gameManager == null) return;

        if (gameManager.IsInputLocked)
        {
            Debug.Log("[ManualCardInputButton] Cannot open manual card input while another blocking UI is open.");
            return;
        }

        var player = gameManager.GetCurrentPlayer();
        if (player != null)
            gameManager.StartManualCardInput(player);
    }
}
