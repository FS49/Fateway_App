using UnityEngine;

[CreateAssetMenu(menuName = "Fateway/Cards/Field Card", fileName = "FieldCard_")]
public class FieldCardDefinition : PointsCardDefinition
{
    [Header("Field Behavior")]
    [Tooltip("If true, landing on a field using this card will trigger manual card scan instead of applying points directly.")]
    public bool triggersManualScan = false;

    // You could still use pointsDelta in combination with triggersManualScan later if you want.
}