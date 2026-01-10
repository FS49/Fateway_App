using System.Collections.Generic;

public static class BoardFieldConfig
{
    private const int TOTAL_FIELDS = 93;

    public static List<BoardFieldDefinition> BuildAll()
    {
        var fields = CreateAllFields(TOTAL_FIELDS);

        // =====================================================================
        // CROSSROADS (universal)
        // =====================================================================
        SetFieldType(fields, new[] { 9, 37, 65 }, FieldType.Crossroad);
        SetDescription(fields, new[] { 9, 37, 65 }, "Kreuzung");

        // =====================================================================
        // CROSSROAD RISK-EVALUATION (Last Crossroads Fields)
        // =====================================================================
        SetLastCrossroads(fields, new[] { 28, 56, 84 });
        SetDescription(fields, new[] { 28, 56, 84 }, "Risiko-Auswertung");

        // =====================================================================
        // MINIGAMES (safe)
        // =====================================================================
        SetSafeFieldCard(fields, new[] { 7, 17, 24, 31, 50, 63, 67, 75, 78, 89 }, CardIds.Fields.MINIGAME);
        SetDescription(fields, new[] { 7, 17, 24, 31, 50, 63, 67, 75, 78, 89 }, "Minispiel");

        // =====================================================================
        // MINIGAMES (risk)
        // =====================================================================
        SetRiskFieldCard(fields, new[] { 11, 18, 21, 27, 42, 48, 54, 70, 73, 81, 83 }, CardIds.Fields.MINIGAME);
        SetRiskDescription(fields, new[] { 11, 18, 21, 27, 42, 48, 54, 70, 73, 81, 83 }, "Minispiel");

        // =====================================================================
        // EVENTS (safe)
        // =====================================================================
        SetSafeFieldCard(fields, new[] { 2, 6, 11, 15, 18, 21, 23, 27, 30, 33, 36, 39, 42, 46, 49, 54, 59, 61, 68, 73, 77, 80, 85, 88, 91 }, CardIds.Fields.CARD_INPUT);
        SetDescription(fields, new[] { 2, 6, 11, 15, 18, 21, 23, 27, 30, 33, 36, 39, 42, 46, 49, 54, 59, 61, 68, 73, 77, 80, 85, 88, 91 }, "Ereignis");

        // =====================================================================
        // EVENTS (risk)
        // =====================================================================
        SetRiskFieldCard(fields, new[] { 10, 12, 15, 19, 22, 24, 38, 41, 43, 46, 50, 52, 66, 69, 72, 75, 79, 82 }, CardIds.Fields.CARD_INPUT);
        SetRiskDescription(fields, new[] { 10, 12, 15, 19, 22, 24, 38, 41, 43, 46, 50, 52, 66, 69, 72, 75, 79, 82 }, "Ereignis");

        // =====================================================================
        // REST (safe) - nothing happens here
        // =====================================================================
        SetSafeFieldCard(fields, new[] { 3, 13, 20, 25, 35, 40, 44, 47, 56, 71, 83, 86 }, CardIds.Fields.REST);
        SetDescription(fields, new[] { 3, 13, 20, 25, 35, 40, 44, 47, 56, 71, 83, 86 }, "Ruhefeld");

        // =====================================================================
        // REST (risk) - nothing happens here
        // =====================================================================
        SetRiskFieldCard(fields, new[] { 14, 25, 39, 45, 51, 67, 76, 78 }, CardIds.Fields.REST);
        SetRiskDescription(fields, new[] { 14, 25, 39, 45, 51, 67, 76, 78 }, "Ruhefeld");

        // =====================================================================
        // SHOP (safe)
        // =====================================================================
        SetSafeFieldCard(fields, new[] { 4, 12, 28, 43, 52, 58, 64, 70, 81 }, CardIds.Fields.CARD_INPUT);
        SetDescription(fields, new[] { 4, 12, 28, 43, 52, 58, 64, 70, 81 }, "Shop");

        // =====================================================================
        // SHOP (risk)
        // =====================================================================
        SetRiskFieldCard(fields, new[] { 16 }, CardIds.Fields.CARD_INPUT);
        SetRiskDescription(fields, new[] { 16 }, "Shop");

        // =====================================================================
        // POINTS (safe)
        // =====================================================================
        SetSafeFieldCard(fields, new[] { 1, 10, 14, 16, 19, 22, 26, 29, 32, 38, 41, 45, 48, 51, 53, 55, 57, 60, 62, 69, 72, 74, 76, 79, 82, 84, 87, 90 }, CardIds.Fields.MAIN_PASSION_POINTS);
        SetDescription(fields, new[] { 1, 10, 14, 16, 19, 22, 26, 29, 32, 38, 41, 45, 48, 51, 53, 55, 57, 60, 62, 69, 72, 74, 76, 79, 82, 84, 87, 90 }, "Leidenschaftspunkte!");

        // =====================================================================
        // POINTS (risk)
        // =====================================================================
        SetRiskFieldCard(fields, new[] { 13, 17, 20, 23, 26, 40, 44, 47, 49, 53, 55, 68, 71, 74, 77, 80 }, CardIds.Fields.MAIN_PASSION_POINTS);
        SetRiskDescription(fields, new[] { 13, 17, 20, 23, 26, 40, 44, 47, 49, 53, 55, 68, 71, 74, 77, 80 }, "Leidenschaftspunkte!");

        // =====================================================================
        // FINISH
        // =====================================================================
        SetFieldType(fields, 92, FieldType.Finish);
        SetDescription(fields, 92, "Ziel");

        return fields;
    }

