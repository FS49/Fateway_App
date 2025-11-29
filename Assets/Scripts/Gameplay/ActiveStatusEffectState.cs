using System;
using UnityEngine;

[Serializable]
public class ActiveStatusEffectState
{
    [Tooltip("Card ID of the status effect (matches BaseCardDefinition.id).")]
    public string cardId;

    [Tooltip("How many rolls this effect can still be active for (owner's rolls).")]
    public int remainingRolls;

    [Tooltip("How many of this player's turns this effect can still be active for.")]
    public int remainingTurns;

    [Tooltip("How many turns this card will force the player to skip.")]
    public int remainingSkipRounds;
}