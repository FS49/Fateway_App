using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDescriptionPopup : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;

    public GameObject panelRoot;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    [Header("Item Tag Icons")]
    public Transform tagIconContainer;
    public GameObject tagIconPrefab;          // e.g. an Image with optional TMP label
    public ItemTagIconRegistry tagIconRegistry;

    private BaseCardDefinition currentCard;

    private void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        Hide();
    }

    public void Show(BaseCardDefinition card)
    {
        if (card == null) return;
        currentCard = card;

        if (panelRoot != null)
            panelRoot.SetActive(true);

        if (titleText != null)
            titleText.text = card.title;

        if (descriptionText != null)
            descriptionText.text = card.description;

        ClearTagIcons();

        if (card is ItemCardDefinition item && tagIconContainer != null && tagIconPrefab != null)
        {
            if (item.tags != null)
            {
                foreach (var tag in item.tags)
                {
                    if (string.IsNullOrEmpty(tag)) continue;

                    GameObject iconGO = Instantiate(tagIconPrefab, tagIconContainer);
                    var img = iconGO.GetComponent<Image>();
                    var label = iconGO.GetComponentInChildren<TextMeshProUGUI>();

                    if (img != null && tagIconRegistry != null)
                    {
                        var sprite = tagIconRegistry.GetIcon(tag);
                        img.sprite = sprite;
                        img.enabled = (sprite != null);
                    }

                    if (label != null)
                    {
                        label.text = tag;
                    }
                }
            }
        }

        if (gameManager != null)
            gameManager.OnCardPopupOpened();
    }

    public void Hide()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        ClearTagIcons();
        currentCard = null;

        if (gameManager != null)
            gameManager.OnCardPopupClosed();
    }

    private void ClearTagIcons()
    {
        if (tagIconContainer == null) return;

        for (int i = tagIconContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(tagIconContainer.GetChild(i).gameObject);
        }
    }

    // Hook this to the X button
    public void OnCloseButtonClicked()
    {
        Hide();
    }
}
