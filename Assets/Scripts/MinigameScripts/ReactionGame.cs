using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReactionGameManager : MonoBehaviour
{
    [Range(2, 6)] public int playerCount = 2;
    public List<PlayerUI> players = new List<PlayerUI>(6);

    public Image    centerLight;
    public TMP_Text statusText;
    public TMP_Text winnerText;

    public Color waitingColor = Color.red;
    public Color getReadyColor = Color.yellow;
    public Color goColor      = Color.green;

    public Vector2 randomDelayRange = new Vector2(1.5f, 3.5f);
    public float   resetDelay   = 2f;
    public bool    autoNextRound = true;

    // <<<<<< HIER 2 Sekunden
    public float lockDuration = 2f;

    enum State { Waiting, Go, Result }
    State state;
    bool roundFinished;
    float greenTime;

    bool[] pressed;
    bool[] locked;

    void Start()
    {
        InitArrays();
        HookButtons();
        StartCoroutine(RoundLoop());
    }

    void InitArrays()
    {
        playerCount = Mathf.Clamp(playerCount, 2, Mathf.Max(2, players.Count));
        pressed = new bool[playerCount];
        locked  = new bool[playerCount];

        for (int i = 0; i < players.Count; i++)
        {
            bool active = i < playerCount && players[i].button != null;
            if (players[i].button) players[i].button.gameObject.SetActive(active);
            if (players[i].label)  players[i].label.gameObject.SetActive(active);

            // Auto-Refs für Lock
            if (active && players[i].button)
            {
                if (!players[i].canvasGroup)
                    players[i].canvasGroup = players[i].button.GetComponent<CanvasGroup>();
                if (!players[i].targetGraphic)
                    players[i].targetGraphic = players[i].button.targetGraphic;
            }
        }

        if (winnerText) winnerText.gameObject.SetActive(false);
        if (centerLight) centerLight.color = waitingColor;
        if (statusText)  statusText.text   = "Bereit... nicht zu früh drücken!";
    }

    void HookButtons()
    {
        for (int i = 0; i < playerCount; i++)
        {
            int idx = i;
            var btn = players[i].button;
            if (!btn) continue;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnPlayerPressed(idx));
        }
    }

    IEnumerator RoundLoop()
    {
        while (true)
        {
            for (int i = 0; i < playerCount; i++)
            {
                pressed[i] = false;
                locked[i]  = false;
                SetLocked(i, false);  // sicher alles entsperrt
            }

            roundFinished = false;
            state = State.Waiting;
            if (winnerText) winnerText.gameObject.SetActive(false);
            if (centerLight) centerLight.color = waitingColor;
            if (statusText)  statusText.text   = "Bereit... nicht zu früh drücken!";

            yield return new WaitForSeconds(Random.Range(0.4f, 0.9f));
            if (centerLight) centerLight.color = getReadyColor;
            if (statusText)  statusText.text   = "Gleich...";

            yield return new WaitForSeconds(Random.Range(randomDelayRange.x, randomDelayRange.y));
            state = State.Go;
            greenTime = Time.time;
            if (centerLight) centerLight.color = goColor;
            if (statusText)  statusText.text   = "GRÜN! Klick!";

            while (!roundFinished)
            {
#if UNITY_EDITOR
                for (int i = 0; i < playerCount; i++)
                {
                    var key = players[i].testKey;
                    if (key != KeyCode.None && Input.GetKeyDown(key)) OnPlayerPressed(i);
                }
#endif
                yield return null;
            }

            if (!autoNextRound) yield break;
            yield return new WaitForSeconds(resetDelay);
        }
    }

    void OnPlayerPressed(int idx)
    {
        if (roundFinished) return;

        // HARD-LOCK in der Logik (zusätzlich zur UI-Sperre)
        if (locked[idx]) return;

        if (state == State.Waiting)
        {
            // Fehlstart → NUR diesen Spieler 2s sperren
            StartCoroutine(LockPlayer(idx));
            if (statusText) statusText.text = $"{GetName(idx)} Fehlstart! ({lockDuration:0.0}s Sperre)";
            return;
        }

        if (state == State.Go)
        {
            if (pressed[idx]) return;
            pressed[idx] = true;

            roundFinished = true;
            state = State.Result;

            // alle sperren
            for (int i = 0; i < playerCount; i++) SetLocked(i, true);

            if (winnerText)
            {
                winnerText.gameObject.SetActive(true);
                winnerText.text = $"{GetName(idx)} gewinnt!\nReaktionszeit: {Time.time - greenTime:0.000}s";
            }
        }
    }

    IEnumerator LockPlayer(int idx)
    {
        locked[idx] = true;
        SetLocked(idx, true);
        yield return new WaitForSeconds(lockDuration);
        locked[idx] = false;

        // nur wieder freigeben, wenn Runde noch läuft
        if (state != State.Result)
            SetLocked(idx, false);
    }

    // <<< zentral: sperrt/entsperrt Button UND blockiert alle Pointer-Events
    void SetLocked(int idx, bool value)
    {
        var p = players[idx];

        // 1) Button-Interaktion
        if (p.button) p.button.interactable = !value;

        // 2) Raycasts blockieren/zulassen (sicherer als nur interactable)
        if (p.canvasGroup)
        {
            // bei Lock: keine Interaktion & keine Raycasts
            p.canvasGroup.interactable   = !value;
            p.canvasGroup.blocksRaycasts = !value;
            p.canvasGroup.alpha          = value ? 0.5f : 1f; // optisches Feedback
        }
        else
        {
            // Fallback: Raycast-Target direkt am Graphic togglen
            if (p.targetGraphic) p.targetGraphic.raycastTarget = !value;
            if (p.label)         p.label.alpha = value ? 0.5f : 1f;
        }
    }

    string GetName(int idx)
    {
        var p = players[idx];
        if (p.label && !string.IsNullOrEmpty(p.label.text)) return p.label.text;
        if (!string.IsNullOrEmpty(p.displayName))           return p.displayName;
        return $"Player {idx + 1}";
    }
}
