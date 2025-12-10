using UnityEngine;
using UnityEngine.UI;

public class ScratchCard : MonoBehaviour
{
    public int brushSize = 30;          // Größe des „Radierpinsels“
    public Sprite sourceSprite;         // Ursprungs-Sprite (graues Bild)

    private Texture2D scratchTexture;   // Kopie der Texture, in die wir reinmalen
    private Image image;                // UI Image Komponente
    private RectTransform rectTransform;

    void Start()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();

        // Falls in der Inspector-Variable nichts drin ist, nimm das Sprite vom Image
        if (sourceSprite == null)
        {
            sourceSprite = image.sprite;
        }

        // Kopie der Texture erstellen
        Texture2D originalTex = sourceSprite.texture;
        scratchTexture = new Texture2D(originalTex.width, originalTex.height, TextureFormat.RGBA32, false);

        // Pixel kopieren
        scratchTexture.SetPixels(originalTex.GetPixels());
        scratchTexture.Apply();

        // Neue Sprite aus der Texture erstellen
        Rect spriteRect = new Rect(0, 0, scratchTexture.width, scratchTexture.height);
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        Sprite newSprite = Sprite.Create(scratchTexture, spriteRect, pivot);

        image.sprite = newSprite;
    }

    void Update()
    {
        if (Input.GetMouseButton(0)) // Linke Maustaste gedrückt halten
        {
            Vector2 localPoint;
            // Mausposition in lokale Koordinaten des RectTransforms umrechnen
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform,
                    Input.mousePosition,
                    null,             // null = Screen Space Overlay
                    out localPoint))
            {
                // Lokale Koordinate (in px) 0..width, 0..height
                Rect rect = rectTransform.rect;

                float normalizedX = (localPoint.x - rect.x) / rect.width;
                float normalizedY = (localPoint.y - rect.y) / rect.height;

                int texX = (int)(normalizedX * scratchTexture.width);
                int texY = (int)(normalizedY * scratchTexture.height);

                EraseCircle(texX, texY);
            }
        }
    }

    void EraseCircle(int centerX, int centerY)
    {
        int radius = brushSize;
        int rSquared = radius * radius;

        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                int px = centerX + x;
                int py = centerY + y;

                // Innerhalb der Texture?
                if (px >= 0 && px < scratchTexture.width && py >= 0 && py < scratchTexture.height)
                {
                    if (x * x + y * y <= rSquared)
                    {
                        Color c = scratchTexture.GetPixel(px, py);
                        c.a = 0f; // komplett durchsichtig
                        scratchTexture.SetPixel(px, py, c);
                    }
                }
            }
        }

        scratchTexture.Apply();
    }
}
