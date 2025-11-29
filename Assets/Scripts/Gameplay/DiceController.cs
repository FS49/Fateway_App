using UnityEngine;

public class DiceController : MonoBehaviour
{
    [Tooltip("Minimum dice value (inclusive).")]
    public int minValue = 1;

    [Tooltip("Maximum dice value (inclusive).")]
    public int maxValue = 6;

    public int Roll()
    {
        int value = Random.Range(minValue, maxValue + 1);
        Debug.Log($"[Dice] Rolled: {value}");
        return value;
    }
}