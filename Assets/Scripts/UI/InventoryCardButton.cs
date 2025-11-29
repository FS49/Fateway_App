using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryCardButton : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public Button button;

    private BaseCardDefinition card;
    private CardDescriptionPopup popup;

    public void Init(BaseCardDefinition card, CardDescriptionPopup popup)
    {
        this.card = card;
        this.popup = popup;

        if (titleText != null)
            titleText.text = card.title;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);
        }
    }

    private void OnClicked()
    {
        if (popup != null && card != null)
            popup.Show(card);
    }
}
