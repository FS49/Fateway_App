using System;
using UnityEngine;

[Serializable]
public class PassionScoreSet
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

    public void AddScore(PassionColor passion, int delta)
    {
        switch (passion)
        {
            case PassionColor.Yellow:  yellow  += delta; break;
            case PassionColor.Green:   green   += delta; break;
            case PassionColor.Blue:    blue    += delta; break;
            case PassionColor.Purple:  purple  += delta; break;
            case PassionColor.Pink:    pink    += delta; break;
            case PassionColor.Orange:  orange  += delta; break;
        }
    }
}