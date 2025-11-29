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
        return passion switch
        {
            PassionColor.Yellow => yellow,
            PassionColor.Green => green,
            PassionColor.Blue => blue,
            PassionColor.Purple => purple,
            PassionColor.Pink => pink,
            PassionColor.Orange => orange,
            _ => 0
        };
    }

    public void AddScore(PassionColor passion, int amount)
    {
        switch (passion)
        {
            case PassionColor.Yellow: yellow += amount; break;
            case PassionColor.Green: green += amount; break;
            case PassionColor.Blue: blue += amount; break;
            case PassionColor.Purple: purple += amount; break;
            case PassionColor.Pink: pink += amount; break;
            case PassionColor.Orange: orange += amount; break;
        }
    }

    public int TotalScore => yellow + green + blue + purple + pink + orange;
}
