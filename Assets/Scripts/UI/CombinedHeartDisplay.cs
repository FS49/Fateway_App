using UnityEngine;
using UnityEngine.UI;

public class CombinedHeartDisplay : MonoBehaviour
{
    [Header("Heart Images")]
    public Image leftHeartImage;
    public Image rightHeartImage;

    [Header("Registry")]
    public HeartSpriteRegistry heartRegistry;

    [Header("Optional Tinting")]
    public bool useTinting;

    private PlayerData leftPlayer;
    private PlayerData rightPlayer;

    public void Initialize(PlayerData firstPlayer, PlayerData secondPlayer)
    {
        leftPlayer = firstPlayer;
        rightPlayer = secondPlayer;
        UpdateDisplay();
    }

    public void Initialize(PlayerData firstPlayer, PlayerData secondPlayer, HeartSpriteRegistry registry)
    {
        heartRegistry = registry;
        Initialize(firstPlayer, secondPlayer);
    }

    public void Clear()
    {
        leftPlayer = null;
        rightPlayer = null;

        if (leftHeartImage != null)
        {
            leftHeartImage.sprite = null;
            leftHeartImage.enabled = false;
        }

        if (rightHeartImage != null)
        {
            rightHeartImage.sprite = null;
            rightHeartImage.enabled = false;
        }
    }

    private void UpdateDisplay()
    {
        if (heartRegistry == null)
        {
            Debug.LogWarning("[CombinedHeartDisplay] No heart registry assigned.");
            return;
        }

        if (leftHeartImage != null)
        {
            if (leftPlayer != null)
            {
                leftHeartImage.sprite = heartRegistry.GetLeftHeart(leftPlayer);
                leftHeartImage.enabled = leftHeartImage.sprite != null;

                if (useTinting)
                {
                    leftHeartImage.color = PassionColorUtils.GetColor(leftPlayer.passion);
                }
                else
                {
                    leftHeartImage.color = Color.white;
                }
            }
            else
            {
                leftHeartImage.sprite = null;
                leftHeartImage.enabled = false;
            }
        }

        if (rightHeartImage != null)
        {
            if (rightPlayer != null)
            {
                rightHeartImage.sprite = heartRegistry.GetRightHeart(rightPlayer);
                rightHeartImage.enabled = rightHeartImage.sprite != null;

                if (useTinting)
                {
                    rightHeartImage.color = PassionColorUtils.GetColor(rightPlayer.passion);
                }
                else
                {
                    rightHeartImage.color = Color.white;
                }
            }
            else
            {
                rightHeartImage.sprite = null;
                rightHeartImage.enabled = false;
            }
        }
    }

    public PlayerData LeftPlayer => leftPlayer;
    public PlayerData RightPlayer => rightPlayer;
    public bool HasBothPlayers => leftPlayer != null && rightPlayer != null;
}

