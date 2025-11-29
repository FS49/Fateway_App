using UnityEngine;

public class CardResolver
{
    private GameManager game;

    public CardResolver(GameManager gameManager)
    {
        game = gameManager;
    }

    public void ApplyCard(BaseCardDefinition card, PlayerData targetPlayer, PassionColor? passionForPoints = null)
    {
        if (card == null)
        {
            Debug.LogWarning("[CardResolver] Card is null.");
            return;
        }

        if (targetPlayer == null)
        {
            Debug.LogWarning("[CardResolver] Target player is null.");
            return;
        }

        Debug.Log($"[CardResolver] Applying card: {card.title} to {targetPlayer.playerName}");
        Debug.Log($"[CardResolver] Card type: {card.GetType().Name}, effectType: {card.effectType}");

        switch (card.effectType)
        {
            case CardEffectType.GivePoints:
                if (card is PointsCardDefinition pointsCard)
                {
                    ApplyPointsCard(pointsCard, targetPlayer, passionForPoints);
                }
                else
                {
                    Debug.LogWarning("[CardResolver] GivePoints effect on non-PointsCardDefinition.");
                }
                break;

            case CardEffectType.GiveItem:
                GiveItem(card as ItemCardDefinition, targetPlayer);
                break;

            case CardEffectType.TakeItem:
                TakeItem(card as ItemCardDefinition, targetPlayer);
                break;

            case CardEffectType.StartMinigame:
                StartMinigame(card as EventCardDefinition, targetPlayer);
                break;

            case CardEffectType.ScheduleRiskOutcome:
                ScheduleRiskOutcome(card as EventCardDefinition, targetPlayer, passionForPoints);
                break;

            case CardEffectType.HelpLastPlacePlayer:
                HelpLastPlace(card as EventCardDefinition, passionForPoints);
                break;

            case CardEffectType.ShowInventory:
                game.OpenInventoryView();
                break;

            case CardEffectType.Custom:
                Debug.Log("[CardResolver] Custom effect type - implement as needed.");
                break;

            default:
                Debug.LogWarning($"[CardResolver] Unhandled CardEffectType: {card.effectType}");
                break;
        }

        // If this card should appear in the Status Effects list, register it.
        game.AddStatusEffect(targetPlayer, card);

        PlayCardMedia(card);
    }

    // =====================================================================
    //  POINTS (NEW: multi-passion + main-passion support)
    // =====================================================================

    private void ApplyPointsCard(PointsCardDefinition card, PlayerData p, PassionColor? passionOverride)
    {
        if (card == null || p == null)
            return;

        // 1) Multi-passion points (like PassionAddConfig)
        if (card.useMultiPassionPoints && card.multiPassionPoints != null)
        {
            ApplyMultiPassionPoints(card, p);
        }

        // 2) Extra main passion points (uses player.passion)
        if (card.useMainPassionPoints && card.mainPassionPointsDelta != 0)
        {
            PassionColor main = p.passion;
            int basePoints = card.mainPassionPointsDelta;
            AddPointsWithMultipliers(p, main, basePoints, card.title, "Main passion points");
        }

        // 3) Simple points (legacy style)
        if (card.useSimplePoints && card.pointsDelta != 0)
        {
            PassionColor passionToUse;

            if (card.applyToMainPassion)
            {
                passionToUse = p.passion;
            }
            else if (passionOverride.HasValue)
            {
                // external override (e.g. field-specific passion)
                passionToUse = passionOverride.Value;
            }
            else
            {
                passionToUse = card.simplePassion;
            }

            AddPointsWithMultipliers(p, passionToUse, card.pointsDelta, card.title, "Simple points");
        }
    }

    private void ApplyMultiPassionPoints(PointsCardDefinition card, PlayerData p)
    {
        ApplyPassionDelta(p, card, PassionColor.Yellow, card.multiPassionPoints.yellow);
        ApplyPassionDelta(p, card, PassionColor.Green,  card.multiPassionPoints.green);
        ApplyPassionDelta(p, card, PassionColor.Blue,   card.multiPassionPoints.blue);
        ApplyPassionDelta(p, card, PassionColor.Purple, card.multiPassionPoints.purple);
        ApplyPassionDelta(p, card, PassionColor.Pink,   card.multiPassionPoints.pink);
        ApplyPassionDelta(p, card, PassionColor.Orange, card.multiPassionPoints.orange);
    }

