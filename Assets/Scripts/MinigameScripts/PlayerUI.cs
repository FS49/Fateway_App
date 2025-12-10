using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class PlayerUI
{
    public string displayName = "Player";
    public Button button;        // Der runde Reaktionsbutton
    public TMP_Text label;       // Optionaler Name/Score etc.
    [Header("Editor-Testing (optional)")]
    public KeyCode testKey = KeyCode.None; // z.B. A, S, D, F... zum Testen am PC
}
