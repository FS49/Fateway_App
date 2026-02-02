using UnityEngine;

public class RubbellosAutoExit : MonoBehaviour
{
    [SerializeField] private float duration = 15f;

    private void Start()
    {
        Invoke(nameof(ExitMinigame), duration);
    }

    private void ExitMinigame()
    {
        Time.timeScale = 1f;

        var gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.ReturnFromMinigame();
        }
        else
        {
            Debug.LogError("[RubbellosAutoExit] Kein GameManager gefunden!");
        }
    }
}
