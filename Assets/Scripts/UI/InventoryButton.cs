using UnityEngine;

public class InventoryButton : MonoBehaviour
{
    public GameManager gameManager;

    public void OnInventoryClicked()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        if (gameManager != null)
        {
            gameManager.OpenInventoryView();
        }
    }
}