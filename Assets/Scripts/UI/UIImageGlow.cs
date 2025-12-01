using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIImageGlow : MonoBehaviour
{
    public Image image;
    public float pulseSpeed = 2f;
    public float minAlpha = 0.2f;
    public float maxAlpha = 0.8f;
    public bool playOnEnable = false;

    private bool isGlowing;
    private float baseAlpha;

    private void Awake()
    {
        if (image == null)
            image = GetComponent<Image>();

        if (image != null)
            baseAlpha = image.color.a;
    }

    private void OnEnable()
    {
        if (playOnEnable)
            StartGlow();
    }

    private void Update()
    {
        if (!isGlowing || image == null)
            return;

        float t = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f;
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);

        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }

    public void StartGlow()
    {
        if (image == null) return;

        isGlowing = true;

        Color c = image.color;
        if (c.a < minAlpha)
            c.a = minAlpha;
        image.color = c;
    }

    public void StopGlow(bool hideImage = false)
    {
        if (image == null) return;

        isGlowing = false;

        Color c = image.color;
        c.a = hideImage ? 0f : baseAlpha;
        image.color = c;
    }
}
