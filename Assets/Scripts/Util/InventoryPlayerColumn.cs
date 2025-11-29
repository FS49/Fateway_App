using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryPlayerColumn : MonoBehaviour
{
    [Header("UI References")]
    public Image backgroundImage;
    public TextMeshProUGUI playerNameText;
    public Transform statusEffectsContainer;
    public Transform itemsContainer;

    public void Init(PlayerData player)
    {
        if (playerNameText != null)
            playerNameText.text = player.playerName;

        if (backgroundImage != null)
            backgroundImage.color = PassionColorUtils.GetColor(player.passion);
    }
}