    // =========================================================================
    // FIELD CREATION
    // =========================================================================

    private static List<BoardFieldDefinition> CreateAllFields(int count)
    {
        var fields = new List<BoardFieldDefinition>(count);
        for (int i = 0; i < count; i++)
        {
            fields.Add(new BoardFieldDefinition
            {
                index = i,
                fieldType = FieldType.Neutral,
                description = "",
                yieldsPassionScore = false,
                passionReward = PassionColor.Yellow,
                passionRewardAmount = 0,
                requiresManualCardIdInput = false,
                isLastCrossroadsField = false,
                safeEventCardId = null,
                riskEventCardId = null,
                safeItemCardId = null,
                riskItemCardId = null,
                safeFieldCardId = null,
                riskFieldCardId = null,
                safeDescription = null,
                riskDescription = null
            });
        }
        return fields;
    }

    // =========================================================================
    // FIELD TYPE SETTERS
    // =========================================================================

    private static void SetFieldType(List<BoardFieldDefinition> fields, int[] indices, FieldType type)
    {
        for (int i = 0; i < indices.Length; i++)
            SetFieldType(fields, indices[i], type);
    }

    private static void SetFieldType(List<BoardFieldDefinition> fields, int index, FieldType type)
    {
        if (index >= 0 && index < fields.Count)
            fields[index].fieldType = type;
    }

    // =========================================================================
    // DESCRIPTION SETTERS
    // =========================================================================

    private static void SetDescription(List<BoardFieldDefinition> fields, int[] indices, string desc)
    {
        for (int i = 0; i < indices.Length; i++)
            SetDescription(fields, indices[i], desc);
    }

    private static void SetDescription(List<BoardFieldDefinition> fields, int index, string desc)
    {
        if (index >= 0 && index < fields.Count)
            fields[index].description = desc;
    }

    private static void SetRiskDescription(List<BoardFieldDefinition> fields, int[] indices, string desc)
    {
        for (int i = 0; i < indices.Length; i++)
            SetRiskDescription(fields, indices[i], desc);
    }

    private static void SetRiskDescription(List<BoardFieldDefinition> fields, int index, string desc)
    {
        if (index >= 0 && index < fields.Count)
            fields[index].riskDescription = desc;
    }

    // =========================================================================
    // FIELD CARD SETTERS
    // =========================================================================

    private static void SetSafeFieldCard(List<BoardFieldDefinition> fields, int[] indices, string cardId)
    {
        for (int i = 0; i < indices.Length; i++)
            SetSafeFieldCard(fields, indices[i], cardId);
    }

