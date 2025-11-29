using UnityEngine;

public class PointsCardDefinition : BaseCardDefinition
{
    [Header("Simple Points (optional, legacy style)")]
    [Tooltip("If true, this card gives a flat pointsDelta to a single passion.")]
    public bool useSimplePoints = false;

    [Tooltip("Flat point change (can be positive or negative).")]
    public int pointsDelta = 0;

    [Tooltip("If true, apply pointsDelta to the player's main passion instead of simplePassion or any field override.")]
    public bool applyToMainPassion = false;

    [Tooltip("Which passion to affect if applyToMainPassion is false and no field override is given.")]
    public PassionColor simplePassion = PassionColor.Yellow;

    [Header("Multi-Passion Points")]
    [Tooltip("If true, this card can add points to multiple passions at once.")]
    public bool useMultiPassionPoints = false;

    [Tooltip("Per-passion point changes. Only used if useMultiPassionPoints is true.")]
    public PassionAddConfig multiPassionPoints = new PassionAddConfig();

    [Header("Extra Main Passion Points")]
    [Tooltip("If true, this card also adds a fixed amount to the player's main passion (player.passion).")]
    public bool useMainPassionPoints = false;

    [Tooltip("Flat points added to the player's main passion if useMainPassionPoints is true.")]
    public int mainPassionPointsDelta = 0;
}