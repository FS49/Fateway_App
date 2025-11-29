using UnityEngine;

[CreateAssetMenu(menuName = "Fateway/Cards/Event Card", fileName = "EventCard_")]
public class EventCardDefinition : PointsCardDefinition
{
    [Header("Event Settings")]
    [Tooltip("If true, this event should apply to the last-place player instead of current.")]
    public bool onlyAffectsLastPlace;

    [Header("Minigame")]
    [Tooltip("Does this event start a minigame?")]
    public bool triggersMinigame;

    [Tooltip("Identifier or scene name for the minigame.")]
    public string minigameId;

    [Header("Risk / Delayed Outcome")]
    [Tooltip("If true, this event schedules a delayed risk outcome.")]
    public bool isRiskOutcome;

    [Tooltip("Number of turns over which this risk can trigger (e.g. 3).")]
    public int riskDurationTurns = 3;
}