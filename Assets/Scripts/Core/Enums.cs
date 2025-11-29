using UnityEngine;

public enum PassionColor
{
    Yellow,   // Kreativ
    Green,    // Fitness
    Blue,     // Wissen
    Purple,   // Erfolg
    Pink,     // Sozial
    Orange    // Abenteuerlustig
}

public enum Gender
{
    Male,
    Female
}

public enum FieldType
{
    Neutral,
    Event,
    ItemShop,
    Minigame,
    Crossroad,
    Finish
}

public enum CardType
{
    Event,
    Item,
    Field
}

public enum CardEffectType
{
    GivePoints,
    GiveItem,
    TakeItem,
    StartMinigame,
    ScheduleRiskOutcome,
    HelpLastPlacePlayer,
    ShowInventory,   // NEW
    Custom
}
