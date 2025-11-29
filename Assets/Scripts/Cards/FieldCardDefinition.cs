using UnityEngine;

[CreateAssetMenu(menuName = "Fateway/Cards/Field Card", fileName = "FieldCard_")]
public class FieldCardDefinition : PointsCardDefinition
{
    [Header("Field Behavior")]
    public bool triggersManualScan;
}
