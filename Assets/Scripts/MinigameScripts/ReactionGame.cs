using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReactionGameManager : MonoBehaviour
{
    [Header("Spieleranzahl (2–6)")]
    [Range(2, 6)] public int playerCount = 2;

    [Header("OPTION A: Ein Canvas mit bis zu 6 Slots")]
    [Tooltip("Befülle bis zu 6 PlayerUI-Einträge. Der Manager nutzt die ersten N Einträge gemäß playerCount.")]
    public List<PlayerUI> playerSlots = new List<PlayerUI>(6);

    [Header("OPTION B: Mehrere Canvas-Layouts (index = playerCount-2)")]
    [Tooltip("Optional: Verschiedene Canvas-Layouts (2..6 Spieler). Lass leer, wenn du Option A nutzt.")]
    public Canvas[] layoutsByPlayerCount; // z.B. [Canvas_2, Canvas_3, ..., Canvas_6]

    [Header("Zentrale UI")]
    public Image centerLight;     // Das mittige Licht
    public TMP_Text statusText;   // Status/Anweisungen
    public TMP_Text winnerText;   // Gewinneranzeige

    [Header("Farben")]
    public Color waitingColor = Color.red;
    public Color getReadyColor = Color.yellow;
    public Color goColor = Color.green;

    [Header("Timing")]
    public Vector2 randomDelayRange = new Vector2(1.5f, 3.5f);
    public float resetDelay = 2f;          // Zeit bis neue Runde (falls auto-restart gewünscht)
    public bool autoNextRound = true;      // oder false, wenn ihr manuell neu startet

    private enum State { Idle, Waiting, Go, Result }
    private State state = State.Idle;

    private float greenTime;
    private bool roundFinished;

    private bool[] locked;          // Spieler temporär gesperrt
    public float lockDuration = 1.5f;


    // aktive Spieler dieser Runde (gekürzt auf playerCount)
    private List<PlayerUI> activePlayers = new List<PlayerUI>(6);
    private bool[] pressed; // pressed[i] = hat Spieler i bereits gedrückt

    void Start()
    {
        SetupLayoutAndPlayers();
        HookButtons();
        StartCoroutine(RoundLoop());
        locked = new bool[playerCount];

    }

    // Aktiviert passendes Canvas (Option B) ODER blendet Slots (Option A) aus
    void SetupLayoutAndPlayers()
    {

        // Sicherheitsnetz
        playerCount = Mathf.Clamp(playerCount, 2, 6);
        locked = new bool[playerCount];
        // OPTION B: Falls Layouts vorhanden → genau eines aktivieren
        if (layoutsByPlayerCount != null && layoutsByPlayerCount.Length >= (playerCount - 1))
        {
            for (int i = 0; i < layoutsByPlayerCount.Length; i++)
            {
                if (layoutsByPlayerCount[i] != null)
                    layoutsByPlayerCount[i].gameObject.SetActive(i == (playerCount - 2));
            }
        }

        // OPTION A: Aus der Liste die ersten N Slots aktivieren, Rest ausblenden
        activePlayers.Clear();
        for (int i = 0; i < playerSlots.Count; i++)
        {
            bool shouldBeActive = i < playerCount;
            if (playerSlots[i]?.button)
                playerSlots[i].button.gameObject.SetActive(shouldBeActive);
            if (playerSlots[i]?.label)
                playerSlots[i].label.gameObject.SetActive(shouldBeActive);

            if (shouldBeActive)
                activePlayers.Add(playerSlots[i]);
        }

        if (winnerText) winnerText.gameObject.SetActive(false);
        if (statusText) statusText.text = "Bereit... Nicht zu früh drücken!";
        if (centerLight) centerLight.color = waitingColor;

        pressed = new bool[playerCount];
    }

    void HookButtons()
    {
        // Alte Listener entfernen, neue setzen
        for (int i = 0; i < activePlayers.Count; i++)
        {
            int idx = i;
            var p = activePlayers[i];
            if (p?.button)
            {
                p.button.onClick.RemoveAllListeners();
                p.button.onClick.AddListener(() => OnPlayerPressed(idx));
            }
        }
    }

    IEnumerator RoundLoop()
    {
        while (true)
        {
            // Reset/Start Zustand
            for (int i = 0; i < pressed.Length; i++) pressed[i] = false;
            roundFinished = false;
            state = State.Waiting;

            if (winnerText) winnerText.gameObject.SetActive(false);
            if (statusText) statusText.text = "Bereit... Nicht zu früh drücken!";
            if (centerLight) centerLight.color = waitingColor;
            SetButtonsInteractable(true);

            // Optional: kurze Vorwarnung
            yield return new WaitForSeconds(Random.Range(0.4f, 0.9f));
            if (centerLight) centerLight.color = getReadyColor;
            if (statusText) statusText.text = "Gleich...";

            // Zufallswartezeit bis GO
            float wait = Random.Range(randomDelayRange.x, randomDelayRange.y);
            yield return new WaitForSeconds(wait);

            // GO!
            state = State.Go;
            if (centerLight) centerLight.color = goColor;
            if (statusText) statusText.text = "GRÜN! Klick!";

            greenTime = Time.time;

            // Tastaturtests (optional) ermöglichen (A,S,D,F,J,K,L z.B.)
            // → im Update abfragen wäre möglich; hier simpel im Loop:
            while (!roundFinished)
            {
                EditorKeyboardTest();
                yield return null;
            }

            if (!autoNextRound) yield break; // wenn keine Auto-Runde, hier stoppen

            yield return new WaitForSeconds(resetDelay);
            // Danach geht die Schleife weiter → neue Runde
        }
    }

    void OnPlayerPressed(int idx)
{
    if (roundFinished) return;

    // Spieler gesperrt? -> Ignorieren
    if (locked[idx]) return;

    switch (state)
    {
        case State.Waiting: // ROT/GELB → Fehlstart, aber Runde läuft weiter
            StartCoroutine(LockPlayer(idx));
            ShowFalseStartMsg(idx);
            break;

        case State.Go: // GRÜN → gültige Reaktion
            if (pressed[idx]) return; // hat schon gedrückt
            pressed[idx] = true;
            DeclareWinner(idx, Time.time - greenTime);
            break;
    }
}

IEnumerator LockPlayer(int idx)
{
    locked[idx] = true;

    // Optional: Button visuell deaktivieren
    if (activePlayers[idx].button)
        activePlayers[idx].button.interactable = false;

    yield return new WaitForSeconds(lockDuration);

    locked[idx] = false;

    // Button wieder aktivieren (aber nur wenn Runde noch aktiv)
    if (!roundFinished && activePlayers[idx].button)
        activePlayers[idx].button.interactable = true;
}

void ShowFalseStartMsg(int idx)
{
    string offender = GetPlayerName(idx);

    if (statusText)
        statusText.text = $"{offender} ist gesperrt! (Fehlstart)";

    // Licht bleibt ROT/GELB → Runde läuft weiter
}


    void DeclareFalseStart(int who)
    {
        roundFinished = true;
        state = State.Result;
        SetButtonsInteractable(false);

        string offender = GetPlayerName(who);
        string other = FirstOtherPlayerName(who);
        ShowWinnerText($"FEHLSTART {offender}!\n{other} gewinnt!");
    }

    void DeclareWinner(int who, float reactionTime)
    {
        roundFinished = true;
        state = State.Result;
        SetButtonsInteractable(false);

        string name = GetPlayerName(who);
        ShowWinnerText($"{name} gewinnt!\nReaktionszeit: {reactionTime:0.000}s");
    }

    void ShowWinnerText(string msg)
    {
        if (winnerText)
        {
            winnerText.gameObject.SetActive(true);
            winnerText.text = msg;
        }
    }

    string GetPlayerName(int idx)
    {
        var p = activePlayers[idx];
        if (p != null && p.label != null && !string.IsNullOrEmpty(p.label.text))
            return p.label.text;
        if (p != null && !string.IsNullOrEmpty(p.displayName))
            return p.displayName;
        return $"Player {idx + 1}";
    }

    string FirstOtherPlayerName(int offender)
    {
        // Falls 2 Spieler: der andere; bei >2: „Rest“
        if (playerCount == 2)
            return GetPlayerName(offender == 0 ? 1 : 0);
        return "Jemand anderes";
    }

    void SetButtonsInteractable(bool enable)
    {
        foreach (var p in activePlayers)
            if (p?.button) p.button.interactable = enable;
    }

    // Optional: Tastatur-Testing im Editor (weise Keys in PlayerUI.testKey zu)
    void EditorKeyboardTest()
    {
#if UNITY_EDITOR
        for (int i = 0; i < activePlayers.Count; i++)
        {
            var key = activePlayers[i].testKey;
            if (key != KeyCode.None && Input.GetKeyDown(key))
            {
                OnPlayerPressed(i);
            }
        }
#endif
    }

    // Falls du zur Laufzeit Spielerzahl wechseln willst:
    public void SetPlayerCount(int newCount)
    {
        playerCount = Mathf.Clamp(newCount, 2, 6);
        SetupLayoutAndPlayers();
        HookButtons();
    }

    // Manuell neue Runde starten (wenn autoNextRound = false)
    public void StartNewRound()
    {
        StopAllCoroutines();
        StartCoroutine(RoundLoop());
    }
}
