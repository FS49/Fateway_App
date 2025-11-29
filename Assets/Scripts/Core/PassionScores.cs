using System;

[Serializable]
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
            case PassionColor.Yellow: yellow = Math.Max(0, yellow + amount); break;
            case PassionColor.Green: green = Math.Max(0, green + amount); break;
            case PassionColor.Blue: blue = Math.Max(0, blue + amount); break;
            case PassionColor.Purple: purple = Math.Max(0, purple + amount); break;
            case PassionColor.Pink: pink = Math.Max(0, pink + amount); break;
            case PassionColor.Orange: orange = Math.Max(0, orange + amount); break;
        }
    }

    public int TotalScore => yellow + green + blue + purple + pink + orange;
}
