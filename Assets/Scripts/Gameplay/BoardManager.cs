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
    public bool yieldsPassionScore;
    public PassionColor passionReward;

    [Header("Card Overrides")]
    public bool useSpecificEventCard;
    public EventCardDefinition eventCardOverride;
    public bool useSpecificItemCard;
    public ItemCardDefinition itemCardOverride;

    [Header("Field Card")]
    public bool useFieldCard;
    public FieldCardDefinition fieldCardOverride;

    [Header("Manual Card Input")]
    public bool requiresManualCardIdInput;
}

public class BoardManager : MonoBehaviour
{
    public int totalFields = 91;
    public List<BoardFieldDefinition> specialFields = new List<BoardFieldDefinition>();

    private Dictionary<int, BoardFieldDefinition> fieldLookup;

    private void Awake()
    {
        BuildLookup();
    }

    private void BuildLookup()
    {
        fieldLookup = new Dictionary<int, BoardFieldDefinition>(specialFields.Count);
        foreach (var field in specialFields)
        {
            if (field != null && !fieldLookup.ContainsKey(field.index))
            {
                fieldLookup[field.index] = field;
            }
        }
    }

    public FieldType GetFieldTypeAt(int positionIndex)
    {
        if (fieldLookup != null && fieldLookup.TryGetValue(positionIndex, out var field))
        {
            return field.fieldType;
        }

        if (positionIndex >= totalFields - 1)
            return FieldType.Finish;

        return FieldType.Neutral;
    }

    public BoardFieldDefinition GetFieldDefinitionAt(int positionIndex)
    {
        if (fieldLookup != null && fieldLookup.TryGetValue(positionIndex, out var field))
        {
            return field;
        }
        return null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        BuildLookup();
    }
#endif
}
