using UnityEngine;

public class CardResolver
{
    private readonly GameManager game;

    private static readonly PassionColor[] AllPassions =
    {
        PassionColor.Yellow, PassionColor.Green, PassionColor.Blue,
        PassionColor.Purple, PassionColor.Pink, PassionColor.Orange
    };

    public CardResolver(GameManager gameManager)
    {
        game = gameManager;
    }

    public void ApplyCard(BaseCardDefinition card, PlayerData targetPlayer, PassionColor? passionForPoints = null)
    {
        if (card == null || targetPlayer == null) return;

        Debug.Log($"[CardResolver] Applying card: {card.title} to {targetPlayer.playerName}");

        switch (card.effectType)
        {
            case CardEffectType.GivePoints:
                if (card is PointsCardDefinition pointsCard)
                    ApplyPointsCard(pointsCard, targetPlayer, passionForPoints);
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
                break;
        }

        if (card is EventCardDefinition eventCard)
        {
            ApplyRelationshipEffects(eventCard, targetPlayer);
            ApplyItemDistribution(eventCard, targetPlayer);
            ApplyPartnerPointsBonus(eventCard, targetPlayer);
        }

        game.AddStatusEffect(targetPlayer, card);
        PlayCardMedia(card);
    }

    private void ApplyPointsCard(PointsCardDefinition card, PlayerData p, PassionColor? passionOverride)
    {
        if (card == null || p == null) return;

        if (card.useMultiPassionPoints && card.multiPassionPoints != null)
        {
            ApplyMultiPassionPoints(card, p);
        }

        if (card.useMainPassionPoints && card.mainPassionPointsDelta != 0)
        {
            AddPointsWithMultipliers(p, p.passion, card.mainPassionPointsDelta, card.title);
        }

        if (card.useSimplePoints && card.pointsDelta != 0)
        {
            PassionColor passionToUse = card.applyToMainPassion
                ? p.passion
                : passionOverride ?? card.simplePassion;

            AddPointsWithMultipliers(p, passionToUse, card.pointsDelta, card.title);
        }
    }

    private void ApplyMultiPassionPoints(PointsCardDefinition card, PlayerData p)
    {
        var mp = card.multiPassionPoints;
        if (mp.yellow != 0) AddPointsWithMultipliers(p, PassionColor.Yellow, mp.yellow, card.title);
        if (mp.green != 0) AddPointsWithMultipliers(p, PassionColor.Green, mp.green, card.title);
        if (mp.blue != 0) AddPointsWithMultipliers(p, PassionColor.Blue, mp.blue, card.title);
        if (mp.purple != 0) AddPointsWithMultipliers(p, PassionColor.Purple, mp.purple, card.title);
        if (mp.pink != 0) AddPointsWithMultipliers(p, PassionColor.Pink, mp.pink, card.title);
        if (mp.orange != 0) AddPointsWithMultipliers(p, PassionColor.Orange, mp.orange, card.title);
    }

    private void AddPointsWithMultipliers(PlayerData p, PassionColor passion, int basePoints, string cardTitle)
    {
        if (basePoints == 0) return;

        float factor = passion == p.passion ? 1.2f : 1f;
        factor *= game.GetItemScoreMultiplier(p, passion);

        int finalPoints = Mathf.RoundToInt(basePoints * factor);
        if (finalPoints == 0) return;

        game.AddPassionPoints(p, passion, finalPoints);

        Debug.Log($"[CardResolver] Card '{cardTitle}' gives {finalPoints} points in {passion} to {p.playerName} (base {basePoints}, factor {factor:F2}). Total: {p.GetTotalScore()}");
    }

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

