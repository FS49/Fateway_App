using System;
using UnityEngine;

[Serializable]
public class StatusEffectLifetimeConfig
{
    [Header("Roll-based lifetime")]
    public bool useRollLifetime;
    public int maxRolls;

    [Header("Turn-based lifetime")]
    public bool useTurnLifetime;
    public int maxTurns;

    [Header("Break on specific dice values")]
    public bool useRollValueBreak;
    [Range(1, 6)]
    public int[] breakOnRollValues = Array.Empty<int>();

    [Header("Skip turns")]
    public int skipRounds;
}
