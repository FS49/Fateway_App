using UnityEngine;
using TMPro;

public class GameHUDUI : MonoBehaviour
{
    public GameManager gameManager;

    [Header("UI References")]
    public TextMeshProUGUI availableRollsText;
    public TextMeshProUGUI lastRollText;

    private int cachedRolls = -1;
    private int cachedBaseRoll = -1;
    private int cachedBonus = -1;
    private int cachedFinal = -1;

    private void Start()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        Refresh(true);
    }

    private void Update()
    {
        Refresh(false);
    }

    private void Refresh(bool force)
    {
        if (gameManager?.players == null || gameManager.players.Count == 0)
            return;

        var player = gameManager.GetCurrentPlayer();
        if (player == null) return;

        if (availableRollsText != null && (force || cachedRolls != player.availableRolls))
        {
            cachedRolls = player.availableRolls;
            availableRollsText.text = $"Available rolls: {cachedRolls}";
        }

        if (lastRollText != null)
        {
            int baseRoll = gameManager.lastBaseRoll;
            int bonus = gameManager.lastRollBonus;
            int final_ = gameManager.lastFinalRoll;

            if (force || cachedBaseRoll != baseRoll || cachedBonus != bonus || cachedFinal != final_)
            {
                cachedBaseRoll = baseRoll;
                cachedBonus = bonus;
                cachedFinal = final_;

                lastRollText.text = (baseRoll == 0 && final_ == 0 && bonus == 0)
                    ? "Last roll: -"
                    : $"Last roll: {final_} (base {baseRoll} + bonus {bonus})";
            }
        }
    }
}
