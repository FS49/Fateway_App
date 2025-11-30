using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PartnerPlayerRow : MonoBehaviour
{
    [Header("UI References")]
    public Image backgroundImage;
    public TextMeshProUGUI playerNameText;
    public Toggle selectionToggle;
    public Image checkmarkImage;

    private PlayerData player;
    private int playerIndex;
    private Action<PartnerPlayerRow, bool> onSelectionChanged;

    public void Initialize(PlayerData playerData, int index, Action<PartnerPlayerRow, bool> selectionCallback)
    {
        player = playerData;
        playerIndex = index;
        onSelectionChanged = selectionCallback;

        if (playerNameText != null)
            playerNameText.text = playerData.playerName;

        if (backgroundImage != null)
            backgroundImage.color = PassionColorUtils.GetColor(playerData.passion);

        if (selectionToggle != null)
        {
            selectionToggle.isOn = false;
            selectionToggle.onValueChanged.RemoveAllListeners();
            selectionToggle.onValueChanged.AddListener(OnToggleChanged);
        }

        UpdateCheckmark(false);
    }

    private void OnToggleChanged(bool isOn)
    {
        UpdateCheckmark(isOn);
        onSelectionChanged?.Invoke(this, isOn);
    }

    private void UpdateCheckmark(bool isOn)
    {
        if (checkmarkImage != null)
            checkmarkImage.enabled = isOn;
    }

    public void SetSelected(bool selected)
    {
        if (selectionToggle != null && selectionToggle.isOn != selected)
        {
            selectionToggle.SetIsOnWithoutNotify(selected);
            UpdateCheckmark(selected);
        }
    }

    public bool IsSelected => selectionToggle != null && selectionToggle.isOn;
    public PlayerData Player => player;
    public int PlayerIndex => playerIndex;
}

