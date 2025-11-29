using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerData
{
    [Header("Identity")]
    public string playerName;
    public PassionColor passion;
    public Gender gender;

    [Header("Board State")]
    public int boardPosition;
    public int availableRolls = 1;

    [Tooltip("True if this player has reached the finish and is out of the turn rotation.")]
    public bool hasFinished = false;

    [Header("Scoring")]
    public PassionScoreSet passionScores = new PassionScoreSet();
    public int starCount = 0;

    [Header("Inventory & Status")]
    [Tooltip("Item card IDs currently held by this player.")]
    public List<string> inventory = new List<string>();

    [Tooltip("Status-effect card IDs currently affecting this player (non-items).")]
    public List<string> statusEffectCards = new List<string>();
    
    [Tooltip("Runtime data for active status effects (durability counters, skip rounds).")]
    public List<ActiveStatusEffectState> activeStatusEffects = new List<ActiveStatusEffectState>();

    public PlayerData(string name, PassionColor passion, Gender gender)
    {
        this.playerName = name;
        this.passion = passion;
        this.gender = gender;

        boardPosition = 0;
        availableRolls = 1;
        hasFinished = false;
        starCount = 0;
        passionScores = new PassionScoreSet();
        inventory = new List<string>();
        statusEffectCards = new List<string>();
    }

    /// <summary>
    /// Returns the sum of all passion scores (no multipliers, no bonuses).
    /// </summary>
    public int GetTotalScore()
    {
        return passionScores.yellow +
               passionScores.green +
               passionScores.blue +
               passionScores.purple +
               passionScores.pink +
               passionScores.orange;
    }
}