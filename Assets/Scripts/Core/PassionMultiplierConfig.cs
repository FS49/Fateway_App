using UnityEngine;

[System.Serializable]
public class PassionMultiplierConfig
{
    [Tooltip("Multiplier for Yellow passion (1 = no change).")]
    public float yellow = 1f;

    [Tooltip("Multiplier for Green passion (1 = no change).")]
    public float green = 1f;

    [Tooltip("Multiplier for Blue passion (1 = no change).")]
    public float blue = 1f;

    [Tooltip("Multiplier for Purple passion (1 = no change).")]
    public float purple = 1f;

    [Tooltip("Multiplier for Pink passion (1 = no change).")]
    public float pink = 1f;

    [Tooltip("Multiplier for Orange passion (1 = no change).")]
    public float orange = 1f;

    public float GetMultiplier(PassionColor passion)
    {
        switch (passion)
        {
            case PassionColor.Yellow:  return yellow;
            case PassionColor.Green:   return green;
            case PassionColor.Blue:    return blue;
            case PassionColor.Purple:  return purple;
            case PassionColor.Pink:    return pink;
            case PassionColor.Orange:  return orange;
            default:                   return 1f;
        }
    }
}