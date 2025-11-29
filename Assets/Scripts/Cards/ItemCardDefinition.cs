using UnityEngine;

[CreateAssetMenu(menuName = "Fateway/Cards/Item Card", fileName = "ItemCard_")]
public class ItemCardDefinition : PointsCardDefinition
{
    [Header("Redeemable Score on Finish")]
    [Tooltip("If true, this item will convert to passion score when the player crosses the finish line.")]
    public bool isRedeemable = false;

    [Tooltip("Passion scores granted (or subtracted if negative) when redeemed at finish.")]
    public PassionAddConfig redeemScores = new PassionAddConfig();
    
    [Header("Item Tags")]
    [Tooltip("Tags like Vehicle, Dangerous, Leisure, etc.")]
    public string[] tags;

    [Tooltip("If true, a player can only have one of this item (e.g., only one vehicle).")]
    public bool uniquePerPlayer;

    [Header("Combo Settings")]
    [Tooltip("If true, this item can be part of a combo event with other items.")]
    public bool partOfCombo;

    [Header("Score Multipliers")]
    [Tooltip("If true, this item multiplies scores for specific passions.")]
    public bool enableScoreMultipliers = false;
    
    [Tooltip("Per-passion score multipliers (1 = no change).")]
    public PassionMultiplierConfig passionMultipliers = new PassionMultiplierConfig();

    [Header("Passive Per-Roll Bonus")]
    [Tooltip("If true, this item grants bonus points each time the player rolls the dice.")]
    public bool grantRollBonus = false;
    public PassionAddConfig perRollBonus = new PassionAddConfig();
    
    [Tooltip("Which passion receives the per-roll bonus.")]
    public PassionColor rollBonusPassion = PassionColor.Yellow;

    [Tooltip("Base points gained in this passion on every roll (before multipliers).")]
    public int rollBonusPoints = 0;

    [Header("Dice Buffs")]
    [Tooltip("Additional value added to each dice roll while this item is in inventory.")]
    public int diceRollBonus = 0;
}