using UnityEngine;
using TMPro;

public class PlinkoRewardManager : MonoBehaviour
{
    public GameObject[] rewardVisuals;
    public TMP_Text resultText;

    public void GiveReward(int index)
    {
        foreach (var r in rewardVisuals)
            r.SetActive(false);

        if (index >= 0 && index < rewardVisuals.Length)
            rewardVisuals[index].SetActive(true);

        if (resultText != null)
            resultText.text = "Reward erhalten: " + (index + 1);

        Debug.Log("Reward: " + index);

        
        Invoke(nameof(ExitMinigame), 2f); // 2 Sekunden Reward anzeigen lassen
    }

    private void ExitMinigame()
    {
        Time.timeScale = 1f; // wichtig!

        var gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.ReturnFromMinigame();
        }
        else
        {
            Debug.LogError("Kein GameManager gefunden! Ist er DontDestroyOnLoad?");
        }
    }
}
