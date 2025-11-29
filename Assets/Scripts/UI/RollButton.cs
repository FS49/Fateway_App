using UnityEngine;

public class RollButton : MonoBehaviour
{
    public GameManager gameManager;

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();
    }

    public void OnRollClicked()
    {
        if (gameManager != null)
            gameManager.TryRollForCurrentPlayer();
    }
}
