using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BoardFieldDefinition
{
    public int index;
    public FieldType fieldType = FieldType.Neutral;

    [TextArea]
    public string description;

    [TextArea]
    public string riskDescription;

    [TextArea]
    public string safeDescription;

    [Header("Passion Scoring")]
    public bool yieldsPassionScore;
    public PassionColor passionReward;
    public int passionRewardAmount = 3;

    [Header("Safe Route - Card IDs")]
    public string safeEventCardId;
    public string safeItemCardId;
    public string safeFieldCardId;

    [Header("Risk Route - Card IDs")]
    public string riskEventCardId;
    public string riskItemCardId;
    public string riskFieldCardId;

    [Header("Special Flags")]
    public bool requiresManualCardIdInput;
    public bool isLastCrossroadsField;

    public string GetDescription(bool isRisk)
    {
        if (isRisk && !string.IsNullOrEmpty(riskDescription))
            return riskDescription;

        if (!isRisk && !string.IsNullOrEmpty(safeDescription))
            return safeDescription;

        return description;
    }

    public EventCardDefinition GetEventCard(bool isRisk, CardManager cardManager)
    {
        if (cardManager == null) return null;

        string cardId = isRisk ? riskEventCardId : safeEventCardId;
        if (string.IsNullOrEmpty(cardId))
        {
            if (isRisk && !string.IsNullOrEmpty(safeEventCardId))
                cardId = safeEventCardId;
            else
                return null;
        }

        return cardManager.GetCardById(cardId) as EventCardDefinition;
    }

    public ItemCardDefinition GetItemCard(bool isRisk, CardManager cardManager)
    {
        if (cardManager == null) return null;

        string cardId = isRisk ? riskItemCardId : safeItemCardId;
        if (string.IsNullOrEmpty(cardId))
        {
            if (isRisk && !string.IsNullOrEmpty(safeItemCardId))
                cardId = safeItemCardId;
            else
                return null;
        }

        return cardManager.GetItemById(cardId);
    }

    public FieldCardDefinition GetFieldCard(bool isRisk, CardManager cardManager)
    {
        if (cardManager == null) return null;

        string cardId = isRisk ? riskFieldCardId : safeFieldCardId;
        if (string.IsNullOrEmpty(cardId))
        {
            if (isRisk && !string.IsNullOrEmpty(safeFieldCardId))
                cardId = safeFieldCardId;
            else
                return null;
        }

        return cardManager.GetCardById(cardId) as FieldCardDefinition;
    }

    public bool HasFieldCard(bool isRisk)
    {
        if (isRisk && !string.IsNullOrEmpty(riskFieldCardId))
            return true;
        return !string.IsNullOrEmpty(safeFieldCardId);
    }

    public bool HasEventCard(bool isRisk)
    {
        if (isRisk && !string.IsNullOrEmpty(riskEventCardId))
            return true;
        return !string.IsNullOrEmpty(safeEventCardId);
    }

    public bool HasItemCard(bool isRisk)
    {
        if (isRisk && !string.IsNullOrEmpty(riskItemCardId))
            return true;
        return !string.IsNullOrEmpty(safeItemCardId);
    }

    public bool IsMinigameField(bool isRisk)
    {
    string id = isRisk ? riskFieldCardId : safeFieldCardId;

    // fallback: wenn risk-route kein eigenes fieldCardId hat, nutze safe
    if (isRisk && string.IsNullOrEmpty(id))
        id = safeFieldCardId;

    return id == CardIds.Fields.MINIGAME;
}

}

public class BoardManager : MonoBehaviour
{
    [Header("Board Settings")]
    public int totalFields = 93;

    [Header("Debug (Read-Only at Runtime)")]
    [Tooltip("Shows loaded field definitions. Populated from BoardFieldConfig.")]
    public List<BoardFieldDefinition> specialFields = new List<BoardFieldDefinition>();

    private Dictionary<int, BoardFieldDefinition> fieldLookup;
    private List<int> crossroadIndices;
    private List<int> lastCrossroadsIndices;

