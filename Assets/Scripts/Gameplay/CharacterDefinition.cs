using UnityEngine;

[CreateAssetMenu(menuName = "Game/CharacterDefinition")]
public class CharacterDefinition : ScriptableObject
{
    public string displayName;
    public Sprite portrait;
    public PassionColor passion;
    public Gender gender;
}
