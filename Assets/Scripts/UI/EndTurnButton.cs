using UnityEngine;

public class EndTurnButton : MonoBehaviour
{
    public GameManager gameManager;

    public void OnEndTurnClicked()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        if (gameManager != null)
        {
            gameManager.TryEndTurn();
        }
    }
}