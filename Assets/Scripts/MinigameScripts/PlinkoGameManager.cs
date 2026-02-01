using UnityEngine;

public class PlinkoGameManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject startPanel;

    [Header("Gameplay")]
    public PlinkoBallSpawner ballSpawner;

    void Start()
    {
        // Spiel blockieren
        Time.timeScale = 0f;

        if (startPanel) startPanel.SetActive(true);

        // Spawner sicherheitshalber sperren
        if (ballSpawner)
            ballSpawner.enabled = false;
    }

    // Wird vom START-Button aufgerufen
    public void StartGame()
    {
        if (startPanel) startPanel.SetActive(false);

        Time.timeScale = 1f;

        if (ballSpawner)
            ballSpawner.enabled = true;
    }

    // Optional: neue Runde
    public void RestartRound()
    {
        if (ballSpawner)
            ballSpawner.AllowNextSpawn();
    }
}
