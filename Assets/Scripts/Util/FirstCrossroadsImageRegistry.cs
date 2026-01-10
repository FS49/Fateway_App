using UnityEngine;

[CreateAssetMenu(menuName = "Fateway/Registries/First Crossroads Image Registry")]
public class FirstCrossroadsImageRegistry : ScriptableObject
{
    [System.Serializable]
    public class ImageEntry
    {
        public string characterName;
        public PassionColor color;
        public Gender gender;
        public Sprite safeImage;
        public Sprite riskImage;
    }

    public ImageEntry[] entries;

    public Sprite GetImage(string characterName, PassionColor color, Gender gender, bool isRisk)
    {
        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            if (entry.characterName == characterName && entry.color == color && entry.gender == gender)
            {
                return isRisk ? entry.riskImage : entry.safeImage;
            }
        }

        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            if (entry.color == color && entry.gender == gender)
            {
                return isRisk ? entry.riskImage : entry.safeImage;
            }
        }

        Debug.LogWarning($"[FirstCrossroadsImageRegistry] No image found for {characterName}/{color}/{gender}");
        return null;
    }
}
