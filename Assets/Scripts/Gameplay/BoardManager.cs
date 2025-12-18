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
    public int passionRewardAmount = 10;

    [Header("Safe Route - Card Overrides")]
    public bool useSpecificEventCard;
    public EventCardDefinition eventCardOverride;
    public bool useSpecificItemCard;
    public ItemCardDefinition itemCardOverride;

    [Header("Safe Route - Field Card")]
    public bool useFieldCard;
    public FieldCardDefinition fieldCardOverride;

    [Header("Risk Route - Card Overrides")]
    public bool useRiskEventCard;
    public EventCardDefinition riskEventCardOverride;
    public bool useRiskItemCard;
    public ItemCardDefinition riskItemCardOverride;

    [Header("Risk Route - Field Card")]
    public bool useRiskFieldCard;
    public FieldCardDefinition riskFieldCardOverride;

    [Header("Manual Card Input")]
    public bool requiresManualCardIdInput;

    public EventCardDefinition GetEventCard(bool isRisk)
    {
        if (isRisk && useRiskEventCard && riskEventCardOverride != null)
            return riskEventCardOverride;
        if (useSpecificEventCard && eventCardOverride != null)
            return eventCardOverride;
        return null;
    }

    public ItemCardDefinition GetItemCard(bool isRisk)
    {
        if (isRisk && useRiskItemCard && riskItemCardOverride != null)
            return riskItemCardOverride;
        if (useSpecificItemCard && itemCardOverride != null)
            return itemCardOverride;
        return null;
    }

    public FieldCardDefinition GetFieldCard(bool isRisk)
    {
        if (isRisk && useRiskFieldCard && riskFieldCardOverride != null)
            return riskFieldCardOverride;
        if (useFieldCard && fieldCardOverride != null)
            return fieldCardOverride;
        return null;
    }

    public bool HasFieldCard(bool isRisk)
    {
        if (isRisk && useRiskFieldCard && riskFieldCardOverride != null)
            return true;
        return useFieldCard && fieldCardOverride != null;
    }
}

public class BoardManager : MonoBehaviour
{
    public int totalFields = 91;
    public List<BoardFieldDefinition> specialFields = new List<BoardFieldDefinition>();

    private Dictionary<int, BoardFieldDefinition> fieldLookup;
    private List<int> crossroadIndices;

    private void Awake()
    {
        BuildLookup();
    }

    private void Start()
    {
        Debug.Log($"[BoardManager] Initialized with {specialFields.Count} special fields, {crossroadIndices.Count} crossroads at indices: [{string.Join(", ", crossroadIndices)}]");
    }

    public void RebuildLookup()
    {
        BuildLookup();
        Debug.Log($"[BoardManager] Rebuilt lookup. Crossroads: [{string.Join(", ", crossroadIndices)}]");
    }

    private void BuildLookup()
    {
        fieldLookup = new Dictionary<int, BoardFieldDefinition>(specialFields.Count);
        crossroadIndices = new List<int>();

        Debug.Log($"[BoardManager] BuildLookup: Processing {specialFields.Count} special fields...");

        foreach (var field in specialFields)
        {
            if (field != null && !fieldLookup.ContainsKey(field.index))
            {
                fieldLookup[field.index] = field;
                Debug.Log($"[BoardManager] Registered field index {field.index} as {field.fieldType}");

                if (field.fieldType == FieldType.Crossroad)
                {
                    crossroadIndices.Add(field.index);
                    Debug.Log($"[BoardManager] Added crossroad at index {field.index}");
                }
            }
        }

        crossroadIndices.Sort();
        Debug.Log($"[BoardManager] BuildLookup complete. Total crossroads: {crossroadIndices.Count}");
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

    public int FindNextCrossroad(int currentPosition)
    {
        for (int i = 0; i < crossroadIndices.Count; i++)
        {
            if (crossroadIndices[i] > currentPosition)
                return crossroadIndices[i];
        }
        return -1;
    }

    public bool IsCrossroadInPath(int startPosition, int endPosition)
    {
        for (int i = 0; i < crossroadIndices.Count; i++)
        {
            int crossroad = crossroadIndices[i];
            if (crossroad > startPosition && crossroad <= endPosition)
                return true;
        }
        return false;
    }

    public int GetFirstCrossroadInPath(int startPosition, int endPosition)
    {
        Debug.Log($"[BoardManager] Checking crossroads in path {startPosition} -> {endPosition}. Known crossroads: [{string.Join(", ", crossroadIndices)}]");
        
        for (int i = 0; i < crossroadIndices.Count; i++)
        {
            int crossroad = crossroadIndices[i];
            if (crossroad > startPosition && crossroad <= endPosition)
            {
                Debug.Log($"[BoardManager] Found crossroad at index {crossroad}");
                return crossroad;
            }
        }
        
        if (crossroadIndices.Count == 0)
        {
            Debug.Log($"[BoardManager] Crossroad list empty, checking field lookup directly...");
            for (int pos = startPosition + 1; pos <= endPosition; pos++)
            {
                if (fieldLookup != null && fieldLookup.TryGetValue(pos, out var field))
                {
                    if (field.fieldType == FieldType.Crossroad)
                    {
                        Debug.Log($"[BoardManager] Found crossroad at index {pos} via fallback lookup");
                        crossroadIndices.Add(pos);
                        return pos;
                    }
                }
            }
        }
        
        Debug.Log($"[BoardManager] No crossroad found in path");
        return -1;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        BuildLookup();
    }
#endif
}
