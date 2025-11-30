using UnityEngine;

public class PartnerButton : MonoBehaviour
{
    public PartnerPanelUI partnerPanel;

    private void Awake()
    {
        if (partnerPanel == null)
            partnerPanel = FindObjectOfType<PartnerPanelUI>();
    }

    public void OnPartnerButtonClicked()
    {
        if (partnerPanel != null)
            partnerPanel.Show();
    }
}

