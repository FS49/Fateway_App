using UnityEngine;

[System.Serializable]
public class PassionAddConfig
{
    [Tooltip("Flat points to add for Yellow passion.")]
    public int yellow = 0;

    [Tooltip("Flat points to add for Green passion.")]
    public int green = 0;

    [Tooltip("Flat points to add for Blue passion.")]
    public int blue = 0;

    [Tooltip("Flat points to add for Purple passion.")]
    public int purple = 0;

    [Tooltip("Flat points to add for Pink passion.")]
    public int pink = 0;

    [Tooltip("Flat points to add for Orange passion.")]
    public int orange = 0;

    public int GetAmount(PassionColor passion)
    {
        switch (passion)
        {
            case PassionColor.Yellow:  return yellow;
            case PassionColor.Green:   return green;
            case PassionColor.Blue:    return blue;
            case PassionColor.Purple:  return purple;
            case PassionColor.Pink:    return pink;
            case PassionColor.Orange:  return orange;
            default:                   return 0;
        }
    }
}