    private static void SetSafeFieldCard(List<BoardFieldDefinition> fields, int index, string cardId)
    {
        if (index >= 0 && index < fields.Count)
            fields[index].safeFieldCardId = cardId;
    }

    private static void SetRiskFieldCard(List<BoardFieldDefinition> fields, int[] indices, string cardId)
    {
        for (int i = 0; i < indices.Length; i++)
            SetRiskFieldCard(fields, indices[i], cardId);
    }

    private static void SetRiskFieldCard(List<BoardFieldDefinition> fields, int index, string cardId)
    {
        if (index >= 0 && index < fields.Count)
            fields[index].riskFieldCardId = cardId;
    }

    // =========================================================================
    // EVENT CARD SETTERS
    // =========================================================================

    private static void SetSafeEventCard(List<BoardFieldDefinition> fields, int[] indices, string cardId)
    {
        for (int i = 0; i < indices.Length; i++)
            SetSafeEventCard(fields, indices[i], cardId);
    }

    private static void SetSafeEventCard(List<BoardFieldDefinition> fields, int index, string cardId)
    {
        if (index >= 0 && index < fields.Count)
            fields[index].safeEventCardId = cardId;
    }

    private static void SetRiskEventCard(List<BoardFieldDefinition> fields, int[] indices, string cardId)
    {
        for (int i = 0; i < indices.Length; i++)
            SetRiskEventCard(fields, indices[i], cardId);
    }

    private static void SetRiskEventCard(List<BoardFieldDefinition> fields, int index, string cardId)
    {
        if (index >= 0 && index < fields.Count)
            fields[index].riskEventCardId = cardId;
    }

    // =========================================================================
    // ITEM CARD SETTERS
    // =========================================================================

    private static void SetSafeItemCard(List<BoardFieldDefinition> fields, int[] indices, string cardId)
    {
        for (int i = 0; i < indices.Length; i++)
            SetSafeItemCard(fields, indices[i], cardId);
    }

    private static void SetSafeItemCard(List<BoardFieldDefinition> fields, int index, string cardId)
    {
        if (index >= 0 && index < fields.Count)
            fields[index].safeItemCardId = cardId;
    }

    private static void SetRiskItemCard(List<BoardFieldDefinition> fields, int[] indices, string cardId)
    {
        for (int i = 0; i < indices.Length; i++)
            SetRiskItemCard(fields, indices[i], cardId);
    }

    private static void SetRiskItemCard(List<BoardFieldDefinition> fields, int index, string cardId)
    {
        if (index >= 0 && index < fields.Count)
            fields[index].riskItemCardId = cardId;
    }

    // =========================================================================
    // FLAG SETTERS
    // =========================================================================

    private static void SetLastCrossroads(List<BoardFieldDefinition> fields, int[] indices)
    {
        for (int i = 0; i < indices.Length; i++)
        {
            if (indices[i] >= 0 && indices[i] < fields.Count)
                fields[indices[i]].isLastCrossroadsField = true;
        }
    }

    private static void SetManualInput(List<BoardFieldDefinition> fields, int[] indices)
    {
        for (int i = 0; i < indices.Length; i++)
        {
            if (indices[i] >= 0 && indices[i] < fields.Count)
                fields[indices[i]].requiresManualCardIdInput = true;
        }
    }

    // =========================================================================
    // PASSION REWARD SETTERS
    // =========================================================================

    private static void SetPassionReward(List<BoardFieldDefinition> fields, int[] indices, PassionColor color, int amount)
    {
        for (int i = 0; i < indices.Length; i++)
            SetPassionReward(fields, indices[i], color, amount);
    }

    private static void SetPassionReward(List<BoardFieldDefinition> fields, int index, PassionColor color, int amount)
    {
        if (index >= 0 && index < fields.Count)
        {
            fields[index].yieldsPassionScore = true;
            fields[index].passionReward = color;
            fields[index].passionRewardAmount = amount;
        }
    }
}
