using UnityEngine;
using TMPro;

public class PlinkoRewardManager : MonoBehaviour
{
    public GameObject[] rewardVisuals; // 6 verschiedene UI-Objekte
    public TMP_Text resultText;        // optional UI-Text

    public void GiveReward(int index)
    {
        // alles ausblenden
        foreach (var r in rewardVisuals)
            r.SetActive(false);

        // Reward visualisieren
        if (index >= 0 && index < rewardVisuals.Length)
            rewardVisuals[index].SetActive(true);

        if (resultText != null)
            resultText.text = "Reward erhalten: " + (index + 1);

        Debug.Log("Reward: " + index);
    }
}
