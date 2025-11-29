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
    public GameObject tagIconPrefab;
    public ItemTagIconRegistry tagIconRegistry;

    private void Start()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        Hide();
    }

    public void Show(BaseCardDefinition card)
    {
        if (card == null) return;

        if (panelRoot != null)
            panelRoot.SetActive(true);

        if (titleText != null)
            titleText.text = card.title;

        if (descriptionText != null)
            descriptionText.text = card.description;

        ClearTagIcons();

        if (card is ItemCardDefinition item && item.tags != null && tagIconContainer != null && tagIconPrefab != null)
        {
            for (int i = 0; i < item.tags.Length; i++)
            {
                var tag = item.tags[i];
                if (string.IsNullOrEmpty(tag)) continue;

                GameObject iconGO = Instantiate(tagIconPrefab, tagIconContainer);
                var img = iconGO.GetComponent<Image>();
                var label = iconGO.GetComponentInChildren<TextMeshProUGUI>();

                if (img != null && tagIconRegistry != null)
                {
                    var sprite = tagIconRegistry.GetIcon(tag);
                    img.sprite = sprite;
                    img.enabled = sprite != null;
                }

                if (label != null)
                    label.text = tag;
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

    public void OnCloseButtonClicked()
    {
        Hide();
    }
}
