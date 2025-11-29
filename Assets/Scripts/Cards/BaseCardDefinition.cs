using UnityEngine;

public abstract class BaseCardDefinition : ScriptableObject
{
    [Header("Basic Info")]
    public string id;
    public string title;
    [TextArea] public string description;

    [Header("Card Type")]
    public CardType cardType;
    public CardEffectType effectType;

    [Header("Media Templates (placeholders)")]
    public string audioTemplateName;
    public string videoTemplateName;

    [Header("Inventory / Status UI")]
    [Tooltip("If true, this card will be shown in the player's Status Effects list when applied (non-items).")]
    public bool trackAsStatusEffect = false;

    [Header("Polarity / Risk Flags")]
    [Tooltip("True if this card is generally beneficial.")]
    public bool isPositive = false;

    [Tooltip("True if this card is generally harmful.")]
    public bool isNegative = false;

    [Tooltip("True if this card represents a risky effect (can go good or bad).")]
    public bool isRisk = false;

    [Header("Status Effect Lifetime")]
    [Tooltip("If this card is tracked as a status effect, this config defines how long it stays active.")]
    public StatusEffectLifetimeConfig statusLifetime = new StatusEffectLifetimeConfig();

}
