using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(menuName = "Fateway/Registries/Last Crossroads Animation Registry")]
public class LastCrossroadsAnimationRegistry : ScriptableObject
{
    [System.Serializable]
    public class AnimationEntry
    {
        public string characterName;
        public PassionColor color;
        public Gender gender;
        public VideoClip successClip;
        public VideoClip failedClip;
    }

    public AnimationEntry[] entries;

    public VideoClip GetClip(string characterName, PassionColor color, Gender gender, bool isSuccess)
    {
        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            if (entry.characterName == characterName && entry.color == color && entry.gender == gender)
            {
                return isSuccess ? entry.successClip : entry.failedClip;
            }
        }

        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            if (entry.color == color && entry.gender == gender)
            {
                return isSuccess ? entry.successClip : entry.failedClip;
            }
        }

        Debug.LogWarning($"[LastCrossroadsAnimationRegistry] No animation found for {characterName}/{color}/{gender}");
        return null;
    }
}
