using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    [Header("Event Cards")]
    public List<EventCardDefinition> eventCards = new List<EventCardDefinition>();

    [Header("Item Cards")]
    public List<ItemCardDefinition> itemCards = new List<ItemCardDefinition>();

    [Header("Field Cards")]
    public List<FieldCardDefinition> fieldCards = new List<FieldCardDefinition>();

    private Dictionary<string, EventCardDefinition> eventLookup;
    private Dictionary<string, ItemCardDefinition> itemLookup;
    private Dictionary<string, FieldCardDefinition> fieldLookup;
    private bool isInitialized;

    private void Awake()
    {
        BuildLookups();
    }

    private void BuildLookups()
    {
        if (isInitialized) return;

        eventLookup = new Dictionary<string, EventCardDefinition>(eventCards.Count);
        foreach (var card in eventCards)
        {
            if (card != null && !string.IsNullOrEmpty(card.id))
                eventLookup[card.id] = card;
        }

        itemLookup = new Dictionary<string, ItemCardDefinition>(itemCards.Count);
        foreach (var card in itemCards)
        {
            if (card != null && !string.IsNullOrEmpty(card.id))
                itemLookup[card.id] = card;
        }

        fieldLookup = new Dictionary<string, FieldCardDefinition>(fieldCards.Count);
        foreach (var card in fieldCards)
        {
            if (card != null && !string.IsNullOrEmpty(card.id))
                fieldLookup[card.id] = card;
        }

        isInitialized = true;
    }

    public EventCardDefinition DrawRandomEventCard()
    {
        if (eventCards == null || eventCards.Count == 0)
        {
            Debug.LogWarning("[CardManager] No event cards defined.");
            return null;
        }

        int index = Random.Range(0, eventCards.Count);
        var card = eventCards[index];
        Debug.Log($"[CardManager] Drew event card: {card.title}");
        return card;
    }

    public ItemCardDefinition DrawRandomItemCard()
    {
        if (itemCards == null || itemCards.Count == 0)
        {
            Debug.LogWarning("[CardManager] No item cards defined.");
            return null;
        }

        int index = Random.Range(0, itemCards.Count);
        var card = itemCards[index];
        Debug.Log($"[CardManager] Drew item card: {card.title}");
        return card;
    }

    public ItemCardDefinition GetItemById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        BuildLookups();
        return itemLookup.TryGetValue(id, out var item) ? item : null;
    }

    public BaseCardDefinition GetCardById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        BuildLookups();

        string trimmed = id.Trim();

        if (itemLookup.TryGetValue(trimmed, out var item))
        {
            Debug.Log($"[CardManager] GetCardById('{id}') → ITEM: {item.title}");
            return item;
        }

        if (eventLookup.TryGetValue(trimmed, out var ev))
        {
            Debug.Log($"[CardManager] GetCardById('{id}') → EVENT: {ev.title}");
            return ev;
        }

        if (fieldLookup.TryGetValue(trimmed, out var field))
        {
            Debug.Log($"[CardManager] GetCardById('{id}') → FIELD: {field.title}");
            return field;
        }

        Debug.LogWarning($"[CardManager] GetCardById('{id}') → no card found.");
        return null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        isInitialized = false;
    }
#endif
}

