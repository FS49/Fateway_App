using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClickerGameManager : MonoBehaviour
{
    [Header("Game Rules")]
    public int targetLead = 15;   // 15 Unterschied = Sieg

    [Header("UI - Text (TMP)")]
    public TMP_Text leftScoreText;
    public TMP_Text rightScoreText;
    public TMP_Text infoText;

    [Header("Winner Display")]
    public TMP_Text winText;

    [Header("Dual Progress (links grün, rechts blau)")]
    public Image leftFill;   // Filled, Horizontal, Origin Left
    public Image rightFill;  // Filled, Horizontal, Origin Right

    [Header("Buttons (optional)")]
    public Button leftButton;
    public Button rightButton;

    [Header("Juice (optional)")]
    public RectTransform leftPulseTarget;
    public RectTransform rightPulseTarget;
    public float pulseScale = 1.08f;
    public float pulseTime = 0.06f;

    private int leftScore;
    private int rightScore;
    private bool isRunning;

    // --- Flat-Logik: aktuelle Mitte (0..1). 0.5 = Gleichstand, 1 = voll grün, 0 = voll blau
    private float currentLeft = 0.5f;
    private float Step => 0.5f / Mathf.Max(1, targetLead); // pro Klick

    void Start()
    {
        if (leftButton)  leftButton.onClick.AddListener(LeftClick);
        if (rightButton) rightButton.onClick.AddListener(RightClick);
        ResetGame();
        StartGame();
    }

    void Update()
    {
        if (!isRunning)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                ResetAndStart();
            return;
        }

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            LeftClick();

        if (Input.GetKeyDown(KeyCode.L) || Input.GetKeyDown(KeyCode.RightArrow))
            RightClick();
    }

    public void LeftClick()
    {
        if (!isRunning) return;
        leftScore++;
        Pulse(leftPulseTarget);
        // FLAT: + Step für grün, - Step für blau (spiegelt sich in rightFill automatisch)
        currentLeft = Mathf.Clamp01(currentLeft + Step);
        UpdateFillFromCurrent();
        CheckWinOrUpdate();
    }

    public void RightClick()
    {
        if (!isRunning) return;
        rightScore++;
        Pulse(rightPulseTarget);
        currentLeft = Mathf.Clamp01(currentLeft - Step);
        UpdateFillFromCurrent();
        CheckWinOrUpdate();
    }

    public void StartGame()
    {
        isRunning = true;
        SetInfo("GO!");
        UpdateUIInstant();
    }

    public void EndGame(string winnerLabel)
    {
        isRunning = false;

    // Bestehenden Info-Text verstecken
    if (infoText) infoText.gameObject.SetActive(false);

    // Gewinner-Text anzeigen
    if (winText)
    {
        winText.text = $"{winnerLabel} wins!";
        winText.gameObject.SetActive(true);
    }
    }

    public void ResetGame()
    {
    leftScore = 0;
    rightScore = 0;
    currentLeft = 0.5f;
    isRunning = false;

    // Info wieder sichtbar, WinText ausblenden
    if (infoText) infoText.gameObject.SetActive(true);
    if (winText) winText.gameObject.SetActive(false);

    SetInfo("Bereit? (↵ oder Space: Start)");
    UpdateUIInstant();
    }

    public void ResetAndStart()
    {
        ResetGame();
        StartGame();
    }

    private void CheckWinOrUpdate()
    {
        int diff = leftScore - rightScore;
        if (Mathf.Abs(diff) >= targetLead)
        {
            // Beim Sieg sofort auf 100% springen
            currentLeft = (diff > 0) ? 1f : 0f;
            UpdateFillFromCurrent();
            EndGame(diff > 0 ? "Player 1" : "Player 2");
        }
        else
        {
            UpdateScoreTexts();
        }
    }

    private void UpdateUIInstant()
    {
        UpdateScoreTexts();
        UpdateFillFromCurrent();
    }

    private void UpdateScoreTexts()
    {
        if (leftScoreText)  leftScoreText.text  = leftScore.ToString();
        if (rightScoreText) rightScoreText.text = rightScore.ToString();
    }

    private void UpdateFillFromCurrent()
    {
        if (leftFill)  leftFill.fillAmount  = currentLeft;
        if (rightFill) rightFill.fillAmount = 1f - currentLeft;
    }

    private void SetInfo(string msg)
    {
        if (infoText) infoText.text = msg;
    }

    private void Pulse(RectTransform target)
    {
        if (!target) return;
        StartCoroutine(DoPulse(target));
    }

    private System.Collections.IEnumerator DoPulse(RectTransform target)
    {
        Vector3 orig = target.localScale;
        Vector3 peak = orig * pulseScale;
        float t = 0f;

        while (t < pulseTime) { t += Time.deltaTime; target.localScale = Vector3.Lerp(orig, peak, t / pulseTime); yield return null; }
        t = 0f;
        while (t < pulseTime) { t += Time.deltaTime; target.localScale = Vector3.Lerp(peak, orig, t / pulseTime); yield return null; }

        target.localScale = orig;
    }
}
