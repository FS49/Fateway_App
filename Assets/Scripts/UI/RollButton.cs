using UnityEngine;

public class RollButton : MonoBehaviour
{
    public GameManager gameManager;

    public void OnRollClicked()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        if (gameManager != null)
        {
            gameManager.TryRollForCurrentPlayer();
        }
    }
}