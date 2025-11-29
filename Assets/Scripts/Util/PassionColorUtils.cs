using UnityEngine;

public static class PassionColorUtils
{
    private static readonly Color32 YellowColor = new Color32(250, 206, 122, 255);
    private static readonly Color32 GreenColor = new Color32(177, 219, 130, 255);
    private static readonly Color32 BlueColor = new Color32(102, 208, 237, 255);
    private static readonly Color32 PurpleColor = new Color32(174, 164, 233, 255);
    private static readonly Color32 PinkColor = new Color32(237, 151, 203, 255);
    private static readonly Color32 OrangeColor = new Color32(243, 158, 106, 255);

    public static Color GetColor(PassionColor passion)
    {
        return passion switch
        {
            PassionColor.Yellow => YellowColor,
            PassionColor.Green => GreenColor,
            PassionColor.Blue => BlueColor,
            PassionColor.Purple => PurpleColor,
            PassionColor.Pink => PinkColor,
            PassionColor.Orange => OrangeColor,
            _ => Color.white
        };
    }
}
