using UnityEngine;

public class PointsCardDefinition : BaseCardDefinition
{
    [Header("Simple Points")]
    public bool useSimplePoints;
    public int pointsDelta;
    public bool applyToMainPassion;
    public PassionColor simplePassion = PassionColor.Yellow;

    [Header("Multi-Passion Points")]
    public bool useMultiPassionPoints;
    public PassionAddConfig multiPassionPoints = new PassionAddConfig();

    [Header("Extra Main Passion Points")]
    public bool useMainPassionPoints;
    public int mainPassionPointsDelta;
}
