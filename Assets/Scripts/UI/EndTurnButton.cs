using UnityEngine;

public class EndTurnButton : MonoBehaviour
{
    public GameManager gameManager;

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();
    }

    public void OnEndTurnClicked()
    {
        if (gameManager != null)
            gameManager.TryEndTurn();
    }
}
