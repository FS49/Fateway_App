using System;

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

    public void AddScore(PassionColor passion, int delta)
    {
        switch (passion)
        {
            case PassionColor.Yellow: yellow = Math.Max(0, yellow + delta); break;
            case PassionColor.Green: green = Math.Max(0, green + delta); break;
            case PassionColor.Blue: blue = Math.Max(0, blue + delta); break;
            case PassionColor.Purple: purple = Math.Max(0, purple + delta); break;
            case PassionColor.Pink: pink = Math.Max(0, pink + delta); break;
            case PassionColor.Orange: orange = Math.Max(0, orange + delta); break;
        }
    }

    public int TotalScore => yellow + green + blue + purple + pink + orange;
}