    public enum StopFieldType
    {
        None,
        Crossroad,
        LastCrossroads
    }

    public struct StopFieldResult
    {
        public int index;
        public StopFieldType type;

        public static StopFieldResult None => new StopFieldResult { index = -1, type = StopFieldType.None };
    }

    private void Awake()
    {
        specialFields = BoardFieldConfig.BuildAll();
        Debug.Log($"[BoardManager] Loaded {specialFields.Count} field definitions from BoardFieldConfig.");
        BuildLookup();
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Debug.Log($"[BoardManager] Initialized with {specialFields.Count} special fields, {crossroadIndices.Count} crossroads at [{string.Join(", ", crossroadIndices)}], {lastCrossroadsIndices.Count} last-crossroads at [{string.Join(", ", lastCrossroadsIndices)}]");
    }

    public void RebuildLookup()
    {
        BuildLookup();
        Debug.Log($"[BoardManager] Rebuilt lookup. Crossroads: [{string.Join(", ", crossroadIndices)}], LastCrossroads: [{string.Join(", ", lastCrossroadsIndices)}]");
    }

    private void BuildLookup()
    {
        fieldLookup = new Dictionary<int, BoardFieldDefinition>(specialFields.Count);
        crossroadIndices = new List<int>();
        lastCrossroadsIndices = new List<int>();

        foreach (var field in specialFields)
        {
            if (field != null && !fieldLookup.ContainsKey(field.index))
            {
                fieldLookup[field.index] = field;

                if (field.fieldType == FieldType.Crossroad)
                    crossroadIndices.Add(field.index);

                if (field.isLastCrossroadsField)
                    lastCrossroadsIndices.Add(field.index);
            }
        }

        crossroadIndices.Sort();
        lastCrossroadsIndices.Sort();
    }

    public FieldType GetFieldTypeAt(int positionIndex)
    {
        if (fieldLookup != null && fieldLookup.TryGetValue(positionIndex, out var field))
            return field.fieldType;

        if (positionIndex >= totalFields - 1)
            return FieldType.Finish;

        return FieldType.Neutral;
    }

    public BoardFieldDefinition GetFieldDefinitionAt(int positionIndex)
    {
        if (fieldLookup != null && fieldLookup.TryGetValue(positionIndex, out var field))
            return field;
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
        for (int i = 0; i < crossroadIndices.Count; i++)
        {
            int crossroad = crossroadIndices[i];
            if (crossroad > startPosition && crossroad <= endPosition)
                return crossroad;
        }
        return -1;
    }

    public StopFieldResult GetFirstStopFieldInPath(int startPosition, int endPosition)
    {
        int firstCrossroad = -1;
        int firstLastCrossroads = -1;

        for (int i = 0; i < crossroadIndices.Count; i++)
        {
            if (crossroadIndices[i] > startPosition && crossroadIndices[i] <= endPosition)
            {
                firstCrossroad = crossroadIndices[i];
                break;
            }
        }

        for (int i = 0; i < lastCrossroadsIndices.Count; i++)
        {
            if (lastCrossroadsIndices[i] > startPosition && lastCrossroadsIndices[i] <= endPosition)
            {
                firstLastCrossroads = lastCrossroadsIndices[i];
                break;
            }
        }

        if (firstCrossroad < 0 && firstLastCrossroads < 0)
            return StopFieldResult.None;

        if (firstCrossroad >= 0 && (firstLastCrossroads < 0 || firstCrossroad < firstLastCrossroads))
            return new StopFieldResult { index = firstCrossroad, type = StopFieldType.Crossroad };

        return new StopFieldResult { index = firstLastCrossroads, type = StopFieldType.LastCrossroads };
    }

    public int GetFirstLastCrossroadsInPath(int startPosition, int endPosition)
    {
        for (int i = 0; i < lastCrossroadsIndices.Count; i++)
        {
            int lc = lastCrossroadsIndices[i];
            if (lc > startPosition && lc <= endPosition)
                return lc;
        }
        return -1;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (specialFields != null && specialFields.Count > 0)
            BuildLookup();
    }
#endif
}