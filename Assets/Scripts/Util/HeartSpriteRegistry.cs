using System;
using UnityEngine;

public enum HeartSide
{
    Left,
    Right
}

[Serializable]
public class HeartSpriteEntry
{
    public PassionColor passion;
    public Gender gender;
    public HeartSide side;
    public Sprite sprite;
}

[CreateAssetMenu(menuName = "Fateway/UI/Heart Sprite Registry")]
public class HeartSpriteRegistry : ScriptableObject
{
    public HeartSpriteEntry[] entries;

    public Sprite GetSprite(PassionColor passion, Gender gender, HeartSide side)
    {
        if (entries == null) return null;

        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            if (entry != null && 
                entry.passion == passion && 
                entry.gender == gender && 
                entry.side == side)
            {
                return entry.sprite;
            }
        }

        Debug.LogWarning($"[HeartSpriteRegistry] No heart sprite found for {passion}/{gender}/{side}");
        return null;
    }

    public Sprite GetLeftHeart(PassionColor passion, Gender gender)
    {
        return GetSprite(passion, gender, HeartSide.Left);
    }

    public Sprite GetRightHeart(PassionColor passion, Gender gender)
    {
        return GetSprite(passion, gender, HeartSide.Right);
    }

    public Sprite GetLeftHeart(PlayerData player)
    {
        if (player == null) return null;
        return GetLeftHeart(player.passion, player.gender);
    }

    public Sprite GetRightHeart(PlayerData player)
    {
        if (player == null) return null;
        return GetRightHeart(player.passion, player.gender);
    }
}

