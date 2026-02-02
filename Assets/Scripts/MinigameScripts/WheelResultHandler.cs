using UnityEngine;

public class WheelResultHandler : MonoBehaviour
{
    [Header("Exit Settings")]
    public float exitDelay = 2f; // Ergebnis kurz anzeigen

    public void OnWheelResult(int index)
    {
        Debug.Log("Gewonnen: " + index);

        // Hier kannst du optional Reward/Points vergeben
        // z.B. abhängig vom index

        Invoke(nameof(ExitMinigame), exitDelay);
    }

    private void ExitMinigame()
    {
        Time.timeScale = 1f;

        var gm = FindObjectOfType<GameManager>();
        if (gm != null)
            gm.ReturnFromMinigame();
        else
            Debug.LogError("[WheelResultHandler] Kein GameManager gefunden.");
    }
}