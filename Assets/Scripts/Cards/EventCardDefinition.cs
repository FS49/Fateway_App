using UnityEngine;

[CreateAssetMenu(menuName = "Fateway/Cards/Event Card", fileName = "EventCard_")]
public class EventCardDefinition : PointsCardDefinition
{
    [Header("Event Settings")]
    public bool onlyAffectsLastPlace;

    [Header("Minigame")]
    public bool triggersMinigame;
    public string minigameId;

    [Header("Risk / Delayed Outcome")]
    public bool isRiskOutcome;
    public int riskDurationTurns = 3;
}
