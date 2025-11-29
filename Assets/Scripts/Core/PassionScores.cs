using UnityEngine;

[System.Serializable]
public class PassionScores
{
    public int yellow;
    public int green;
    public int blue;
    public int purple;
    public int pink;
    public int orange;

    public int GetScore(PassionColor passion)
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

    public void AddScore(PassionColor passion, int amount)
    {
        switch (passion)
        {
            case PassionColor.Yellow:  yellow  += amount; break;
            case PassionColor.Green:   green   += amount; break;
            case PassionColor.Blue:    blue    += amount; break;
            case PassionColor.Purple:  purple  += amount; break;
            case PassionColor.Pink:    pink    += amount; break;
            case PassionColor.Orange:  orange  += amount; break;
        }
    }

    public int TotalScore =>
        yellow + green + blue + purple + pink + orange;
}