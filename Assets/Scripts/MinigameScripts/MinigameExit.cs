using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameExit : MonoBehaviour
{
    // Button/Win/Lose ruft das auf
    public void FinishMinigame()
    {
        var gm = FindObjectOfType<GameManager>();
        if (gm == null)
        {
            Debug.LogError("[MinigameExit] No GameManager found.");
            return;
        }

        gm.ReturnFromMinigame();
    }
}
