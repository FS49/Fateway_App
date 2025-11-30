using System;
using UnityEngine;

[Serializable]
public class PlayerAvatarEntry
{
    public PassionColor passion;
    public Gender gender;
    public Sprite[] frames;
}

[CreateAssetMenu(menuName = "Fateway/UI/Player Avatar Registry")]
public class PlayerAvatarRegistry : ScriptableObject
{
    public PlayerAvatarEntry[] entries;

    public Sprite[] GetFrames(PassionColor passion, Gender gender)
    {
        if (entries == null) return null;

        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            if (entry != null && entry.passion == passion && entry.gender == gender)
            {
                return entry.frames;
            }
        }

        Debug.LogWarning($"[PlayerAvatarRegistry] No avatar found for {passion}/{gender}");
        return null;
    }

    public Sprite GetFirstFrame(PassionColor passion, Gender gender)
    {
        var frames = GetFrames(passion, gender);
        return frames != null && frames.Length > 0 ? frames[0] : null;
    }
}

