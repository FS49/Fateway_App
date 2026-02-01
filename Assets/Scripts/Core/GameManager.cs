using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ButtonHotkey
{
    public string name;
    public Button button;
    public KeyCode key;
}

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public DiceController diceController;
    public BoardManager boardManager;
    public CardManager cardManager;

    [Header("Players")]
    public List<PlayerData> players = new List<PlayerData>();
    public int currentPlayerIndex;

    [Header("Turn Settings")]
    public int defaultRollsPerTurn = 1;

    [Header("Debug / Last Roll Info")]
    public int lastBaseRoll;
    public int lastRollBonus;
    public int lastFinalRoll;

    [Header("UI References")]
    public ManualCardInputUI manualCardInputUI;
    public InventoryUI inventoryUI;
    public CrossroadChoiceUI crossroadChoiceUI;
    public GameFeedbackUI gameFeedbackUI;
    public LastCrossroadsPopupUI lastCrossroadsPopupUI;
    public ScreenManager screenManager;

    [Header("Button Hotkeys")]
    [SerializeField] private List<ButtonHotkey> buttonHotkeys = new List<ButtonHotkey>();

    [HideInInspector] public bool isManualInputOpen;
    [HideInInspector] public bool isInventoryOpen;
    [HideInInspector] public bool isCardPopupOpen;
    [HideInInspector] public bool isPartnerPanelOpen;
    [HideInInspector] public bool isCrossroadChoiceOpen;
    [HideInInspector] public bool isLastCrossroadsPopupOpen;
    [HideInInspector] public bool isSetupOpen;

    public bool IsInputLocked => isManualInputOpen || isInventoryOpen || isCardPopupOpen || isPartnerPanelOpen || isCrossroadChoiceOpen || isLastCrossroadsPopupOpen || isSetupOpen;

    private CardResolver cardResolver;
    private PlayerData firstFinisher;
    private readonly List<ScheduledRisk> scheduledRisks = new List<ScheduledRisk>();

    private static readonly PassionColor[] AllPassions =
    {
        PassionColor.Yellow, PassionColor.Green, PassionColor.Blue,
        PassionColor.Purple, PassionColor.Pink, PassionColor.Orange
    };

    [System.Serializable]
    private class ScheduledRisk
    {
        public EventCardDefinition riskCard;
        public PlayerData targetPlayer;
        public PassionColor? passionForPoints;
        public int remainingTurns;
    }

    private void Awake()
    {
        cardResolver = new CardResolver(this);
    }

    private void Start()
    {
        if (gameFeedbackUI == null)
            gameFeedbackUI = FindObjectOfType<GameFeedbackUI>(true);

        if (players == null || players.Count == 0)
        {
            Debug.Log("[GameManager] No players defined in Inspector, creating 2 default players.");
            players = new List<PlayerData>
            {
                new PlayerData("Player 1", PassionColor.Yellow, Gender.Male),
                new PlayerData("Player 2", PassionColor.Blue, Gender.Female)
            };
        }

        ClearAllRelationships();
        ResetAllRiskFlags();

        StartTurnForCurrentPlayer();
        PrintPlayerStates();
        Debug.Log($"[GameManager] Starting game. Current player: {GetCurrentPlayer().playerName}");
    }

    public void ResetGame()
    {
        Debug.Log("[GameManager] Resetting game...");

        if (players != null)
        {
            for (int i = 0; i < players.Count; i++)
            {
                players[i].Reset();
            }
        }

        currentPlayerIndex = 0;

        isManualInputOpen = false;
        isInventoryOpen = false;
        isCardPopupOpen = false;
        isPartnerPanelOpen = false;
        isCrossroadChoiceOpen = false;
        isLastCrossroadsPopupOpen = false;

        ClearAllRelationships();

        StartTurnForCurrentPlayer();
        PrintPlayerStates();

        Debug.Log("[GameManager] Game reset complete.");
    }

    public void ApplySetupPlayersAndRestart(List<PlayerData> newPlayers)
{
    if (newPlayers == null || newPlayers.Count < 1) return;
    players = newPlayers;
    currentPlayerIndex = 0;
    ResetGame();
}

    public void ResetAllRiskFlags()
    {
        if (players == null) return;

        for (int i = 0; i < players.Count; i++)
        {
            players[i].riskFlag = false;
            players[i].pendingMovement = 0;
        }
    }

    private void Update()
    {
        if (buttonHotkeys == null) return;

        for (int i = 0; i < buttonHotkeys.Count; i++)
        {
            var hotkey = buttonHotkeys[i];
            if (hotkey?.button != null && Input.GetKeyDown(hotkey.key))
            {
                hotkey.button.onClick.Invoke();
            }
        }
    }

    public PlayerData GetCurrentPlayer()
    {
        if (players == null || players.Count == 0)
            return null;

        if (currentPlayerIndex < 0 || currentPlayerIndex >= players.Count)
            currentPlayerIndex = 0;

        return players[currentPlayerIndex];
    }

    public List<PlayerData> GetPlayerRankings()
    {
        if (players == null || players.Count == 0)
            return new List<PlayerData>();

        var sorted = new List<PlayerData>(players);
        sorted.Sort((a, b) => b.GetTotalScore().CompareTo(a.GetTotalScore()));
        return sorted;
    }

    public PlayerData GetWinner()
    {
        var rankings = GetPlayerRankings();
        return rankings.Count > 0 ? rankings[0] : null;
    }

    public PlayerData GetLastPlacePlayer()
    {
        if (players == null || players.Count == 0)
            return null;

        PlayerData last = players[0];
        for (int i = 1; i < players.Count; i++)
        {
            if (players[i].boardPosition < last.boardPosition)
                last = players[i];
        }
        return last;
    }

    private void StartTurnForCurrentPlayer()
    {
        var player = GetCurrentPlayer();
        if (player == null) return;

        if (player.hasFinished)
        {
            Debug.Log($"[GameManager] {player.playerName} already finished. Advancing.");
            AdvanceToNextPlayer();
            return;
        }

        if (HandleTurnStartStatusEffects(player))
            return;

        player.availableRolls = Mathf.Max(1, defaultRollsPerTurn);
        lastBaseRoll = 0;
        lastRollBonus = 0;
        lastFinalRoll = 0;

        Debug.Log($"[GameManager] Turn started for {player.playerName}. Rolls available: {player.availableRolls}");
    }

    public void TryRollForCurrentPlayer()
    {
        if (IsInputLocked)
        {
            Debug.Log("[GameManager] Cannot roll while a blocking UI is open.");
            return;
        }

        PlayerData player = GetCurrentPlayer();
        if (player == null) return;

        if (player.hasFinished)
        {
            Debug.Log($"[GameManager] {player.playerName} has already finished.");
            return;
        }

        if (player.availableRolls <= 0)
        {
            Debug.Log($"[GameManager] {player.playerName} has no rolls left.");
            return;
        }

        HandleRollForCurrentPlayer();
    }

    private void HandleRollForCurrentPlayer()
    {
        PlayerData player = GetCurrentPlayer();
        if (player == null) return;

        int baseRoll = diceController != null ? diceController.Roll() : Random.Range(1, 7);
        int diceBonus = GetDiceRollModifier(player);
        int oddEvenBonus = GetOddEvenRollModifier(player, baseRoll);
        int totalBonus = diceBonus + oddEvenBonus;
        int finalRoll = Mathf.Max(0, baseRoll + totalBonus);

        lastBaseRoll = baseRoll;
        lastRollBonus = totalBonus;
        lastFinalRoll = finalRoll;

        string oddEvenText = oddEvenBonus != 0 ? $" ({(baseRoll % 2 != 0 ? "odd" : "even")} bonus: {oddEvenBonus:+0;-0;0})" : "";
        Debug.Log($"[GameManager] {player.playerName} rolled {baseRoll} + bonus {totalBonus} = {finalRoll}.{oddEvenText}");

        if (baseRoll == 1 && player.HasPartner)
        {
            Debug.Log($"[GameManager] {player.playerName} rolled a 1! Relationship ends.");
            ClearPartner(player);
        }

        ApplyPerRollScoreBonuses(player);
        UpdateStatusEffectsOnRoll(player, baseRoll);

        player.availableRolls--;

        if (finalRoll > 0)
        {
            MovePlayerWithCrossroadCheck(player, finalRoll);
        }
        else
        {
            Debug.Log($"[GameManager] {player.playerName} rolled 0 total - no movement.");
            ProcessScheduledRisksForPlayer(player);
            CheckForGameEnd();
        }
    }

    private void MovePlayerWithCrossroadCheck(PlayerData player, int movement)
    {
        if (player == null || movement <= 0) return;

        int startPosition = player.boardPosition;
        int targetPosition = startPosition + movement;

        Debug.Log($"[GameManager] MovePlayerWithCrossroadCheck: {player.playerName} moving from {startPosition} to {targetPosition} (movement: {movement})");

        if (boardManager != null)
        {
            var stopField = boardManager.GetFirstStopFieldInPath(startPosition, targetPosition);

            if (stopField.type == BoardManager.StopFieldType.Crossroad)
            {
                int remainingMovement = targetPosition - stopField.index;
                player.boardPosition = stopField.index;
                player.pendingMovement = remainingMovement;

                Debug.Log($"[GameManager] {player.playerName} stopped at crossroad (index {stopField.index}). Remaining movement: {remainingMovement}");

                player.riskFlag = false;

                ShowCrossroadChoice(player, remainingMovement);
                return;
            }

            if (stopField.type == BoardManager.StopFieldType.LastCrossroads)
            {
                int remainingMovement = targetPosition - stopField.index;
                player.boardPosition = stopField.index;
                player.pendingMovement = remainingMovement;

                Debug.Log($"[GameManager] {player.playerName} stopped at Last Crossroads Field (index {stopField.index}). Remaining movement: {remainingMovement}");

                HandleLastCrossroadsFieldWithContinuation(player);
                return;
            }
        }
        else
        {
            Debug.LogWarning("[GameManager] BoardManager is null! Cannot check for stop fields.");
        }

        player.boardPosition = targetPosition;
        Debug.Log($"[GameManager] {player.playerName} moves to position {player.boardPosition}.");

        ProcessScheduledRisksForPlayer(player);
        ResolveField(player);
        CheckForGameEnd();
    }

    private void ShowCrossroadChoice(PlayerData player, int remainingMovement)
    {
        Debug.Log($"[GameManager] ShowCrossroadChoice called for {player?.playerName}, remaining: {remainingMovement}");

        if (crossroadChoiceUI != null)
        {
            Debug.Log("[GameManager] CrossroadChoiceUI reference exists, calling Show...");
            crossroadChoiceUI.Show(player, remainingMovement, (choseRisk) =>
            {
                OnCrossroadChoiceMade(player, choseRisk, remainingMovement);
            });
        }
        else
        {
            Debug.LogWarning("[GameManager] No CrossroadChoiceUI assigned! Auto-selecting Safe route.");
            OnCrossroadChoiceMade(player, false, remainingMovement);
        }
    }

    private void OnCrossroadChoiceMade(PlayerData player, bool choseRisk, int remainingMovement)
    {
        player.riskFlag = choseRisk;
        player.pendingMovement = 0;

        Debug.Log($"[GameManager] {player.playerName} chose {(choseRisk ? "RISK" : "SAFE")} route.");

        ResolveField(player);

        if (remainingMovement > 0)
        {
            Debug.Log($"[GameManager] Continuing movement with {remainingMovement} remaining steps.");
            MovePlayerWithCrossroadCheck(player, remainingMovement);
        }
        else
        {
            ProcessScheduledRisksForPlayer(player);
            CheckForGameEnd();
        }
    }

    public void OnCrossroadChoiceOpened()
    {
        isCrossroadChoiceOpen = true;
    }

    public void OnCrossroadChoiceClosed()
    {
        isCrossroadChoiceOpen = false;
    }

    public void TryEndTurn()
    {
        if (IsInputLocked)
        {
            Debug.Log("[GameManager] Cannot end turn while a blocking UI is open.");
            return;
        }

        PlayerData player = GetCurrentPlayer();
        if (player == null) return;

        if (!player.hasFinished && player.availableRolls > 0)
        {
            Debug.Log($"[GameManager] {player.playerName} still has {player.availableRolls} roll(s) left.");
            return;
        }

        UpdateTurnLifetimeOnEndTurn(player);

        if (!CheckForGameEnd())
        {
            PlayerData endingPlayer = player;
            PlayerData nextPlayer = GetNextPlayer();

            AdvanceToNextPlayer();

            if (gameFeedbackUI != null && nextPlayer != null)
            {
                gameFeedbackUI.ShowTurnEndedFeedback(endingPlayer.playerName, endingPlayer.passion, nextPlayer.playerName, nextPlayer.passion);
            }
        }
    }

    private PlayerData GetNextPlayer()
    {
        if (players == null || players.Count == 0) return null;

        int nextIndex = currentPlayerIndex;
        int safeGuard = 0;

        do
        {
            nextIndex = (nextIndex + 1) % players.Count;
            safeGuard++;
            if (safeGuard > players.Count + 1) break;
        } while (players[nextIndex].hasFinished && nextIndex != currentPlayerIndex);

        return players[nextIndex];
    }

    public PlayerData GetNextPlayerAfter(PlayerData player)
    {
        if (players == null || players.Count == 0 || player == null) return null;

        int playerIndex = players.IndexOf(player);
        if (playerIndex < 0) return null;

        int nextIndex = playerIndex;
        int safeGuard = 0;

        do
        {
            nextIndex = (nextIndex + 1) % players.Count;
            safeGuard++;
            if (safeGuard > players.Count + 1) break;
        } while (players[nextIndex].hasFinished && nextIndex != playerIndex);

        if (nextIndex == playerIndex) return null;

        return players[nextIndex];
    }

    private void UpdateTurnLifetimeOnEndTurn(PlayerData player)
    {
        if (player?.activeStatusEffects == null || cardManager == null) return;

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
    }

    private int GetDiceRollModifier(PlayerData player)
    {
        if (player?.inventory == null || cardManager == null)
            return 0;

        int totalBonus = 0;
        for (int i = 0; i < player.inventory.Count; i++)
        {
            var itemDef = cardManager.GetItemById(player.inventory[i]);
            if (itemDef != null)
                totalBonus += itemDef.diceRollBonus;
        }
        return totalBonus;
    }

    private int GetOddEvenRollModifier(PlayerData player, int baseRoll)
    {
        if (player?.inventory == null || cardManager == null)
            return 0;

        bool isOdd = baseRoll % 2 != 0;
        int totalBonus = 0;

        for (int i = 0; i < player.inventory.Count; i++)
        {
            var itemDef = cardManager.GetItemById(player.inventory[i]);
            if (itemDef != null)
            {
                totalBonus += isOdd ? itemDef.oddRollBonus : itemDef.evenRollBonus;
            }
        }

        return totalBonus;
    }

    public void AddPassionPoints(PlayerData player, PassionColor passion, int delta)
    {
        if (player?.passionScores == null || delta == 0)
            return;

        int oldStars = player.passionScores.GetScore(passion) / 100;
        player.passionScores.AddScore(passion, delta);
        int newStars = player.passionScores.GetScore(passion) / 100;

        int gainedStars = newStars - oldStars;
        if (gainedStars > 0)
        {
            player.starCount += gainedStars;
            Debug.Log($"[GameManager] {player.playerName} gained {gainedStars} star(s). Total stars: {player.starCount}");
        }

        if (gameFeedbackUI != null)
            gameFeedbackUI.ShowPointsFeedback(player.playerName, player.passion, delta);
    }

    public float GetItemScoreMultiplier(PlayerData player, PassionColor passion)
    {
        if (player?.inventory == null || cardManager == null)
            return 1f;

        float factor = 1f;
        for (int i = 0; i < player.inventory.Count; i++)
        {
            var itemDef = cardManager.GetItemById(player.inventory[i]);
            if (itemDef == null || !itemDef.enableScoreMultipliers || itemDef.passionMultipliers == null)
                continue;

            float itemFactor = itemDef.passionMultipliers.GetMultiplier(passion);
            if (itemFactor > 0f)
                factor *= itemFactor;
        }
        return factor;
    }

    private void ApplyPerRollScoreBonuses(PlayerData player)
    {
        if (player?.inventory == null || cardManager == null)
            return;

        for (int i = 0; i < player.inventory.Count; i++)
        {
            var itemDef = cardManager.GetItemById(player.inventory[i]);
            if (itemDef == null || !itemDef.grantRollBonus || itemDef.perRollBonus == null)
                continue;

            for (int j = 0; j < AllPassions.Length; j++)
            {
                ApplyPerRollForPassion(player, itemDef, AllPassions[j]);
            }
        }
    }

    private void ApplyPerRollForPassion(PlayerData player, ItemCardDefinition itemDef, PassionColor passion)
    {
        int basePoints = itemDef.perRollBonus.GetAmount(passion);
        if (basePoints == 0) return;

        float factor = passion == player.passion ? 1.2f : 1f;
        factor *= GetItemScoreMultiplier(player, passion);

        int finalPoints = Mathf.RoundToInt(basePoints * factor);
        if (finalPoints == 0) return;

        AddPassionPoints(player, passion, finalPoints);

        Debug.Log($"[GameManager] {player.playerName} gains {finalPoints} passive points in {passion} from item '{itemDef.title}'. Total: {player.GetTotalScore()}");
    }

    private void ResolveField(PlayerData player)
    {
        if (boardManager == null) return;

        FieldType fieldType = boardManager.GetFieldTypeAt(player.boardPosition);
        BoardFieldDefinition fieldDef = boardManager.GetFieldDefinitionAt(player.boardPosition);

        Debug.Log($"[GameManager] {player.playerName} landed on a {fieldType} field (Index: {player.boardPosition}).");

        if (fieldDef != null && fieldDef.isLastCrossroadsField)
        {
            Debug.Log($"[GameManager] ResolveField fallback: Last Crossroads Field already processed during movement.");
            return;
        }

        if (gameFeedbackUI != null && fieldDef != null)
        {
            string desc = fieldDef.GetDescription(player.riskFlag);
            if (!string.IsNullOrEmpty(desc))
                gameFeedbackUI.ShowFieldFeedback(player.playerName, player.passion, desc);
        }

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

        if (fieldDef != null)
        {
            string desc = fieldDef.GetDescription(player.riskFlag);
            if (!string.IsNullOrEmpty(desc))
                Debug.Log($"[GameManager] Field description: {desc}");

            if (fieldDef.requiresManualCardIdInput)
                StartManualCardInput(player);
        }
    }

    private void HandleNeutralField(PlayerData player, BoardFieldDefinition fieldDef)
    {
        if (fieldDef != null && fieldDef.HasFieldCard(player.riskFlag))
        {
            var fieldCard = fieldDef.GetFieldCard(player.riskFlag, cardManager);

            if (fieldCard != null && fieldCard.triggersManualScan)
            {
                Debug.Log($"[GameManager] Neutral field {fieldDef.index} uses manual-scan FieldCard. Prompting {player.playerName}.");
                StartManualCardInput(player);
            }
            else if (fieldCard != null && fieldCard.triggersMinigame)
            {
                // TODO: Minigame trigger logic
                string minigameId = !string.IsNullOrEmpty(fieldCard.minigameId) ? fieldCard.minigameId : "default";
                Debug.Log($"[GameManager] Neutral field {fieldDef.index} triggers minigame '{minigameId}' for {player.playerName}.");
                StartMinigame(minigameId, player);
            }
            else if (fieldCard != null)
            {
                PassionColor? passionForPoints = fieldDef.yieldsPassionScore ? fieldDef.passionReward : (PassionColor?)null;
                string routeType = player.riskFlag ? "RISK" : "SAFE";
                Debug.Log($"[GameManager] Applying FieldCard '{fieldCard.title}' ({routeType}) on Neutral field {fieldDef.index}.");
                cardResolver.ApplyCard(fieldCard, player, passionForPoints);
            }
        }

        if (fieldDef != null && fieldDef.yieldsPassionScore && fieldDef.passionRewardAmount != 0)
        {
            int basePoints = fieldDef.passionRewardAmount;
            PassionColor passion = fieldDef.passionReward;

            float factor = passion == player.passion ? 1.2f : 1f;
            factor *= GetItemScoreMultiplier(player, passion);

            int finalPoints = Mathf.RoundToInt(basePoints * factor);
            if (finalPoints != 0)
            {
                AddPassionPoints(player, passion, finalPoints);
                Debug.Log($"[GameManager] {player.playerName} received {finalPoints} {passion} points for landing on field {fieldDef.index} (base {basePoints}, factor {factor:F2}).");
            }
        }

        if (fieldDef?.requiresManualCardIdInput == true)
            StartManualCardInput(player);
    }

    private void HandleEventField(PlayerData player, BoardFieldDefinition fieldDef)
    {
        if (cardManager == null) return;

        EventCardDefinition card = fieldDef?.GetEventCard(player.riskFlag, cardManager);
        if (card == null)
            card = cardManager.DrawRandomEventCard();

        if (card == null) return;

        string routeType = player.riskFlag ? "RISK" : "SAFE";
        Debug.Log($"[GameManager] Applying event card '{card.title}' ({routeType}) to {player.playerName}.");

        PassionColor? passionForPoints = fieldDef?.yieldsPassionScore == true ? fieldDef.passionReward : (PassionColor?)null;
        cardResolver.ApplyCard(card, player, passionForPoints);
    }

    private void HandleItemShopField(PlayerData player)
    {
        if (cardManager == null) return;

        BoardFieldDefinition fieldDef = boardManager.GetFieldDefinitionAt(player.boardPosition);

        ItemCardDefinition card = fieldDef?.GetItemCard(player.riskFlag, cardManager);
        if (card == null)
            card = cardManager.DrawRandomItemCard();

        if (card != null)
        {
            string routeType = player.riskFlag ? "RISK" : "SAFE";
            Debug.Log($"[GameManager] Applying item card '{card.title}' ({routeType}) to {player.playerName}.");
            cardResolver.ApplyCard(card, player);
        }
    }

    private void HandleMinigameField(PlayerData player)
    {
        Debug.Log($"[GameManager] Minigame field reached by {player.playerName}!");
    }

    private void HandleCrossroadField(PlayerData player)
    {
        Debug.Log($"[GameManager] {player.playerName} is at crossroad. Current route: {(player.riskFlag ? "RISK" : "SAFE")}");
    }

    private void HandleFinishField(PlayerData player)
    {
        Debug.Log($"[GameManager] {player.playerName} reached the finish!");

        if (player.hasFinished) return;

        player.hasFinished = true;
        player.availableRolls = 0;

        if (player.HasPartner)
        {
            Debug.Log($"[GameManager] {player.playerName} crossed the finish line! Relationship ends.");
            ClearPartner(player);
        }

        int finishIndex = boardManager != null ? boardManager.totalFields - 1 : player.boardPosition;
        if (player.boardPosition > finishIndex)
            player.boardPosition = finishIndex;

        if (firstFinisher == null)
        {
            firstFinisher = player;

            int mainScoreBefore = player.passionScores.GetScore(player.passion);
            int bonus = Mathf.RoundToInt(mainScoreBefore * 0.2f);

            if (bonus > 0)
            {
                AddPassionPoints(player, player.passion, bonus);
                Debug.Log($"[GameManager] {player.playerName} is FIRST to finish and gets +{bonus} points in {player.passion} (1.2x bonus).");
            }
        }

        ClearEffectsAndRedeemItems(player);
    }

    private void HandleLastCrossroadsField(PlayerData player)
    {
        HandleLastCrossroadsFieldWithContinuation(player);
    }

    private void HandleLastCrossroadsFieldWithContinuation(PlayerData player)
    {
        Debug.Log($"[GameManager] {player.playerName} stopped at Last Crossroads Field. RiskFlag: {player.riskFlag}");

        int remainingMovement = player.pendingMovement;

        if (!player.riskFlag)
        {
            Debug.Log($"[GameManager] {player.playerName} was on safe route. No popup, continuing movement.");
            player.riskFlag = false;
            ContinueAfterLastCrossroads(player, remainingMovement);
            return;
        }

        bool isSuccess = Random.value >= 0.5f;
        Debug.Log($"[GameManager] {player.playerName} risk outcome: {(isSuccess ? "SUCCESS" : "FAILED")}");

        if (isSuccess)
        {
            float factor = 1.2f;
            factor *= GetItemScoreMultiplier(player, player.passion);
            int finalPoints = Mathf.RoundToInt(10 * factor);

            AddPassionPoints(player, player.passion, finalPoints);
            Debug.Log($"[GameManager] {player.playerName} gains {finalPoints} points in {player.passion} from risk success!");
        }

        if (lastCrossroadsPopupUI != null)
        {
            isLastCrossroadsPopupOpen = true;
            lastCrossroadsPopupUI.Show(player, isSuccess, () =>
            {
                isLastCrossroadsPopupOpen = false;
                player.riskFlag = false;
                ContinueAfterLastCrossroads(player, remainingMovement);
            });
        }
        else
        {
            player.riskFlag = false;
            ContinueAfterLastCrossroads(player, remainingMovement);
        }
    }

    private void ContinueAfterLastCrossroads(PlayerData player, int remainingMovement)
    {
        if (remainingMovement > 0)
        {
            Debug.Log($"[GameManager] Continuing movement with {remainingMovement} remaining steps after Last Crossroads.");
            MovePlayerWithCrossroadCheck(player, remainingMovement);
        }
        else
        {
            Debug.Log($"[GameManager] {player.playerName} ended exactly on Last Crossroads Field. Turn complete.");
            ProcessScheduledRisksForPlayer(player);
            CheckForGameEnd();
        }
    }

    public void StartManualCardInput(PlayerData player)
    {
        Debug.Log($"[GameManager] Manual card input required for {player.playerName}.");
        isManualInputOpen = true;

        if (manualCardInputUI != null)
            manualCardInputUI.ShowForPlayer(player);
    }

    public void AddStatusEffect(PlayerData player, BaseCardDefinition card)
    {
        if (player == null || card == null) return;

        var cfg = card.statusLifetime;
        bool hasLifetime = cfg.useRollLifetime || cfg.useTurnLifetime || cfg.useRollValueBreak || cfg.skipRounds > 0;
        bool shouldTrack = card.trackAsStatusEffect || hasLifetime;

        if (!shouldTrack) return;

        if (card.trackAsStatusEffect && !player.statusEffectCards.Contains(card.id))
        {
            player.statusEffectCards.Add(card.id);
            Debug.Log($"[GameManager] Added status effect '{card.title}' to {player.playerName}.");
        }

        var existing = player.activeStatusEffects.Find(e => e.cardId == card.id);
        if (existing == null)
        {
            player.activeStatusEffects.Add(new ActiveStatusEffectState
            {
                cardId = card.id,
                remainingRolls = cfg.useRollLifetime ? cfg.maxRolls : 0,
                remainingTurns = cfg.useTurnLifetime ? cfg.maxTurns : 0,
                remainingSkipRounds = cfg.skipRounds
            });
        }
        else
        {
            if (cfg.useRollLifetime)
                existing.remainingRolls = cfg.maxRolls;
            if (cfg.useTurnLifetime)
                existing.remainingTurns = cfg.maxTurns;
            existing.remainingSkipRounds = cfg.skipRounds;
        }
    }

    public void RemoveStatusEffect(PlayerData player, string cardId)
    {
        if (player == null || string.IsNullOrEmpty(cardId)) return;

        player.statusEffectCards.Remove(cardId);
        player.activeStatusEffects?.RemoveAll(e => e.cardId == cardId);

        if (cardManager != null)
        {
            var def = cardManager.GetCardById(cardId);
            if (def is ItemCardDefinition && player.inventory.Remove(cardId))
            {
                Debug.Log($"[GameManager] Removed expired item '{cardId}' from {player.playerName}'s inventory.");
            }
        }

        Debug.Log($"[GameManager] Removed status effect '{cardId}' from {player.playerName}.");
    }

    public void OnManualCardInputClosed() => isManualInputOpen = false;
    public void OnInventoryOpened() => isInventoryOpen = true;

    public void OnInventoryClosed()
    {
        OnCardPopupClosed();
        isInventoryOpen = false;
    }

    public void OpenInventoryView()
    {
        if (inventoryUI != null)
            inventoryUI.Show();
    }

    public void OnCardPopupOpened() => isCardPopupOpen = true;
    public void OnCardPopupClosed() => isCardPopupOpen = false;

    public void ApplyCardFromIdForCurrentPlayer(string cardId, PassionColor? passionForPoints = null)
    {
        ApplyCardFromId(cardId, GetCurrentPlayer(), passionForPoints);
    }

    public void ApplyCardFromId(string cardId, PlayerData targetPlayer, PassionColor? passionForPoints = null)
    {
        if (cardManager == null || targetPlayer == null) return;

        BaseCardDefinition card = cardManager.GetCardById(cardId);
        if (card == null) return;

        Debug.Log($"[GameManager] Applying card '{card.title}' to {targetPlayer.playerName} by manual input.");
        cardResolver.ApplyCard(card, targetPlayer, passionForPoints);

        if (gameFeedbackUI != null)
        {
            if (card is EventCardDefinition)
            {
                gameFeedbackUI.ShowEventCardFeedback(targetPlayer.playerName, targetPlayer.passion, card.title);
            }
            else if (card is ItemCardDefinition)
            {
                gameFeedbackUI.ShowItemCardFeedback(targetPlayer.playerName, targetPlayer.passion, card.title);
            }
        }
    }

    private bool CheckForGameEnd()
    {
        if (boardManager == null || players == null || players.Count == 0)
            return false;

        for (int i = 0; i < players.Count; i++)
        {
            if (!players[i].hasFinished)
                return false;
        }

        PlayerData winner = null;
        int bestScore = int.MinValue;

        for (int i = 0; i < players.Count; i++)
        {
            int finalScore = players[i].GetTotalScore();
            if (finalScore > bestScore)
            {
                winner = players[i];
                bestScore = finalScore;
            }
        }

        if (winner != null)
        {
            Debug.Log($"[GameManager] GAME OVER. Winner: {winner.playerName} with TOTAL score {bestScore}");
        }

        if (screenManager != null)
            screenManager.ShowResultsScreen();

        return true;
    }

    private void AdvanceToNextPlayer()
    {
        if (players == null || players.Count == 0) return;

        int startIndex = currentPlayerIndex;
        int safeGuard = 0;

        do
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
            safeGuard++;
            if (safeGuard > players.Count + 1) break;
        } while (players[currentPlayerIndex].hasFinished && currentPlayerIndex != startIndex);

        StartTurnForCurrentPlayer();

        Debug.Log($"[GameManager] Next player: {GetCurrentPlayer()?.playerName}");
        PrintPlayerStates();
    }

    private void PrintPlayerStates()
    {
        Debug.Log("[GameManager] Player states:");
        for (int i = 0; i < players.Count; i++)
        {
            var p = players[i];
            Debug.Log(
                $"{p.playerName} ({p.passion})\n" +
                $"  Position: {p.boardPosition}, Finished: {p.hasFinished}\n" +
                $"  Scores: Y:{p.passionScores.yellow} G:{p.passionScores.green} B:{p.passionScores.blue} P:{p.passionScores.purple} Pi:{p.passionScores.pink} O:{p.passionScores.orange}\n" +
                $"  Stars: {p.starCount}, Rolls: {p.availableRolls}, Total: {p.GetTotalScore()}"
            );
        }
    }

    public void StartMinigame(string minigameId, PlayerData player)
    {
        Debug.Log($"[GameManager] Would load minigame '{minigameId}' for {player.playerName}");
    }

    public void ScheduleRiskOutcome(EventCardDefinition card, PlayerData player, PassionColor? passionForPoints = null)
    {
        if (card == null || player == null) return;

        scheduledRisks.Add(new ScheduledRisk
        {
            riskCard = card,
            targetPlayer = player,
            passionForPoints = passionForPoints,
            remainingTurns = Mathf.Max(1, card.riskDurationTurns)
        });
    }

    private void ProcessScheduledRisksForPlayer(PlayerData playerJustMoved)
    {
        for (int i = scheduledRisks.Count - 1; i >= 0; i--)
        {
            var risk = scheduledRisks[i];
            if (risk.targetPlayer != playerJustMoved) continue;

            risk.remainingTurns--;

            float probability = risk.remainingTurns >= 2 ? 0.5f : risk.remainingTurns == 1 ? 0.7f : 0.9f;

            if (Random.value <= probability || risk.remainingTurns <= 0)
            {
                Debug.Log($"[GameManager] Risk outcome triggered for {playerJustMoved.playerName}!");
                cardResolver.ApplyCard(risk.riskCard, playerJustMoved, risk.passionForPoints);
                scheduledRisks.RemoveAt(i);
            }
        }
    }

    private void UpdateStatusEffectsOnRoll(PlayerData player, int baseRoll)
    {
        if (player?.activeStatusEffects == null || cardManager == null) return;

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

            if (cfg.useRollLifetime && state.remainingRolls > 0)
            {
                state.remainingRolls--;
                if (state.remainingRolls <= 0)
                {
                    Debug.Log($"[GameManager] Status effect '{card.title}' expired for {player.playerName} (roll lifetime).");
                    RemoveStatusEffect(player, card.id);
                    continue;
                }
            }

            if (cfg.useRollValueBreak && cfg.breakOnRollValues != null)
            {
                for (int r = 0; r < cfg.breakOnRollValues.Length; r++)
                {
                    if (cfg.breakOnRollValues[r] == baseRoll)
                    {
                        Debug.Log($"[GameManager] Status effect '{card.title}' removed for {player.playerName} (rolled {baseRoll}).");
                        RemoveStatusEffect(player, card.id);
                        break;
                    }
                }
            }
        }
    }

    private bool HandleTurnStartStatusEffects(PlayerData player)
    {
        if (player?.activeStatusEffects == null || cardManager == null) return false;

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

                Debug.Log($"[GameManager] {player.playerName} must skip a turn due to '{card.title}'. Skip rounds left: {state.remainingSkipRounds}");

                if (state.remainingSkipRounds <= 0)
                {
                    Debug.Log($"[GameManager] Skip effect '{card.title}' expired for {player.playerName}.");
                    RemoveStatusEffect(player, card.id);
                }
            }
        }

        if (mustSkip)
        {
            Debug.Log($"[GameManager] {player.playerName} skips this entire turn.");

            PlayerData skippedPlayer = player;
            PlayerData nextPlayer = GetNextPlayer();

            AdvanceToNextPlayer();

            if (gameFeedbackUI != null && nextPlayer != null)
            {
                gameFeedbackUI.ShowTurnSkippedFeedback(skippedPlayer.playerName, skippedPlayer.passion, nextPlayer.playerName, nextPlayer.passion);
            }

            return true;
        }

        return false;
    }

    private void ClearEffectsAndRedeemItems(PlayerData player)
    {
        if (player == null || cardManager == null) return;

        for (int i = 0; i < player.inventory.Count; i++)
        {
            var item = cardManager.GetItemById(player.inventory[i]);
            if (item != null && item.isRedeemable && item.redeemScores != null)
                RedeemItemScores(player, item);
        }

        player.inventory.Clear();
        player.statusEffectCards.Clear();
        player.activeStatusEffects?.Clear();

        Debug.Log($"[GameManager] Cleared all items and status effects for {player.playerName} at finish.");
    }

    private void RedeemItemScores(PlayerData player, ItemCardDefinition item)
    {
        if (player == null || item?.redeemScores == null) return;

        RedeemPassion(player, item, PassionColor.Yellow, item.redeemScores.yellow);
        RedeemPassion(player, item, PassionColor.Green, item.redeemScores.green);
        RedeemPassion(player, item, PassionColor.Blue, item.redeemScores.blue);
        RedeemPassion(player, item, PassionColor.Purple, item.redeemScores.purple);
        RedeemPassion(player, item, PassionColor.Pink, item.redeemScores.pink);
        RedeemPassion(player, item, PassionColor.Orange, item.redeemScores.orange);
    }

    private void RedeemPassion(PlayerData player, ItemCardDefinition item, PassionColor passion, int delta)
    {
        if (delta == 0) return;

        AddPassionPoints(player, passion, delta);
        Debug.Log($"[GameManager] Redeemed item '{item.title}' for {delta} points in {passion} for {player.playerName}. Total: {player.GetTotalScore()}");
    }

    public bool IsInRelationship(PlayerData player)
    {
        return player != null && player.HasPartner;
    }

    public PlayerData GetPartner(PlayerData player)
    {
        if (player == null || !player.HasPartner)
            return null;

        if (player.partnerIndex < 0 || player.partnerIndex >= players.Count)
            return null;

        return players[player.partnerIndex];
    }

    public int GetPlayerIndex(PlayerData player)
    {
        if (player == null || players == null)
            return -1;

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i] == player)
                return i;
        }
        return -1;
    }

    public void SetPartners(PlayerData playerA, PlayerData playerB)
    {
        if (playerA == null || playerB == null || playerA == playerB)
            return;

        int indexA = GetPlayerIndex(playerA);
        int indexB = GetPlayerIndex(playerB);

        if (indexA < 0 || indexB < 0)
            return;

        ClearPartner(playerA);
        ClearPartner(playerB);

        playerA.partnerIndex = indexB;
        playerB.partnerIndex = indexA;

        Debug.Log($"[GameManager] {playerA.playerName} and {playerB.playerName} are now partners.");
    }

    public void ClearPartner(PlayerData player)
    {
        if (player == null || !player.HasPartner)
            return;

        PlayerData partner = GetPartner(player);
        
        if (partner != null)
        {
            Debug.Log($"[GameManager] Cleared partnership between {player.playerName} and {partner.playerName}.");
            partner.partnerIndex = -1;
        }

        player.partnerIndex = -1;
    }

    public void ClearAllRelationships()
    {
        if (players == null) return;

        for (int i = 0; i < players.Count; i++)
        {
            players[i].partnerIndex = -1;
        }

        Debug.Log("[GameManager] Cleared all relationships.");
    }

    public void OnPartnerPanelOpened()
    {
        isPartnerPanelOpen = true;
    }

    public void OnPartnerPanelClosed()
    {
        isPartnerPanelOpen = false;
    }
}
