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
    public bool hasFinished;

    [Header("Scoring")]
    public PassionScoreSet passionScores = new PassionScoreSet();
    public int starCount;

    [Header("Inventory & Status")]
    public List<string> inventory = new List<string>();
    public List<string> statusEffectCards = new List<string>();
    public List<ActiveStatusEffectState> activeStatusEffects = new List<ActiveStatusEffectState>();

    public PlayerData(string name, PassionColor passion, Gender gender)
    {
        playerName = name;
        this.passion = passion;
        this.gender = gender;
        boardPosition = 0;
        availableRolls = 1;
        hasFinished = false;
        starCount = 0;
        passionScores = new PassionScoreSet();
        inventory = new List<string>();
        statusEffectCards = new List<string>();
        activeStatusEffects = new List<ActiveStatusEffectState>();
    }

    public int GetTotalScore()
    {
        return passionScores.TotalScore;
    }
}
