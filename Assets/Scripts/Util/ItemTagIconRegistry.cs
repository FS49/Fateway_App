using System.Collections.Generic;
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

    private Dictionary<string, Sprite> lookup;

    public Sprite GetIcon(string tag)
    {
        if (entries == null) return null;

        if (lookup == null)
        {
            lookup = new Dictionary<string, Sprite>(entries.Length);
            for (int i = 0; i < entries.Length; i++)
            {
                var e = entries[i];
                if (e != null && !string.IsNullOrEmpty(e.tag) && !lookup.ContainsKey(e.tag))
                    lookup[e.tag] = e.icon;
            }
        }

        return lookup.TryGetValue(tag, out var sprite) ? sprite : null;
    }

    private void OnEnable()
    {
        lookup = null;
    }
}
