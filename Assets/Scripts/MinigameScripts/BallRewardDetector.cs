using UnityEngine;

public class BallRewardDetector : MonoBehaviour
{
    public PlinkoRewardManager manager;
    bool rewarded;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (rewarded) return;

        var slot = other.GetComponent<RewardSlot>();
        if (slot != null)
        {
            rewarded = true;
            manager.GiveReward(slot.slotIndex);

            // optional: Ball stoppen
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }
    }
}
