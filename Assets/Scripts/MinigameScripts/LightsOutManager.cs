using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class LightsOutManager : MonoBehaviour
{
    [Header("Gameplay")]
    [Range(1, 60)] public float roundDuration = 20f;
    public bool autoStart = true;
    public bool useTimer = false;             
    public bool avoidImmediateRepeat = true;  

    [Header("UI Bindings")]
    public List<LightButton> buttons = new List<LightButton>();
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;

    [Header("Result UI")] 
    public GameObject resultPopup;         
    public TextMeshProUGUI resultText;     
    [TextArea]
    public string resultTemplate = "Gratuliere! du bekommst {0} Punkte für deine Leidenschaft!";

    [Header("Start UI")]
public GameObject startPopup;   

    [Header("Colors")]
    public Color targetColor = Color.red;     
    public bool forceWarmPalette = true;       
    public List<Color> nonTargetPalette = new List<Color>   
    {
        new Color(1f, 0.85f, 0.20f),  // Gelb
        new Color(1f, 0.70f, 0.10f),  // Orange
        new Color(1f, 0.50f, 0.20f),  // Dunkel-Orange
        new Color(1f, 0.60f, 0.60f),  // Hellrosa
        new Color(1f, 0.40f, 0.40f),  // Rosa
        new Color(1f, 0.30f, 0.30f),  // Rot-nah
        new Color(1f, 0.20f, 0.20f)   // Tiefrosa/Rotton
    };

    private int currentTarget = -1;
    private int lastTarget = -1;
    private int score = 0;
    private float timeLeft;
    private bool playing = false;

    void Awake()
    {
        
        if (buttons == null || buttons.Count == 0)
        {
            buttons = GetComponentsInChildren<LightButton>(true).ToList();
            buttons.Sort((a, b) => a.index.CompareTo(b.index));
        }

        if (forceWarmPalette)
        {
            nonTargetPalette = new List<Color>
            {
                new Color(1f, 0.85f, 0.20f),
                new Color(1f, 0.70f, 0.10f),
                new Color(1f, 0.50f, 0.20f),
                new Color(1f, 0.60f, 0.60f),
                new Color(1f, 0.40f, 0.40f),
                new Color(1f, 0.30f, 0.30f),
                new Color(1f, 0.20f, 0.20f)
            };
        }

        
        nonTargetPalette = nonTargetPalette.Where(c => !ApproximatelyEqual(c, targetColor)).ToList();
        if (nonTargetPalette.Count == 0)
            nonTargetPalette = new List<Color> { new Color(1f, 0.8f, 0.2f), new Color(1f, 0.5f, 0.2f), new Color(1f, 0.4f, 0.4f) };
    }

    void Start()
{
    foreach (var b in buttons)
    {
        if (b == null) continue;
        b.Init(this);
        b.SetInteractable(false); 
    }

    if (resultPopup) resultPopup.SetActive(false);
    if (!autoStart && startPopup) startPopup.SetActive(true); 

    if (autoStart) StartRound();
    else UpdateUI();
}

    void Update()
    {
        if (!playing || !useTimer) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            EndRound();
        }
        UpdateUI();
    }

    public void StartRound()
{
    score = 0;
    timeLeft = roundDuration;
    playing = true;

    if (startPopup) startPopup.SetActive(false);  // <-- Popup weg
    if (resultPopup) resultPopup.SetActive(false);

    foreach (var b in buttons)
        if (b != null) b.SetInteractable(true);

    lastTarget = -1;
    AssignNewTargetAndColors();
    UpdateUI();
}

    public void EndRound()
    {
        playing = false;
        foreach (var b in buttons)
            if (b != null) b.SetInteractable(false);

        ShowResultPopup();
        UpdateUI();
    }

    public void OnButtonPressed(int idx)
    {
        if (!playing) return;

        if (idx == currentTarget)
        {
            score += 1;                 // richtig
            lastTarget = currentTarget;
            AssignNewTargetAndColors();
        }
        else
        {
            score -= 1;                 // falsch
        }

        UpdateUI();
    }

    // Popup bauen
    private void ShowResultPopup()
    {
        int reward = Mathf.FloorToInt(score / 5f); 
        if (resultText)
            resultText.text = string.Format(resultTemplate, reward);
        if (resultPopup)
            resultPopup.SetActive(true);
    }

    
    private void AssignNewTargetAndColors()
    {
        var valid = Enumerable.Range(0, buttons.Count).Where(IsValidIndex).ToList();
        if (valid.Count == 0) { currentTarget = -1; return; }

        var pool = valid;
        if (avoidImmediateRepeat && lastTarget != -1 && pool.Count > 1)
            pool = pool.Where(i => i != lastTarget).ToList();

        currentTarget = pool[Random.Range(0, pool.Count)];

        int palCount = nonTargetPalette.Count;
        int offset = Random.Range(0, palCount);

        for (int i = 0; i < buttons.Count; i++)
        {
            if (!IsValidIndex(i)) continue;
            if (i == currentTarget) continue;

            var col = nonTargetPalette[(offset + i) % palCount];
            if (ApproximatelyEqual(col, targetColor))
                col = nonTargetPalette[(offset + i + 1) % palCount];

            buttons[i].SetColor(col);
        }

        if (IsValidIndex(currentTarget))
            buttons[currentTarget].SetColor(targetColor);
    }

    private bool IsValidIndex(int idx)
    {
        return idx >= 0 && idx < buttons.Count && buttons[idx] != null;
    }

    private void UpdateUI()
    {
        if (timerText) timerText.text = useTimer ? Mathf.CeilToInt(timeLeft).ToString() : "";
        if (scoreText) scoreText.text = score.ToString();
    }

    private bool ApproximatelyEqual(Color a, Color b)
    {
        const float eps = 0.01f;
        return Mathf.Abs(a.r - b.r) < eps &&
               Mathf.Abs(a.g - b.g) < eps &&
               Mathf.Abs(a.b - b.b) < eps &&
               Mathf.Abs(a.a - b.a) < eps;
    }

    // UI Hooks
    public void UI_StartRound() { if (!playing) StartRound(); }
    public void UI_Restart()    { StartRound(); }
    public void UI_ClosePopup() { if (resultPopup) resultPopup.SetActive(false); }




    public void UI_ExitMinigame()
    {
    Time.timeScale = 1f;

    var gm = FindObjectOfType<GameManager>();
    if (gm != null)
        gm.ReturnFromMinigame();
    else
        Debug.LogError("[LightsOutManager] Kein GameManager gefunden.");
    }
}
