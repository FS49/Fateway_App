using UnityEngine;

[System.Serializable]
public class PassionMultiplierConfig
{
    public float yellow = 1f;
    public float green = 1f;
    public float blue = 1f;
    public float purple = 1f;
    public float pink = 1f;
    public float orange = 1f;

    public float GetMultiplier(PassionColor passion)
    {
        return passion switch
        {
            PassionColor.Yellow => yellow,
            PassionColor.Green => green,
            PassionColor.Blue => blue,
            PassionColor.Purple => purple,
            PassionColor.Pink => pink,
            PassionColor.Orange => orange,
            _ => 1f
        };
    }
}
