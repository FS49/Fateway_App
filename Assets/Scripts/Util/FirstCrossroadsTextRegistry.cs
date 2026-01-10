using UnityEngine;

[CreateAssetMenu(menuName = "Fateway/Registries/First Crossroads Text Registry")]
public class FirstCrossroadsTextRegistry : ScriptableObject
{
    [System.Serializable]
    public class TextEntry
    {
        public string characterName;
        public PassionColor color;
        public Gender gender;
        [TextArea] public string safeText;
        [TextArea] public string riskText;
    }

    public TextEntry[] entries;

    [Header("Fallback Text")]
    [TextArea] public string defaultSafeText = "You chose the safe path.";
    [TextArea] public string defaultRiskText = "You chose the risky path!";

    public string GetText(string characterName, PassionColor color, Gender gender, bool isRisk)
    {
        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            if (entry.characterName == characterName && entry.color == color && entry.gender == gender)
            {
                return isRisk ? entry.riskText : entry.safeText;
            }
        }

        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            if (entry.color == color && entry.gender == gender)
            {
                return isRisk ? entry.riskText : entry.safeText;
            }
        }

        return isRisk ? defaultRiskText : defaultSafeText;
    }
}