        if (p.inventory.Remove(itemCard.id))
        {
            Debug.Log($"[CardResolver] {p.playerName} loses item: {itemCard.title} ({itemCard.id})");
        }
        else
        {
            Debug.Log($"[CardResolver] {p.playerName} does not have item: {itemCard.id}");
        }
    }

    private void StartMinigame(EventCardDefinition card, PlayerData p)
    {
        if (card == null || p == null || !card.triggersMinigame) return;

        Debug.Log($"[CardResolver] Starting minigame '{card.minigameId}' for {p.playerName}");
        game.StartMinigame(card.minigameId, p);
    }

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
        if (last == null) return;

        if (card is PointsCardDefinition pointsCard)
        {
            Debug.Log($"[CardResolver] Helping last place {last.playerName} using card '{card.title}'.");
            ApplyPointsCard(pointsCard, last, passionOverride);
        }
    }

    private void ApplyRelationshipEffects(EventCardDefinition card, PlayerData targetPlayer)
    {
        if (card == null) return;

        if (card.resetsAllRelationships)
        {
            Debug.Log($"[CardResolver] Card '{card.title}' resets ALL relationships.");
            game.ClearAllRelationships();
        }
        else if (card.resetsCurrentPlayerRelationship)
        {
            Debug.Log($"[CardResolver] Card '{card.title}' resets {targetPlayer.playerName}'s relationship.");
            game.ClearPartner(targetPlayer);
        }
    }

    private void ApplyItemDistribution(EventCardDefinition card, PlayerData currentPlayer)
    {
        if (card == null) return;

        if (card.givesItemToNextPlayer && !string.IsNullOrEmpty(card.nextPlayerItemId))
        {
            PlayerData nextPlayer = game.GetNextPlayerAfter(currentPlayer);
            if (nextPlayer != null)
            {
                GiveItemById(nextPlayer, card.nextPlayerItemId, card.title);
            }
        }

        if (card.givesItemToAllPlayers && !string.IsNullOrEmpty(card.allPlayersItemId))
        {
            var allPlayers = game.players;
            if (allPlayers != null)
            {
                for (int i = 0; i < allPlayers.Count; i++)
                {
                    var player = allPlayers[i];
                    if (player != null && !player.hasFinished)
                    {
                        GiveItemById(player, card.allPlayersItemId, card.title);
                    }
                }
            }
        }

        if (card.givesItemToAllPartneredPlayers && !string.IsNullOrEmpty(card.allPartneredPlayersItemId))
        {
            var allPlayers = game.players;
            if (allPlayers != null)
            {
                for (int i = 0; i < allPlayers.Count; i++)
                {
                    var player = allPlayers[i];
                    if (player != null && !player.hasFinished && player.HasPartner)
                    {
                        GiveItemById(player, card.allPartneredPlayersItemId, card.title);
                    }
                }
            }
        }

        if (card.givesItemToCurrentPlayerPartner && !string.IsNullOrEmpty(card.currentPlayerPartnerItemId))
        {
            if (currentPlayer.HasPartner)
            {
                PlayerData partner = game.GetPartner(currentPlayer);
                if (partner != null && !partner.hasFinished)
                {
                    GiveItemById(partner, card.currentPlayerPartnerItemId, card.title);
                }
            }
        }
    }

    private void GiveItemById(PlayerData player, string itemId, string sourceCardTitle)
    {
        if (player == null || string.IsNullOrEmpty(itemId)) return;

        var itemCard = game.cardManager?.GetItemById(itemId);
        if (itemCard == null)
        {
            Debug.LogWarning($"[CardResolver] Item '{itemId}' not found for distribution from '{sourceCardTitle}'.");
            return;
        }

        if (itemCard.uniquePerPlayer && player.inventory.Contains(itemId))
        {
            Debug.Log($"[CardResolver] {player.playerName} already has unique item '{itemId}'. Skipping.");
            return;
        }

        player.inventory.Add(itemId);
        game.AddStatusEffect(player, itemCard);
        Debug.Log($"[CardResolver] {player.playerName} receives item '{itemCard.title}' from event card '{sourceCardTitle}'.");
    }

    private void ApplyPartnerPointsBonus(EventCardDefinition card, PlayerData currentPlayer)
    {
        if (card == null) return;

        if (card.givesPointsToAllPartneredPlayers && card.partneredPlayersPointsDelta != 0)
        {
            var allPlayers = game.players;
            if (allPlayers != null)
            {
                for (int i = 0; i < allPlayers.Count; i++)
                {
                    var player = allPlayers[i];
                    if (player != null && !player.hasFinished && player.HasPartner)
                    {
                        AddPointsWithMultipliers(player, player.passion, card.partneredPlayersPointsDelta, card.title);
                        Debug.Log($"[CardResolver] {player.playerName} (partnered) receives {card.partneredPlayersPointsDelta} main passion points from '{card.title}'.");
                    }
                }
            }
        }

        if (card.givesPointsToCurrentPlayerAndPartner && card.currentPlayerAndPartnerPointsDelta != 0)
        {
            if (currentPlayer != null && currentPlayer.HasPartner)
            {
                AddPointsWithMultipliers(currentPlayer, currentPlayer.passion, card.currentPlayerAndPartnerPointsDelta, card.title);
                Debug.Log($"[CardResolver] {currentPlayer.playerName} receives {card.currentPlayerAndPartnerPointsDelta} main passion points from '{card.title}'.");

                PlayerData partner = game.GetPartner(currentPlayer);
                if (partner != null && !partner.hasFinished)
                {
                    AddPointsWithMultipliers(partner, partner.passion, card.currentPlayerAndPartnerPointsDelta, card.title);
                    Debug.Log($"[CardResolver] {partner.playerName} (partner) receives {card.currentPlayerAndPartnerPointsDelta} main passion points from '{card.title}'.");
                }
            }
        }
    }

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
