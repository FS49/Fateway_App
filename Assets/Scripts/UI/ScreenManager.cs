using UnityEngine;
using UnityEngine.Events;

public class ScreenManager : MonoBehaviour
{
    [Header("Screen References")]
    public GameObject screenStart;
    public GameObject screenCharacterSelection;
    public GameObject screenGame;
    public GameObject screenResults;
    public GameObject screenEnd;

    [Header("Game Manager")]
    public GameManager gameManager;

    [Header("Events")]
    public UnityEvent onStartScreenShown;
    public UnityEvent onCharacterSelectionShown;
    public UnityEvent onGameScreenShown;
    public UnityEvent onResultsScreenShown;
    public UnityEvent onEndScreenShown;

    public GameScreen CurrentScreen { get; private set; }

    private static ScreenManager instance;
    public static ScreenManager Instance => instance;

    private static GameScreen? lastScreenBeforeSceneReload = null;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();
    }

    private void Start()
    {

    if (lastScreenBeforeSceneReload.HasValue)
        ShowScreen(lastScreenBeforeSceneReload.Value);
    else
        ShowScreen(GameScreen.Start);
    }

    public void ShowScreen(GameScreen screen)
    {
        CurrentScreen = screen;
        lastScreenBeforeSceneReload = screen;

        if (screenStart != null)
            screenStart.SetActive(screen == GameScreen.Start);

        if (screenCharacterSelection != null)
            screenCharacterSelection.SetActive(screen == GameScreen.CharacterSelection);

        if (screenGame != null)
            screenGame.SetActive(screen == GameScreen.Game);

        if (screenResults != null)
            screenResults.SetActive(screen == GameScreen.Results);

        if (screenEnd != null)
            screenEnd.SetActive(screen == GameScreen.End);

        Debug.Log($"[ScreenManager] Switched to {screen} screen.");

        InvokeScreenEvent(screen);
    }

    private void InvokeScreenEvent(GameScreen screen)
    {
        switch (screen)
        {
            case GameScreen.Start:
                onStartScreenShown?.Invoke();
                break;
            case GameScreen.CharacterSelection:
                onCharacterSelectionShown?.Invoke();
                break;
            case GameScreen.Game:
                onGameScreenShown?.Invoke();
                break;
            case GameScreen.Results:
                onResultsScreenShown?.Invoke();
                break;
            case GameScreen.End:
                onEndScreenShown?.Invoke();
                break;
        }
    }

    public void ShowStartScreen() => ShowScreen(GameScreen.Start);
    public void ShowCharacterSelectionScreen() => ShowScreen(GameScreen.CharacterSelection);
    public void ShowGameScreen() => ShowScreen(GameScreen.Game);
    public void ShowResultsScreen() => ShowScreen(GameScreen.Results);
    public void ShowEndScreen() => ShowScreen(GameScreen.End);



    public void NextScreen()
    {
        switch (CurrentScreen)
        {
            case GameScreen.Start:
                ShowScreen(GameScreen.CharacterSelection);
                break;
            case GameScreen.CharacterSelection:
                ShowScreen(GameScreen.Game);
                break;
            case GameScreen.Game:
                ShowScreen(GameScreen.Results);
                break;
            case GameScreen.Results:
                ShowScreen(GameScreen.End);
                break;
            case GameScreen.End:
                ShowScreen(GameScreen.Start);
                break;
        }
    }

    public void HideGameScreen()
    {
    if (screenGame != null)
        screenGame.SetActive(false);
    }

    public void ShowGameScreenOnly()
    {
    if (screenGame != null)
        screenGame.SetActive(true);
    }

    public void RestartGame()
    {
        if (gameManager != null)
            gameManager.ResetGame();

        ShowScreen(GameScreen.CharacterSelection);
    }

    public void QuitGame()
    {
        Debug.Log("[ScreenManager] Quitting game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
