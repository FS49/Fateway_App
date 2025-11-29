using System;
using UnityEngine;

[Serializable]
public class StatusEffectLifetimeConfig
{
    [Header("Roll-based lifetime")]
    [Tooltip("If true, this effect expires after a certain number of rolls by the owning player.")]
    public bool useRollLifetime = false;

    [Tooltip("Number of rolls the effect should last (owner's rolls).")]
    public int maxRolls = 0;

    [Header("Turn-based lifetime")]
    [Tooltip("If true, this effect expires after a certain number of turns of the owning player.")]
    public bool useTurnLifetime = false;

    [Tooltip("Number of the player's turns the effect should last.")]
    public int maxTurns = 0;

    [Header("Break on specific dice values (base roll only, no bonus)")]
    [Tooltip("If true, this effect ends immediately if one of these dice values is rolled (base, 1–6).")]
    public bool useRollValueBreak = false;

    [Tooltip("Dice values (1–6) that will break this effect when rolled by the owning player.")]
    [Range(1, 6)]
    public int[] breakOnRollValues = new int[0];

    [Header("Skip turns")]
    [Tooltip("How many full turns the player must skip because of this effect.")]
    public int skipRounds = 0;
}