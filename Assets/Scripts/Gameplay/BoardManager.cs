using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BoardFieldDefinition
{
    public int index;
    public FieldType fieldType = FieldType.Neutral;

    [TextArea]
    public string description;

    [Header("Passion Scoring")]
    [Tooltip("If true, an event/field on this tile yields points in this passion.")]
    public bool yieldsPassionScore;

    [Tooltip("Which passion this field rewards (used when a points card is applied).")]
    public PassionColor passionReward;

    [Header("Card Overrides")]
    [Tooltip("If true and Field Type is Event, use this specific event card instead of a random one.")]
    public bool useSpecificEventCard;
    public EventCardDefinition eventCardOverride;

    [Tooltip("If true and Field Type is ItemShop, use this specific item card instead of a random one.")]
    public bool useSpecificItemCard;
    public ItemCardDefinition itemCardOverride;

    [Header("Field Card (Neutral/simple points)")]
    [Tooltip("If true, landing here will apply this FieldCard (simple points).")]
    public bool useFieldCard;
    public FieldCardDefinition fieldCardOverride;

    [Header("Manual Card Input")]
    [Tooltip("If true, player must input a card ID from their stack when landing here.")]
    public bool requiresManualCardIdInput;
}

public class BoardManager : MonoBehaviour
{
    [Tooltip("Total number of fields on the physical board.")]
    public int totalFields = 91;

    [Tooltip("Definitions for special fields (events, shops, minigames, crossroads, finish, etc.).")]
    public List<BoardFieldDefinition> specialFields = new List<BoardFieldDefinition>();

    public FieldType GetFieldTypeAt(int positionIndex)
    {
        foreach (var field in specialFields)
        {
            if (field.index == positionIndex)
            {
                return field.fieldType;
            }
        }

        if (positionIndex >= totalFields - 1)
            return FieldType.Finish;

        return FieldType.Neutral;
    }

    public BoardFieldDefinition GetFieldDefinitionAt(int positionIndex)
    {
        foreach (var field in specialFields)
        {
            if (field.index == positionIndex)
            {
                return field;
            }
        }

        return null;
    }
}
