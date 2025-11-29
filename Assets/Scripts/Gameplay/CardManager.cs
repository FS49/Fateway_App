using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    [Header("Event Cards")] public List<EventCardDefinition> eventCards = new List<EventCardDefinition>();

    [Header("Item Cards")] public List<ItemCardDefinition> itemCards = new List<ItemCardDefinition>();

    [Header("Field Cards")] public List<FieldCardDefinition> fieldCards = new List<FieldCardDefinition>();

    // ---------- Random draws ----------

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

    // ---------- Lookups ----------

    public ItemCardDefinition GetItemById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        foreach (var item in itemCards)
        {
            if (item != null && item.id == id)
                return item;
        }

        return null;
    }

    /// <summary>
    /// Lookup by ID across all card types (event, item, field).
    /// Used for manual card ID input from the player.
    /// </summary>
    public BaseCardDefinition GetCardById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        string trimmed = id.Trim();
        string upper = trimmed.ToUpperInvariant();

        BaseCardDefinition found = null;

        // If ID looks like an item (IT...), check items first.
        if (upper.StartsWith("IT"))
        {
            foreach (var item in itemCards)
            {
                if (item != null && item.id == trimmed)
                {
                    found = item;
                    break;
                }
            }

            if (found != null)
            {
                Debug.Log(
                    $"[CardManager] GetCardById('{id}') → ITEM: {found.title} ({found.GetType().Name}), effectType={found.effectType}");
                return found;
            }
        }

        // If ID looks like an event (EV...), check events first.
        if (upper.StartsWith("EV"))
        {
            foreach (var ev in eventCards)
            {
                if (ev != null && ev.id == trimmed)
                {
                    found = ev;
                    break;
                }
            }

            if (found != null)
            {
                Debug.Log(
                    $"[CardManager] GetCardById('{id}') → EVENT: {found.title} ({found.GetType().Name}), effectType={found.effectType}");
                return found;
            }
        }

        // Otherwise / fallback: search all lists.

        foreach (var ev in eventCards)
        {
            if (ev != null && ev.id == trimmed)
            {
                found = ev;
                break;
            }
        }

        if (found == null)
        {
            foreach (var item in itemCards)
            {
                if (item != null && item.id == trimmed)
                {
                    found = item;
                    break;
                }
            }
        }

        if (found == null)
        {
            foreach (var field in fieldCards)
            {
                if (field != null && field.id == trimmed)
                {
                    found = field;
                    break;
                }
            }
        }

        if (found != null)
        {
            Debug.Log(
                $"[CardManager] GetCardById('{id}') → {found.title} ({found.GetType().Name}), effectType={found.effectType}");
        }
        else
        {
            Debug.LogWarning($"[CardManager] GetCardById('{id}') → no card found.");
        }

        return found;
    }
}
