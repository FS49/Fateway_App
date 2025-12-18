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

    [Header("Relationship Effects")]
    public bool resetsAllRelationships;
    public bool resetsCurrentPlayerRelationship;

    [Header("Item Distribution")]
    public bool givesItemToNextPlayer;
    public string nextPlayerItemId;

    public bool givesItemToAllPlayers;
    public string allPlayersItemId;

    public bool givesItemToAllPartneredPlayers;
    public string allPartneredPlayersItemId;

    public bool givesItemToCurrentPlayerPartner;
    public string currentPlayerPartnerItemId;

    [Header("Partner Points Bonus")]
    public bool givesPointsToAllPartneredPlayers;
    public int partneredPlayersPointsDelta;

    public bool givesPointsToCurrentPlayerAndPartner;
    public int currentPlayerAndPartnerPointsDelta;
}
