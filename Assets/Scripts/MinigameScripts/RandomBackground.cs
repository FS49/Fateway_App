using UnityEngine;
using UnityEngine.UI;

public class RandomBackground : MonoBehaviour
{
    public Sprite[] backgrounds;      // Die 4 Hintergrundbilder
    public Image targetImage;         // Das UI-Image, das den BG anzeigen soll

    void Start()
    {
        if (backgrounds.Length == 0 || targetImage == null)
        {
            Debug.LogWarning("Kein Hintergrund oder kein TargetImage gesetzt!");
            return;
        }

        // Random auswählen
        int index = Random.Range(0, backgrounds.Length);
        targetImage.sprite = backgrounds[index];
    }
}

