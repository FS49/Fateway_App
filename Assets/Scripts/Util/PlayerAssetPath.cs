using UnityEngine;

public static class PlayerAssetPath
{
    private const string BasePath = "Players";

    public static string GetFolderPath(PassionColor passion, Gender gender)
    {
        string colorFolder = GetColorFolderName(passion);
        string genderFolder = GetGenderFolderName(gender);
        return $"{BasePath}/{colorFolder}/{genderFolder}";
    }

    public static string GetAssetPath(PassionColor passion, Gender gender, string assetName)
    {
        return $"{GetFolderPath(passion, gender)}/{assetName}";
    }

    public static string GetColorFolderName(PassionColor passion)
    {
        return passion switch
        {
            PassionColor.Yellow => "Yellow",
            PassionColor.Green => "Green",
            PassionColor.Blue => "Blue",
            PassionColor.Purple => "Purple",
            PassionColor.Pink => "Pink",
            PassionColor.Orange => "Orange",
            _ => "Yellow"
        };
    }

    public static string GetGenderFolderName(Gender gender)
    {
        return gender switch
        {
            Gender.Male => "Male",
            Gender.Female => "Female",
            _ => "Male"
        };
    }

    public static T LoadAsset<T>(PassionColor passion, Gender gender, string assetName) where T : Object
    {
        string path = GetAssetPath(passion, gender, assetName);
        T asset = Resources.Load<T>(path);
        
        if (asset == null)
        {
            Debug.LogWarning($"[PlayerAssetPath] Asset not found: {path}");
        }
        
        return asset;
    }

    public static T[] LoadAllAssets<T>(PassionColor passion, Gender gender) where T : Object
    {
        string path = GetFolderPath(passion, gender);
        T[] assets = Resources.LoadAll<T>(path);
        
        if (assets == null || assets.Length == 0)
        {
            Debug.LogWarning($"[PlayerAssetPath] No assets found in: {path}");
        }
        
        return assets;
    }

    public static Sprite LoadSprite(PassionColor passion, Gender gender, string spriteName)
    {
        return LoadAsset<Sprite>(passion, gender, spriteName);
    }

    public static Sprite[] LoadAllSprites(PassionColor passion, Gender gender)
    {
        return LoadAllAssets<Sprite>(passion, gender);
    }
}

