using UnityEngine;

[System.Serializable]
public class PassionAddConfig
{
    public int yellow;
    public int green;
    public int blue;
    public int purple;
    public int pink;
    public int orange;

    public int GetAmount(PassionColor passion)
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
}
