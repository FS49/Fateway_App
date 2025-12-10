using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class PlayerUI   // kein MonoBehaviour
{
    public string   displayName = "Player";
    public Button   button;              // runder Reaktions-Button
    public TMP_Text label;               // optionaler Name/Label
    public KeyCode  testKey = KeyCode.None; // optional (Editor-Test)

    // OPTIONAL: für harten Lock gegen Clicks
    public CanvasGroup canvasGroup;      // CanvasGroup am Button-Objekt
    public Graphic     targetGraphic;    // z.B. das Image des Buttons
}
