using UnityEngine;

[CreateAssetMenu(menuName = "Fateway/Cards/Item Card", fileName = "ItemCard_")]
public class ItemCardDefinition : PointsCardDefinition
{
    [Header("Redeemable Score on Finish")]
    public bool isRedeemable;
    public PassionAddConfig redeemScores = new PassionAddConfig();

    [Header("Item Tags")]
    public string[] tags;
    public bool uniquePerPlayer;

    [Header("Combo Settings")]
    public bool partOfCombo;

    [Header("Score Multipliers")]
    public bool enableScoreMultipliers;
    public PassionMultiplierConfig passionMultipliers = new PassionMultiplierConfig();

    [Header("Passive Per-Roll Bonus")]
    public bool grantRollBonus;
    public PassionAddConfig perRollBonus = new PassionAddConfig();
    public PassionColor rollBonusPassion = PassionColor.Yellow;
    public int rollBonusPoints;

    [Header("Dice Buffs")]
    public int diceRollBonus;

    [Header("Odd/Even Roll Modifiers")]
    public int oddRollBonus;
    public int evenRollBonus;
}
