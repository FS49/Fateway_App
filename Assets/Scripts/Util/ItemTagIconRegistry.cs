using UnityEngine;

[System.Serializable]
public class TagIconEntry
{
    public string tag;
    public Sprite icon;
}

[CreateAssetMenu(menuName = "Fateway/UI/Item Tag Icon Registry")]
public class ItemTagIconRegistry : ScriptableObject
{
    public TagIconEntry[] entries;

    public Sprite GetIcon(string tag)
    {
        if (entries == null) return null;
        foreach (var e in entries)
        {
            if (e != null && e.tag == tag)
                return e.icon;
        }
        return null;
    }
}