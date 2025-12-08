using UnityEngine;

public class WheelResultHandler : MonoBehaviour
{
    public void OnWheelResult(int index)
    {
        Debug.Log("Gewonnen: " + index);
    }
}
