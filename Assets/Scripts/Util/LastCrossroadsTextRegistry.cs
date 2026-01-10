using UnityEngine;

[CreateAssetMenu(menuName = "Fateway/Registries/Last Crossroads Text Registry")]
public class LastCrossroadsTextRegistry : ScriptableObject
{
    [System.Serializable]
    public class TextEntry
    {
        public string characterName;
        public PassionColor color;
        public Gender gender;
        [TextArea] public string successText;
        [TextArea] public string failedText;
    }

    public TextEntry[] entries;

    [Header("Fallback Text")]
    [TextArea] public string defaultSuccessText = "Success! +10 points to your passion!";
    [TextArea] public string defaultFailedText = "Failed! Better luck next time.";

    public string GetText(string characterName, PassionColor color, Gender gender, bool isSuccess)
    {
        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            if (entry.characterName == characterName && entry.color == color && entry.gender == gender)
            {
                return isSuccess ? entry.successText : entry.failedText;
            }
        }

        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            if (entry.color == color && entry.gender == gender)
            {
                return isSuccess ? entry.successText : entry.failedText;
            }
        }

        return isSuccess ? defaultSuccessText : defaultFailedText;
    }
}
