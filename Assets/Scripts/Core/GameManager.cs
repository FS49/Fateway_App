using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ButtonHotkey
{
    public string name;      // For your reference in the Inspector
    public Button button;    // The UI Button to trigger
    public KeyCode key;      // The key that should activate this button
}

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public DiceController diceController;
    public BoardManager boardManager;
    public CardManager cardManager;

    [Header("Players")]
    public List<PlayerData> players = new List<PlayerData>();
    [Tooltip("Index of the player whose turn it is.")]
    public int currentPlayerIndex = 0;

    [Header("Turn Settings")]
    [Tooltip("Default number of rolls per turn.")]
    public int defaultRollsPerTurn = 1;

    [Header("Debug / Last Roll Info")]
    public int lastBaseRoll;
    public int lastRollBonus;
    public int lastFinalRoll;

    [Header("UI References")]
    public ManualCardInputUI manualCardInputUI;

    [Header("Inventory UI")]
    public InventoryUI inventoryUI;

    [Header("Button Hotkeys")]
    [SerializeField] private List<ButtonHotkey> buttonHotkeys = new List<ButtonHotkey>();

    // When true, rolling and ending turns are blocked (e.g. manual card input open)
    [HideInInspector] public bool isManualInputOpen = false;
    [HideInInspector] public bool isInventoryOpen = false;
    [HideInInspector] public bool isCardPopupOpen = false;

    public bool IsInputLocked => isManualInputOpen || isInventoryOpen || isCardPopupOpen;

    private CardResolver cardResolver;

    [System.Serializable]
    private class ScheduledRisk
    {
        public EventCardDefinition riskCard;
        public PlayerData targetPlayer;
        public PassionColor? passionForPoints;
        public int remainingTurns;
    }

    private List<ScheduledRisk> scheduledRisks = new List<ScheduledRisk>();

    // who finished first (for 1.2x main passion bonus)
    private PlayerData firstFinisher = null;

    private void Awake()
    {
        cardResolver = new CardResolver(this);
    }

    private void Start()
    {
        if (players == null || players.Count == 0)
        {
            Debug.Log("[GameManager] No players defined in Inspector, creating 2 default players.");
            players = new List<PlayerData>
            {
                new PlayerData("Player 1", PassionColor.Yellow, Gender.Male),
                new PlayerData("Player 2", PassionColor.Blue, Gender.Female)
            };
        }

        StartTurnForCurrentPlayer();
        PrintPlayerStates();
        Debug.Log($"[GameManager] Starting game. Current player: {GetCurrentPlayer().playerName}");
    }

    private void Update()
    {
        // Generic hotkeys: press key -> invoke assigned button's OnClick
        if (buttonHotkeys != null)
        {
            foreach (var hotkey in buttonHotkeys)
            {
                if (hotkey == null || hotkey.button == null) continue;
                if (Input.GetKeyDown(hotkey.key))
                {
                    hotkey.button.onClick.Invoke();
                }
            }
        }
    }

    // ---------- Player helpers ----------

    public PlayerData GetCurrentPlayer()
    {
        if (players == null || players.Count == 0)
            return null;

        if (currentPlayerIndex < 0 || currentPlayerIndex >= players.Count)
            currentPlayerIndex = 0;

        return players[currentPlayerIndex];
    }

    public PlayerData GetLastPlacePlayer()
    {
        if (players == null || players.Count == 0)
            return null;

        PlayerData last = players[0];
        foreach (var p in players)
        {
            if (p.boardPosition < last.boardPosition)
            {
                last = p;
            }
        }
        return last;
    }

    private void StartTurnForCurrentPlayer()
    {
        var player = GetCurrentPlayer();
        if (player == null) return;

        // If this player already finished, they should never actually start a turn.
        if (player.hasFinished)
        {
            Debug.Log($"[GameManager] Tried to start turn for {player.playerName}, but they already finished. Advancing.");
            AdvanceToNextPlayer();
            return;
        }

        // NEW: handle turn-based lifetime & skip rounds for status effects
        if (HandleTurnStartStatusEffects(player))
        {
            // this player's turn was skipped, StartTurnForCurrentPlayer will be called again from AdvanceToNextPlayer
            return;
        }

        player.availableRolls = Mathf.Max(1, defaultRollsPerTurn);

        lastBaseRoll = 0;
        lastRollBonus = 0;
        lastFinalRoll = 0;

        Debug.Log($"[GameManager] Turn started for {player.playerName}. Rolls available: {player.availableRolls}");
    }

    // ---------- Turn / dice flow ----------

    public void TryRollForCurrentPlayer()
    {
        if (IsInputLocked)
        {
            Debug.Log("[GameManager] Cannot roll while a blocking UI (manual input / inventory / popup) is open.");
            return;
        }

        PlayerData player = GetCurrentPlayer();
        if (player == null)
        {
            Debug.LogError("[GameManager] No current player!");
            return;
        }

        // finished players cannot roll anymore
        if (player.hasFinished)
        {
            Debug.Log($"[GameManager] {player.playerName} has already finished and cannot roll.");
            return;
        }

        if (player.availableRolls <= 0)
        {
            Debug.Log($"[GameManager] {player.playerName} has no rolls left. End the turn to continue.");
            return;
        }

        HandleRollForCurrentPlayer();
    }

    private void HandleRollForCurrentPlayer()
    {
        PlayerData player = GetCurrentPlayer();
        if (player == null)
        {
            Debug.LogError("[GameManager] No current player!");
            return;
        }

        int baseRoll = diceController != null ? diceController.Roll() : Random.Range(1, 7);
        int bonus = GetDiceRollModifier(player);
        int finalRoll = baseRoll + bonus;
        if (finalRoll < 1) finalRoll = 1;

        // Store last roll info for UI
        lastBaseRoll = baseRoll;
        lastRollBonus = bonus;
        lastFinalRoll = finalRoll;

        Debug.Log($"[GameManager] {player.playerName} rolled {baseRoll} + bonus {bonus} = {finalRoll}.");

        // NEW: update status-effect durability based on baseRoll (without bonus)
        UpdateStatusEffectsOnRoll(player, baseRoll);

        player.boardPosition += finalRoll;
        Debug.Log($"[GameManager] {player.playerName} moves {finalRoll} steps to position {player.boardPosition}.");

        player.availableRolls--;
        Debug.Log($"[GameManager] {player.playerName} now has {player.availableRolls} roll(s) left this turn.");

        ApplyPerRollScoreBonuses(player);
        ProcessScheduledRisksForPlayer(player);
        ResolveField(player);

        // We still check game end after each roll
        CheckForGameEnd();
    }

    public void TryEndTurn()
    {
        if (IsInputLocked)
        {
            Debug.Log("[GameManager] Cannot end turn while a blocking UI (manual input / inventory / popup) is open.");
            return;
        }

        PlayerData player = GetCurrentPlayer();
        if (player == null)
        {
            Debug.LogError("[GameManager] No current player!");
            return;
        }

        if (!player.hasFinished && player.availableRolls > 0)
        {
            Debug.Log($"[GameManager] {player.playerName} still has {player.availableRolls} roll(s) left. " +
                      "They must use all rolls before ending their turn.");
            return;
        }

        // If game already ended, we might not need to advance, but we keep it simple for now
        if (!CheckForGameEnd())
        {
            AdvanceToNextPlayer();
        }
    }

    private int GetDiceRollModifier(PlayerData player)
    {
        if (player == null || cardManager == null || player.inventory == null)
            return 0;

        int totalBonus = 0;
        foreach (var itemId in player.inventory)
        {
            var itemDef = cardManager.GetItemById(itemId);
            if (itemDef == null) continue;
            totalBonus += itemDef.diceRollBonus;
        }

        return totalBonus;
    }

    /// <summary>
    /// Adds passion points to a player and automatically awards stars
    /// whenever the passion score crosses multiples of 100.
    /// </summary>
    public void AddPassionPoints(PlayerData player, PassionColor passion, int delta)
    {
        if (player == null || player.passionScores == null || delta == 0)
            return;

        int oldScore = player.passionScores.GetScore(passion);
        int oldStars = oldScore / 100;

        player.passionScores.AddScore(passion, delta);

        int newScore = player.passionScores.GetScore(passion);
        int newStars = newScore / 100;

        int gainedStars = newStars - oldStars;
        if (gainedStars > 0)
        {
            player.starCount += gainedStars;
            Debug.Log($"[GameManager] {player.playerName} gained {gainedStars} star(s). Total stars: {player.starCount}");
        }
    }

    public float GetItemScoreMultiplier(PlayerData player, PassionColor passion)
    {
        if (player == null || cardManager == null || player.inventory == null)
            return 1f;

        float factor = 1f;

        foreach (var itemId in player.inventory)
        {
            var itemDef = cardManager.GetItemById(itemId);
            if (itemDef == null || !itemDef.enableScoreMultipliers || itemDef.passionMultipliers == null)
                continue;

            float itemFactor = itemDef.passionMultipliers.GetMultiplier(passion);
            if (itemFactor <= 0f) itemFactor = 1f;
            factor *= itemFactor;
        }

        return factor;
    }

    // ---------- NEW per-roll logic using PassionAddConfig ----------

    private void ApplyPerRollScoreBonuses(PlayerData player)
    {
        if (player == null || cardManager == null || player.inventory == null)
            return;

        foreach (var itemId in player.inventory)
        {
            var itemDef = cardManager.GetItemById(itemId);
            if (itemDef == null || !itemDef.grantRollBonus || itemDef.perRollBonus == null)
                continue;

            // Apply per-roll bonus for each passion independently
            ApplyPerRollForPassion(player, itemDef, PassionColor.Yellow);
            ApplyPerRollForPassion(player, itemDef, PassionColor.Green);
            ApplyPerRollForPassion(player, itemDef, PassionColor.Blue);
            ApplyPerRollForPassion(player, itemDef, PassionColor.Purple);
            ApplyPerRollForPassion(player, itemDef, PassionColor.Pink);
            ApplyPerRollForPassion(player, itemDef, PassionColor.Orange);
        }
    }

    private void ApplyPerRollForPassion(PlayerData player, ItemCardDefinition itemDef, PassionColor passion)
    {
        // base flat amount from PassionAddConfig on the item
        int basePoints = itemDef.perRollBonus.GetAmount(passion);
        if (basePoints == 0)
            return;

        float factor = 1f;

        // 1) main passion 1.2x
        if (passion == player.passion)
            factor *= 1.2f;

        // 2) item-based score multipliers
        factor *= GetItemScoreMultiplier(player, passion);

        int finalPoints = Mathf.RoundToInt(basePoints * factor);
        if (finalPoints == 0)
            return;

        AddPassionPoints(player, passion, finalPoints);

        Debug.Log($"[GameManager] {player.playerName} gains {finalPoints} passive points in {passion} " +
                  $"from item '{itemDef.title}' (base {basePoints}, factor {factor:F2}). " +
                  $"Total score now: {player.GetTotalScore()}");
    }

    // ---------- Board / field resolution ----------

    private void ResolveField(PlayerData player)
    {
        if (boardManager == null)
        {
            Debug.LogWarning("[GameManager] No BoardManager assigned, treating all fields as Neutral.");
            return;
        }

        FieldType fieldType = boardManager.GetFieldTypeAt(player.boardPosition);
        BoardFieldDefinition fieldDef = boardManager.GetFieldDefinitionAt(player.boardPosition);

        Debug.Log($"[GameManager] {player.playerName} landed on a {fieldType} field (Index: {player.boardPosition}).");

        switch (fieldType)
        {
            case FieldType.Neutral:
                HandleNeutralField(player, fieldDef);
                break;

            case FieldType.Event:
                HandleEventField(player, fieldDef);
                break;

            case FieldType.ItemShop:
                HandleItemShopField(player);
                break;

            case FieldType.Minigame:
                HandleMinigameField(player);
                break;

            case FieldType.Crossroad:
                HandleCrossroadField(player);
                break;

            case FieldType.Finish:
                HandleFinishField(player);
                break;
        }

        if (fieldDef != null && !string.IsNullOrEmpty(fieldDef.description))
        {
            Debug.Log($"[GameManager] Field description: {fieldDef.description}");
        }

        if (fieldDef != null && fieldDef.requiresManualCardIdInput)
        {
            StartManualCardInput(player);
        }
    }

    private void HandleNeutralField(PlayerData player, BoardFieldDefinition fieldDef)
    {
        if (fieldDef != null && fieldDef.useFieldCard && fieldDef.fieldCardOverride != null)
        {
            var fieldCard = fieldDef.fieldCardOverride;

            if (fieldCard.triggersManualScan)
            {
                Debug.Log($"[GameManager] Neutral field {fieldDef.index} uses manual-scan FieldCard '{fieldCard.title}'. " +
                          $"Prompting {player.playerName} for a physical card ID.");
                StartManualCardInput(player);
            }
            else
            {
                PassionColor? passionForPoints = null;
                if (fieldDef.yieldsPassionScore)
                {
                    passionForPoints = fieldDef.passionReward;
                }

                Debug.Log($"[GameManager] Applying simple FieldCard '{fieldCard.title}' on Neutral field {fieldDef.index}.");
                cardResolver.ApplyCard(fieldCard, player, passionForPoints);
            }
        }
        else
        {
            Debug.Log("[GameManager] Neutral field - nothing happens (no FieldCard set).");
        }

        if (fieldDef != null && fieldDef.requiresManualCardIdInput)
        {
            StartManualCardInput(player);
        }
    }

    private void HandleEventField(PlayerData player, BoardFieldDefinition fieldDef)
    {
        if (cardManager == null)
        {
            Debug.LogWarning("[GameManager] No CardManager assigned, cannot draw event card.");
            return;
        }

        EventCardDefinition card = null;

        if (fieldDef != null && fieldDef.useSpecificEventCard && fieldDef.eventCardOverride != null)
        {
            card = fieldDef.eventCardOverride;
            Debug.Log($"[GameManager] Using specific event card '{card.title}' for field {fieldDef.index}.");
        }
        else
        {
            card = cardManager.DrawRandomEventCard();
        }

        if (card == null) return;

        PassionColor? passionForPoints = null;
        if (fieldDef != null && fieldDef.yieldsPassionScore)
        {
            passionForPoints = fieldDef.passionReward;
        }

        cardResolver.ApplyCard(card, player, passionForPoints);
    }

    private void HandleItemShopField(PlayerData player)
    {
        if (cardManager == null)
        {
            Debug.LogWarning("[GameManager] No CardManager assigned, cannot draw item card.");
            return;
        }

        BoardFieldDefinition fieldDef = boardManager.GetFieldDefinitionAt(player.boardPosition);

        ItemCardDefinition card = null;

        if (fieldDef != null && fieldDef.useSpecificItemCard && fieldDef.itemCardOverride != null)
        {
            card = fieldDef.itemCardOverride;
            Debug.Log($"[GameManager] Using specific item card '{card.title}' for field {fieldDef.index}.");
        }
        else
        {
            card = cardManager.DrawRandomItemCard();
        }

        if (card == null) return;

        cardResolver.ApplyCard(card, player);
    }

    private void HandleMinigameField(PlayerData player)
    {
        Debug.Log($"[GameManager] Minigame field reached by {player.playerName}! (placeholder)");
    }

    private void HandleCrossroadField(PlayerData player)
    {
        Debug.Log($"[GameManager] Crossroad! {player.playerName} must choose a path. (placeholder)");

        bool riskyPath = Random.value > 0.5f;
        Debug.Log($"[GameManager] Auto-choice (prototype): {(riskyPath ? "Risky" : "Safe")} path.");
    }

    private void HandleFinishField(PlayerData player)
    {
        Debug.Log($"[GameManager] {player.playerName} reached the finish!");

        // Mark as finished and clamp to finish index
        if (!player.hasFinished)
        {
            player.hasFinished = true;
            player.availableRolls = 0; // they’re done rolling for the rest of the game

            int finishIndex = boardManager != null ? boardManager.totalFields - 1 : player.boardPosition;
            if (player.boardPosition > finishIndex)
                player.boardPosition = finishIndex;

            // FIRST finisher logic
            if (firstFinisher == null)
            {
                firstFinisher = player;

                // APPLY 1.2x BONUS ON MAIN PASSION IMMEDIATELY
                int mainScoreBefore = player.passionScores.GetScore(player.passion);
                int bonus = Mathf.RoundToInt(mainScoreBefore * 0.2f);

                if (bonus > 0)
                {
                    AddPassionPoints(player, player.passion, bonus);
                    int mainScoreAfter = player.passionScores.GetScore(player.passion);

                    Debug.Log($"[GameManager] {player.playerName} is FIRST to finish and gets +{bonus} " +
                              $"points in {player.passion} (1.2x main passion). " +
                              $"Main passion score: {mainScoreBefore} → {mainScoreAfter}");
                }
                else
                {
                    Debug.Log($"[GameManager] {player.playerName} is FIRST to finish, but main passion score was {mainScoreBefore}, so no bonus applied yet.");
                }
            }

            // NEW: redeem items & clear all effects on finish
            ClearEffectsAndRedeemItems(player);
        }
    }

    // ---------- Manual card input ----------

    public void StartManualCardInput(PlayerData player)
    {
        Debug.Log($"[GameManager] Manual card input required for {player.playerName}.");

        isManualInputOpen = true;

        if (manualCardInputUI != null)
        {
            manualCardInputUI.ShowForPlayer(player);
        }
        else
        {
            Debug.LogWarning("[GameManager] No ManualCardInputUI assigned, cannot show input panel.");
        }
    }

    public void AddStatusEffect(PlayerData player, BaseCardDefinition card)
    {
        if (player == null || card == null) return;
        if (!card.trackAsStatusEffect) return;

        // 1) Track ID for UI (StatusEffects panel)
        if (!player.statusEffectCards.Contains(card.id))
        {
            player.statusEffectCards.Add(card.id);
            Debug.Log($"[GameManager] Added status effect '{card.title}' ({card.id}) to {player.playerName}.");
        }

        // 2) Create or refresh runtime state (durability counters)
        var existing = player.activeStatusEffects.Find(e => e.cardId == card.id);
        if (existing == null)
        {
            var state = new ActiveStatusEffectState
            {
                cardId = card.id,
                remainingRolls = card.statusLifetime.useRollLifetime ? card.statusLifetime.maxRolls : 0,
                remainingTurns = card.statusLifetime.useTurnLifetime ? card.statusLifetime.maxTurns : 0,
                remainingSkipRounds = card.statusLifetime.skipRounds
            };
            player.activeStatusEffects.Add(state);
        }
        else
        {
            // If effect is applied again, you can choose to refresh durations.
            existing.remainingRolls = card.statusLifetime.useRollLifetime ? card.statusLifetime.maxRolls : existing.remainingRolls;
            existing.remainingTurns = card.statusLifetime.useTurnLifetime ? card.statusLifetime.maxTurns : existing.remainingTurns;
            existing.remainingSkipRounds = card.statusLifetime.skipRounds;
        }
    }

    public void RemoveStatusEffect(PlayerData player, string cardId)
    {
        if (player == null || string.IsNullOrEmpty(cardId))
            return;

        // 1) Remove from status-effect lists
        if (player.statusEffectCards.Contains(cardId))
            player.statusEffectCards.Remove(cardId);

        if (player.activeStatusEffects != null)
        {
            player.activeStatusEffects.RemoveAll(e => e.cardId == cardId);
        }

        // 2) If this card is an item, also remove it from inventory
        if (cardManager != null)
        {
            var def = cardManager.GetCardById(cardId);
            if (def is ItemCardDefinition)
            {
                if (player.inventory.Contains(cardId))
                {
                    player.inventory.Remove(cardId);
                    Debug.Log($"[GameManager] Removed expired item '{cardId}' from {player.playerName}'s inventory.");
                }
            }
        }

        Debug.Log($"[GameManager] Removed status effect '{cardId}' from {player.playerName}.");
    }

    // Called by ManualCardInputUI
    public void OnManualCardInputClosed()
    {
        isManualInputOpen = false;
    }

    // Inventory
    public void OnInventoryOpened()
    {
        isInventoryOpen = true;
    }

    public void OnInventoryClosed()
    {
        OnCardPopupClosed();
        isInventoryOpen = false;
    }

    public void OpenInventoryView()
    {
        if (inventoryUI != null)
        {
            inventoryUI.Show();
        }
        else
        {
            Debug.LogWarning("[GameManager] No InventoryUI assigned.");
        }
    }

    // Card description popup
    public void OnCardPopupOpened()
    {
        isCardPopupOpen = true;
    }

    public void OnCardPopupClosed()
    {
        isCardPopupOpen = false;
    }

    public void ApplyCardFromIdForCurrentPlayer(string cardId, PassionColor? passionForPoints = null)
    {
        var player = GetCurrentPlayer();
        ApplyCardFromId(cardId, player, passionForPoints);
    }

    public void ApplyCardFromId(string cardId, PlayerData targetPlayer, PassionColor? passionForPoints = null)
    {
        if (cardManager == null)
        {
            Debug.LogWarning("[GameManager] Cannot apply card by ID; CardManager is null.");
            return;
        }

        BaseCardDefinition card = cardManager.GetCardById(cardId);
        if (card == null)
        {
            Debug.LogWarning($"[GameManager] No card found with id '{cardId}'.");
            return;
        }

        Debug.Log($"[GameManager] Applying card '{card.title}' (id: {card.id}) to {targetPlayer.playerName} by manual input.");
        cardResolver.ApplyCard(card, targetPlayer, passionForPoints);
    }

    // ---------- Game end & turn rotation ----------

    // Final score is now just the total score; first-finisher bonus is already baked in.
    private int GetFinalScoreForPlayer(PlayerData p)
    {
        if (p == null) return 0;
        return p.GetTotalScore();
    }

    private bool CheckForGameEnd()
    {
        if (boardManager == null || players == null || players.Count == 0)
            return false;

        // Game ends ONLY when all players have finished
        bool allFinished = true;
        foreach (var p in players)
        {
            if (!p.hasFinished)
            {
                allFinished = false;
                break;
            }
        }

        if (allFinished)
        {
            PlayerData winner = null;
            int bestScore = int.MinValue;

            foreach (var p in players)
            {
                // Only main passion score counts for win:
                int finalScore = p.passionScores.GetScore(p.passion);

                if (winner == null || finalScore > bestScore)
                {
                    winner = p;
                    bestScore = finalScore;
                }
            }

            if (winner != null)
            {
                int baseScore = winner.GetTotalScore();
                bool hasBonus = (winner == firstFinisher);

                Debug.Log(
                    $"[GameManager] GAME OVER. Winner: {winner.playerName} " +
                    $"with FINAL score {bestScore}" +
                    (hasBonus ? ", includes first-finish 1.2x main passion bonus)." : ").")
                );
            }
            else
            {
                Debug.Log("[GameManager] GAME OVER. No winner found (something went wrong?).");
            }

            return true;
        }

        return false;
    }

    private void AdvanceToNextPlayer()
    {
        if (players == null || players.Count == 0)
            return;

        int startIndex = currentPlayerIndex;
        int safeGuard = 0;

        do
        {
            currentPlayerIndex++;
            if (currentPlayerIndex >= players.Count)
                currentPlayerIndex = 0;

            safeGuard++;
            if (safeGuard > players.Count + 1)
            {
                Debug.LogWarning("[GameManager] AdvanceToNextPlayer safeguard triggered.");
                break;
            }

        } while (players[currentPlayerIndex].hasFinished && currentPlayerIndex != startIndex);

        StartTurnForCurrentPlayer();

        Debug.Log($"[GameManager] Next player: {GetCurrentPlayer().playerName}");
        PrintPlayerStates();
    }

    private void PrintPlayerStates()
    {
        Debug.Log("[GameManager] Player states:");
        foreach (var p in players)
        {
            Debug.Log(
                $"{p.playerName} ({p.passion})\n" +
                $"  Position: {p.boardPosition}\n" +
                $"  Finished: {p.hasFinished}\n" +
                $"  Scores:\n" +
                $"    Yellow: {p.passionScores.yellow}\n" +
                $"    Green:  {p.passionScores.green}\n" +
                $"    Blue:   {p.passionScores.blue}\n" +
                $"    Purple: {p.passionScores.purple}\n" +
                $"    Pink:   {p.passionScores.pink}\n" +
                $"    Orange: {p.passionScores.orange}\n" +
                $"  Stars: {p.starCount}\n" +
                $"  Available Rolls: {p.availableRolls}\n" +
                $"  Total Score = {p.GetTotalScore()}"
            );
        }
    }

    // ---------- Minigame & risk scheduling ----------

    public void StartMinigame(string minigameId, PlayerData player)
    {
        Debug.Log($"[GameManager] (Stub) Would load minigame '{minigameId}' for {player.playerName}");
    }

    public void ScheduleRiskOutcome(EventCardDefinition card, PlayerData player, PassionColor? passionForPoints = null)
    {
        if (card == null || player == null) return;

        var risk = new ScheduledRisk
        {
            riskCard = card,
            targetPlayer = player,
            passionForPoints = passionForPoints,
            remainingTurns = Mathf.Max(1, card.riskDurationTurns)
        };

        scheduledRisks.Add(risk);
    }

    private void ProcessScheduledRisksForPlayer(PlayerData playerJustMoved)
    {
        if (scheduledRisks.Count == 0) return;

        for (int i = scheduledRisks.Count - 1; i >= 0; i--)
        {
            var risk = scheduledRisks[i];
            if (risk.targetPlayer != playerJustMoved)
                continue;

            risk.remainingTurns--;

            float probability;
            if (risk.remainingTurns >= 2) probability = 0.5f;
            else if (risk.remainingTurns == 1) probability = 0.7f;
            else probability = 0.9f;

            if (Random.value <= probability || risk.remainingTurns <= 0)
            {
                Debug.Log($"[GameManager] Risk outcome triggered for {playerJustMoved.playerName}!");
                cardResolver.ApplyCard(risk.riskCard, playerJustMoved, risk.passionForPoints);
                scheduledRisks.RemoveAt(i);
            }
        }
    }

    // ---------- NEW helpers for status-effect lifetime & redeem ----------

    private void UpdateStatusEffectsOnRoll(PlayerData player, int baseRoll)
    {
        if (player == null || cardManager == null || player.activeStatusEffects == null)
            return;

        bool changed = false;

        for (int i = player.activeStatusEffects.Count - 1; i >= 0; i--)
        {
            var state = player.activeStatusEffects[i];
            var card = cardManager.GetCardById(state.cardId);
            if (card == null)
            {
                player.activeStatusEffects.RemoveAt(i);
                changed = true;
                continue;
            }

            var cfg = card.statusLifetime;

            // 1) roll-based lifetime
            if (cfg.useRollLifetime && state.remainingRolls > 0)
            {
                state.remainingRolls--;
                if (state.remainingRolls <= 0)
                {
                    Debug.Log($"[GameManager] Status effect '{card.title}' expired for {player.playerName} (roll lifetime).");
                    RemoveStatusEffect(player, card.id);
                    changed = true;
                    continue;
                }
            }

            // 2) break on specific dice value (baseRoll only)
            if (cfg.useRollValueBreak && cfg.breakOnRollValues != null && cfg.breakOnRollValues.Length > 0)
            {
                for (int r = 0; r < cfg.breakOnRollValues.Length; r++)
                {
                    if (cfg.breakOnRollValues[r] == baseRoll)
                    {
                        Debug.Log($"[GameManager] Status effect '{card.title}' removed for {player.playerName} " +
                                  $"because they rolled {baseRoll}.");
                        RemoveStatusEffect(player, card.id);
                        changed = true;
                        break;
                    }
                }
            }
        }

        if (changed)
        {
            // TODO: refresh inventory / status UI if needed
        }
    }

    /// <summary>
    /// Handles turn-based lifetime and skip-round mechanics for status effects.
    /// Returns true if the player’s turn should be skipped (we already advanced).
    /// </summary>
    private bool HandleTurnStartStatusEffects(PlayerData player)
    {
        if (player == null || cardManager == null || player.activeStatusEffects == null)
            return false;

        // 1) Decrement turn-based lifetime
        for (int i = player.activeStatusEffects.Count - 1; i >= 0; i--)
        {
            var state = player.activeStatusEffects[i];
            var card = cardManager.GetCardById(state.cardId);
            if (card == null)
            {
                player.activeStatusEffects.RemoveAt(i);
                continue;
            }

            var cfg = card.statusLifetime;

            if (cfg.useTurnLifetime && state.remainingTurns > 0)
            {
                state.remainingTurns--;
                if (state.remainingTurns <= 0)
                {
                    Debug.Log($"[GameManager] Status effect '{card.title}' expired for {player.playerName} (turn lifetime).");
                    RemoveStatusEffect(player, card.id);
                }
            }
        }

        // 2) Skip rounds based on remainingSkipRounds
        bool mustSkip = false;
        for (int i = player.activeStatusEffects.Count - 1; i >= 0; i--)
        {
            var state = player.activeStatusEffects[i];
            var card = cardManager.GetCardById(state.cardId);
            if (card == null)
            {
                player.activeStatusEffects.RemoveAt(i);
                continue;
            }

            if (state.remainingSkipRounds > 0)
            {
                mustSkip = true;
                state.remainingSkipRounds--;

                Debug.Log($"[GameManager] {player.playerName} must skip a turn due to '{card.title}'. " +
                          $"Skip rounds left for this effect: {state.remainingSkipRounds}");

                var cfg = card.statusLifetime;
                bool noRollLife = !cfg.useRollLifetime || state.remainingRolls <= 0;
                bool noTurnLife = !cfg.useTurnLifetime || state.remainingTurns <= 0;
                bool noMoreSkip = state.remainingSkipRounds <= 0;

                if (noRollLife && noTurnLife && noMoreSkip)
                {
                    RemoveStatusEffect(player, card.id);
                }
            }
        }

        if (mustSkip)
        {
            Debug.Log($"[GameManager] {player.playerName} skips this entire turn due to status effects.");
            AdvanceToNextPlayer();
            return true;
        }

        return false;
    }

    private void ClearEffectsAndRedeemItems(PlayerData player)
    {
        if (player == null || cardManager == null)
            return;

        // 1) Redeem items
        foreach (var itemId in player.inventory)
        {
            var item = cardManager.GetItemById(itemId) as ItemCardDefinition;
            if (item == null) continue;

            if (item.isRedeemable && item.redeemScores != null)
            {
                RedeemItemScores(player, item);
            }
        }

        // 2) Clear all items and status effects
        player.inventory.Clear();
        player.statusEffectCards.Clear();
        if (player.activeStatusEffects != null)
            player.activeStatusEffects.Clear();

        Debug.Log($"[GameManager] Cleared all items and status effects for {player.playerName} at finish.");
    }

    private void RedeemItemScores(PlayerData player, ItemCardDefinition item)
    {
        if (player == null || item == null || item.redeemScores == null)
            return;

        RedeemPassion(player, item, PassionColor.Yellow, item.redeemScores.yellow);
        RedeemPassion(player, item, PassionColor.Green,  item.redeemScores.green);
        RedeemPassion(player, item, PassionColor.Blue,   item.redeemScores.blue);
        RedeemPassion(player, item, PassionColor.Purple, item.redeemScores.purple);
        RedeemPassion(player, item, PassionColor.Pink,   item.redeemScores.pink);
        RedeemPassion(player, item, PassionColor.Orange, item.redeemScores.orange);
    }

    private void RedeemPassion(PlayerData player, ItemCardDefinition item, PassionColor passion, int delta)
    {
        if (delta == 0)
            return;

        AddPassionPoints(player, passion, delta);

        Debug.Log($"[GameManager] Redeemed item '{item.title}' for {delta} points in {passion} for {player.playerName}. " +
                  $"Total score now: {player.GetTotalScore()}");
    }
}
