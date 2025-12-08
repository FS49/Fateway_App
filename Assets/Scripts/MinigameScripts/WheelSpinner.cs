using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class WheelSpinner : MonoBehaviour
{
    [Header("Wheel Setup")]
    [SerializeField] int segmentCount = 12;          // Anzahl Segmente
    [SerializeField] Transform wheel;                // Dein Rad (der Kreis)

    [Header("Spin Tuning")]
    [SerializeField] float minFullSpins = 3f;        // min. ganze Umdrehungen
    [SerializeField] float maxFullSpins = 6f;        // max. ganze Umdrehungen
    [SerializeField] float spinDuration = 3.5f;      // Sekunden
    [SerializeField] AnimationCurve ease = AnimationCurve.EaseInOut(0,0,1,1);
    [Tooltip("Kleiner Zufall innerhalb des Zielsegments (0..0.49)")]
    [Range(0f,0.49f)] public float segmentJitter = 0.2f;

    [Header("Optional: Gewichtete Wahrscheinlichkeiten")]
    [Tooltip("Leer lassen = alle Segmente gleich wahrscheinlich. Ansonsten Länge = segmentCount.")]
    [SerializeField] float[] weights;

    [Header("Events")]
    public UnityEvent onSpinStart;
    [System.Serializable] public class IntEvent : UnityEvent<int>{}
    public IntEvent onSpinComplete; // gibt gewonnenen Segmentindex (0..segmentCount-1) zurück

    bool isSpinning;

    // --- PUBLIC API ---

    /// <summary>Startet einen Spin mit Zufallsziel nach (optional) Gewichten.</summary>
    public void Spin()
    {
        if (isSpinning || wheel == null || segmentCount <= 0) return;
        int targetIndex = PickIndex();
        StartCoroutine(SpinRoutine(targetIndex));
    }

    /// <summary>Forciert einen Spin auf einen bestimmten Segmentindex.</summary>
    public void SpinToIndex(int targetIndex)
    {
        if (isSpinning || wheel == null) return;
        targetIndex = Mathf.Clamp(targetIndex, 0, segmentCount - 1);
        StartCoroutine(SpinRoutine(targetIndex));
    }

    // --- CORE ---

    IEnumerator SpinRoutine(int targetIndex)
    {
        isSpinning = true;
        onSpinStart?.Invoke();

        float segAngle = 360f / segmentCount;

        // Aktuellen Winkel (0..360) holen
        float startAngle = NormalizeAngle(wheel.localEulerAngles.z);
        float startMod   = startAngle % 360f;

        // Ziel innerhalb des gewünschten Segments (Mitte ± Jitter)
        float jitter = Random.Range(-segAngle * segmentJitter, segAngle * segmentJitter);
        float endMod = targetIndex * segAngle + segAngle * 0.5f + jitter;
        endMod = NormalizeAngle(endMod);

        // Wir wollen IMMER im Uhrzeigersinn (negativ auf Z) drehen.
        // Delta innerhalb [0..360): wie weit müssen wir von startMod nach endMod im CW?
        float cwDeltaMod = (startMod - endMod + 360f) % 360f;

        float fullSpins = Random.Range(minFullSpins, maxFullSpins);
        float totalDeltaCW = 360f * fullSpins + cwDeltaMod; // immer positiv

        float duration = Mathf.Max(0.05f, spinDuration);
        float t = 0f;

        // Zielwinkel in absoluter Form (CW = negativ auf Z)
        float targetAngle = startAngle - totalDeltaCW;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float k = ease.Evaluate(Mathf.Clamp01(t)); // 0..1
            float current = Mathf.Lerp(startAngle, targetAngle, k);
            wheel.localEulerAngles = new Vector3(wheel.localEulerAngles.x,
                                                 wheel.localEulerAngles.y,
                                                 current);
            yield return null;
        }

        // Winkel sauber „snappen“, um Floating-Point-Reste zu vermeiden
        float finalAngle = targetAngle;
        wheel.localEulerAngles = new Vector3(wheel.localEulerAngles.x,
                                             wheel.localEulerAngles.y,
                                             finalAngle);

        isSpinning = false;
        onSpinComplete?.Invoke(targetIndex);
    }

    // --- HELPERS ---

    int PickIndex()
    {
        if (weights == null || weights.Length != segmentCount)
            return Random.Range(0, segmentCount);

        // gewichtete Auswahl
        float sum = 0f;
        for (int i = 0; i < weights.Length; i++)
            sum += Mathf.Max(0f, weights[i]);

        if (sum <= 0f) return Random.Range(0, segmentCount);

        float r = Random.Range(0f, sum);
        float acc = 0f;
        for (int i = 0; i < segmentCount; i++)
        {
            acc += Mathf.Max(0f, weights[i]);
            if (r <= acc) return i;
        }
        return segmentCount - 1;
    }

    float NormalizeAngle(float a)
    {
        a %= 360f;
        if (a < 0f) a += 360f;
        return a;
    }
}
