using UnityEngine;
using UnityEngine.UI;

public class PlayerAnimatedImage : MonoBehaviour
{
    public enum LoadMode
    {
        Registry,
        Resources
    }

    [Header("Display")]
    public Image targetImage;

    [Header("Load Mode")]
    public LoadMode loadMode = LoadMode.Registry;
    public PlayerAvatarRegistry avatarRegistry;

    [Header("Resources Mode Settings")]
    public string assetName = "avatar";

    [Header("Animation Settings")]
    public float frameRate = 12f;
    public bool playOnStart = true;
    public bool loop = true;

    private Sprite[] frames;
    private int currentFrame;
    private float frameTimer;
    private bool isPlaying;
    private PassionColor currentPassion;
    private Gender currentGender;
    private bool isInitialized;

    private void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();
    }

    private void Update()
    {
        if (!isPlaying || frames == null || frames.Length <= 1)
            return;

        frameTimer += Time.deltaTime;
        float frameInterval = 1f / frameRate;

        if (frameTimer >= frameInterval)
        {
            frameTimer -= frameInterval;
            currentFrame++;

            if (currentFrame >= frames.Length)
            {
                if (loop)
                {
                    currentFrame = 0;
                }
                else
                {
                    currentFrame = frames.Length - 1;
                    isPlaying = false;
                }
            }

            if (targetImage != null && currentFrame < frames.Length)
            {
                targetImage.sprite = frames[currentFrame];
            }
        }
    }

    public void Initialize(PlayerData player)
    {
        if (player == null)
        {
            Debug.LogWarning("[PlayerAnimatedImage] Initialize called with null player.");
            return;
        }
        Debug.Log($"[PlayerAnimatedImage] Initializing for {player.playerName}: {player.passion}/{player.gender}");
        Initialize(player.passion, player.gender);
    }

    public void Initialize(PassionColor passion, Gender gender)
    {
        currentPassion = passion;
        currentGender = gender;
        LoadAssets();
        isInitialized = true;

        Debug.Log($"[PlayerAnimatedImage] Loaded {(frames?.Length ?? 0)} frames for {passion}/{gender}");

        if (playOnStart && frames != null && frames.Length > 0)
            Play();
    }

    public void Refresh()
    {
        if (isInitialized)
            LoadAssets();
    }

    private void LoadAssets()
    {
        frames = null;

        switch (loadMode)
        {
            case LoadMode.Registry:
                LoadFromRegistry();
                break;
            case LoadMode.Resources:
                LoadFromResources();
                break;
        }

        ApplyFirstFrame();
    }

    private void LoadFromRegistry()
    {
        if (avatarRegistry == null)
        {
            Debug.Log("[PlayerAnimatedImage] avatarRegistry is null, trying Resources.Load...");
            avatarRegistry = Resources.Load<PlayerAvatarRegistry>("PlayerAvatarRegistry");
        }

        if (avatarRegistry == null)
        {
            Debug.LogWarning("[PlayerAnimatedImage] No avatar registry assigned and none found in Resources. Make sure PlayerAvatarRegistry.asset is in Assets/Resources/");
            return;
        }

        Debug.Log($"[PlayerAnimatedImage] Using registry: {avatarRegistry.name}");
        frames = avatarRegistry.GetFrames(currentPassion, currentGender);

        if (frames == null || frames.Length == 0)
        {
            Debug.LogWarning($"[PlayerAnimatedImage] Registry returned no frames for {currentPassion}/{currentGender}. Check registry entries.");
        }
    }

    private void LoadFromResources()
    {
        Sprite singleSprite = PlayerAssetPath.LoadSprite(currentPassion, currentGender, assetName);

        if (singleSprite != null)
        {
            frames = new Sprite[] { singleSprite };
        }
        else
        {
            frames = PlayerAssetPath.LoadAllSprites(currentPassion, currentGender);

            if (frames != null && frames.Length > 0)
            {
                System.Array.Sort(frames, (a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
            }
        }
    }

    private void ApplyFirstFrame()
    {
        if (frames != null && frames.Length > 0)
        {
            currentFrame = 0;
            if (targetImage != null)
            {
                targetImage.sprite = frames[0];
                targetImage.enabled = true;
            }
        }
        else
        {
            if (targetImage != null)
                targetImage.enabled = false;

            Debug.LogWarning($"[PlayerAnimatedImage] No sprites found for {currentPassion}/{currentGender}");
        }
    }

    public void Play()
    {
        if (frames == null || frames.Length == 0) return;

        isPlaying = true;
        frameTimer = 0f;
    }

    public void Pause()
    {
        isPlaying = false;
    }

    public void Stop()
    {
        isPlaying = false;
        currentFrame = 0;
        frameTimer = 0f;

        if (targetImage != null && frames != null && frames.Length > 0)
        {
            targetImage.sprite = frames[0];
        }
    }

    public void SetFrame(int index)
    {
        if (frames == null || frames.Length == 0) return;

        currentFrame = Mathf.Clamp(index, 0, frames.Length - 1);

        if (targetImage != null)
        {
            targetImage.sprite = frames[currentFrame];
        }
    }

    public bool IsPlaying => isPlaying;
    public int FrameCount => frames?.Length ?? 0;
    public int CurrentFrameIndex => currentFrame;
}
