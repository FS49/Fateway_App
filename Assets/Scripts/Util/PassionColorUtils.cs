using UnityEngine;

public static class PassionColorUtils
{
    public static Color GetColor(PassionColor passion)
    {
        switch (passion)
        {
            case PassionColor.Yellow:
                return new Color32(250, 206, 122, 255);

            case PassionColor.Green:
                return new Color32(  177, 219, 130, 255);

            case PassionColor.Blue:
                return new Color32(  102, 208, 237, 255);

            case PassionColor.Purple:
                return new Color32(174,   164, 233, 255);

            case PassionColor.Pink:
                return new Color32(237, 151, 203, 255);

            case PassionColor.Orange:
                return new Color32(243, 158, 106, 255);

            default:
                return Color.white;
        }
    }
}