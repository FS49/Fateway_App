using UnityEngine;

public class DiceController : MonoBehaviour
{
    public int minValue = 1;
    public int maxValue = 6;

    public int Roll()
    {
        int value = Random.Range(minValue, maxValue + 1);
        Debug.Log($"[Dice] Rolled: {value}");
        return value;
    }
}