    private void ApplyPassionDelta(PlayerData p, PointsCardDefinition card, PassionColor passion, int basePoints)
    {
        if (basePoints == 0)
            return;

        AddPointsWithMultipliers(p, passion, basePoints, card.title, "Multi-passion points");
    }

    /// <summary>
    /// Central place to apply points with:
    /// - 1.2x if passion == player's main passion
    /// - Item-based score multipliers
    /// - Stars via GameManager.AddPassionPoints
    /// </summary>
    private void AddPointsWithMultipliers(PlayerData p, PassionColor passion, int basePoints, string cardTitle, string context)
    {
        if (basePoints == 0)
            return;

        float factor = 1f;

        // Rule: main passion gets 1.2x
        if (passion == p.passion)
            factor *= 1.2f;

        // Item-based multiplier logic (your existing system)
        factor *= game.GetItemScoreMultiplier(p, passion);

        int finalPoints = Mathf.RoundToInt(basePoints * factor);

        if (finalPoints == 0)
            return;

        game.AddPassionPoints(p, passion, finalPoints);

        Debug.Log(
            $"[CardResolver] ({context}) Card '{cardTitle}' gives {finalPoints} points in {passion} " +
            $"to {p.playerName} (base {basePoints}, factor {factor:F2}). Total score now: {p.GetTotalScore()}"
        );
    }

    // =====================================================================
    //  ITEMS
    // =====================================================================

    private void GiveItem(ItemCardDefinition itemCard, PlayerData p)
    {
        if (itemCard == null || p == null) return;

        if (itemCard.uniquePerPlayer && p.inventory.Contains(itemCard.id))
        {
            Debug.Log($"[CardResolver] {p.playerName} already has unique item '{itemCard.id}'. Skipping.");
            return;
        }

        p.inventory.Add(itemCard.id);
        Debug.Log($"[CardResolver] {p.playerName} receives item: {itemCard.title} ({itemCard.id})");
    }

    private void TakeItem(ItemCardDefinition itemCard, PlayerData p)
    {
        if (itemCard == null || p == null) return;

        if (p.inventory.Contains(itemCard.id))
        {
            p.inventory.Remove(itemCard.id);
            Debug.Log($"[CardResolver] {p.playerName} loses item: {itemCard.title} ({itemCard.id})");
        }
        else
        {
            Debug.Log($"[CardResolver] {p.playerName} does not have item: {itemCard.id}");
        }
    }

    // =====================================================================
    //  MINIGAME
    // =====================================================================

    private void StartMinigame(EventCardDefinition card, PlayerData p)
    {
        if (card == null || p == null) return;
        if (!card.triggersMinigame)
        {
            Debug.LogWarning("[CardResolver] Card does not have triggersMinigame set, but effectType is StartMinigame.");
            return;
        }

        Debug.Log($"[CardResolver] Starting minigame '{card.minigameId}' for {p.playerName}");
        game.StartMinigame(card.minigameId, p);
    }

    // =====================================================================
    //  RISK OUTCOMES
    // =====================================================================

    private void ScheduleRiskOutcome(EventCardDefinition card, PlayerData p, PassionColor? passionForPoints)
    {
        if (card == null || p == null) return;

        Debug.Log($"[CardResolver] Scheduling risk outcome card '{card.id}' for {p.playerName}.");
        game.ScheduleRiskOutcome(card, p, passionForPoints);
    }

    private void HelpLastPlace(EventCardDefinition card, PassionColor? passionOverride)
    {
        if (card == null) return;

        PlayerData last = game.GetLastPlacePlayer();
        if (last == null)
        {
            Debug.LogWarning("[CardResolver] No last-place player to help.");
            return;
        }

        // Use the same advanced points logic, but targeting the last-place player.
        if (card is PointsCardDefinition pointsCard)
        {
            Debug.Log($"[CardResolver] Helping last place {last.playerName} using card '{card.title}'.");
            ApplyPointsCard(pointsCard, last, passionOverride);
        }
        else
        {
            Debug.LogWarning("[CardResolver] HelpLastPlace called with non-PointsCardDefinition.");
        }
    }

    // =====================================================================
    //  MEDIA
    // =====================================================================

    private void PlayCardMedia(BaseCardDefinition card)
    {
        if (card == null) return;

        if (!string.IsNullOrEmpty(card.audioTemplateName))
        {
            Debug.Log($"[CardResolver] Would play audio template: {card.audioTemplateName}");
        }

        if (!string.IsNullOrEmpty(card.videoTemplateName))
        {
            Debug.Log($"[CardResolver] Would play video template: {card.videoTemplateName}");
        }
    }
}
