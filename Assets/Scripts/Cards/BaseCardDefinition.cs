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

    [Header("Media Templates")]
    public string audioTemplateName;
    public string videoTemplateName;

    [Header("Inventory / Status UI")]
    public bool trackAsStatusEffect;

    [Header("Polarity / Risk Flags")]
    public bool isPositive;
    public bool isNegative;
    public bool isRisk;

    [Header("Status Effect Lifetime")]
    public StatusEffectLifetimeConfig statusLifetime = new StatusEffectLifetimeConfig();
}
