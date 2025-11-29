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
            case PassionColor.Yellow: yellow += delta; break;
            case PassionColor.Green: green += delta; break;
            case PassionColor.Blue: blue += delta; break;
            case PassionColor.Purple: purple += delta; break;
            case PassionColor.Pink: pink += delta; break;
            case PassionColor.Orange: orange += delta; break;
        }
    }

    public int TotalScore => yellow + green + blue + purple + pink + orange;
}
