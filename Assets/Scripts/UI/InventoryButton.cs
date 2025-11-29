using UnityEngine;

public class InventoryButton : MonoBehaviour
{
    public GameManager gameManager;

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();
    }

    public void OnInventoryClicked()
    {
        if (gameManager != null)
            gameManager.OpenInventoryView();
    }
}
