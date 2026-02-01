using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartSetupPanelUI : MonoBehaviour
{
    private enum Step { PlayerCount, CharacterPick, Done }

    [Header("Refs")]
    public GameManager gameManager;

    [Header("Characters")]
    public List<CharacterDefinition> characters = new List<CharacterDefinition>();
    public bool uniqueCharacters = true;

    [Header("UI")]
    public GameObject root;        // Panel root (zum ein/ausblenden)
    public Text titleText;
    public Text hintText;

    [Header("Player Count UI")]
    public Text playerCountText;   // zeigt die eingegebene Zahl

    [Header("Character UI")]
    public Image portraitImage;
    public Text characterNameText;

    [Header("Limits")]
    public int minPlayers = 2;
    public int maxPlayers = 6;

    private Step step = Step.PlayerCount;

    private string typedCount = "";
    private int playerCount = 0;

    private int currentPicker = 0;          // 0-based: Spieler der gerade wählt
    private int currentCharIndex = 0;

    private readonly List<CharacterDefinition> chosen = new List<CharacterDefinition>();

    private void Awake()
    {
        if (root != null) root.SetActive(false);
    }

    public void Open()
    {
        if (root != null) root.SetActive(true);
        if (gameManager != null) gameManager.isSetupOpen = true;

        step = Step.PlayerCount;
        typedCount = "";
        playerCount = 0;
        currentPicker = 0;
        currentCharIndex = 0;
        chosen.Clear();

        RefreshUI();
    }

    public void Close()
    {
        if (root != null) root.SetActive(false);
        if (gameManager != null) gameManager.isSetupOpen = false;
    }

    private void Update()
    {
        if (root == null || !root.activeSelf) return;

        switch (step)
        {
            case Step.PlayerCount:
                HandlePlayerCountInput();
                break;

            case Step.CharacterPick:
                HandleCharacterPickInput();
                break;
        }
    }

    private void HandlePlayerCountInput()
    {
        // Ziffern lesen
        foreach (char c in Input.inputString)
        {
            if (char.IsDigit(c))
            {
                // nur 1-2 Stellen zulassen (z.B. bis 12), hier maxPlayers<=9 reicht 1 Stelle.
                typedCount += c;
                if (typedCount.Length > 2)
                    typedCount = typedCount.Substring(0, 2);

                RefreshUI();
            }
        }

        if (Input.GetKeyDown(KeyCode.Backspace) && typedCount.Length > 0)
        {
            typedCount = typedCount.Substring(0, typedCount.Length - 1);
            RefreshUI();
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (!int.TryParse(typedCount, out int value))
                value = 0;

            value = Mathf.Clamp(value, minPlayers, maxPlayers);

            if (value < minPlayers)
            {
                // bleibt im Step, UI sagt was
                playerCount = 0;
                RefreshUI();
                return;
            }

            playerCount = value;
            step = Step.CharacterPick;
            currentPicker = 0;
            chosen.Clear();

            // starte bei erstem erlaubten Charakter
            currentCharIndex = GetFirstAllowedCharacterIndex();
            RefreshUI();
        }

        // Escape = abbrechen
        if (Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    private void HandleCharacterPickInput()
    {
        if (characters == null || characters.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            StepCharacter(-1);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            StepCharacter(+1);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            var selected = characters[currentCharIndex];
            if (uniqueCharacters && chosen.Contains(selected))
            {
                // ignorieren, ist schon vergeben
                RefreshUI();
                return;
            }

            chosen.Add(selected);
            currentPicker++;

            if (currentPicker >= playerCount)
            {
                FinishAndStartGame();
                return;
            }

            // nächster Spieler wählt: setze Auswahl auf ersten freien Charakter
            currentCharIndex = GetFirstAllowedCharacterIndex();
            RefreshUI();
        }

        // Backspace = zurück zum vorherigen Spieler
        if (Input.GetKeyDown(KeyCode.Backspace) && chosen.Count > 0)
        {
            chosen.RemoveAt(chosen.Count - 1);
            currentPicker = Mathf.Max(0, currentPicker - 1);
            currentCharIndex = GetFirstAllowedCharacterIndex();
            RefreshUI();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    private void StepCharacter(int dir)
    {
        if (characters == null || characters.Count == 0) return;

        int tries = 0;
        int idx = currentCharIndex;

        do
        {
            idx = (idx + dir + characters.Count) % characters.Count;
            tries++;

            if (!uniqueCharacters || !chosen.Contains(characters[idx]))
            {
                currentCharIndex = idx;
                RefreshUI();
                return;
            }
        } while (tries <= characters.Count);

        // wenn alles belegt (bei unique+zu vielen Spielern), bleibt es einfach
        RefreshUI();
    }

    private int GetFirstAllowedCharacterIndex()
    {
        if (!uniqueCharacters) return 0;

        for (int i = 0; i < characters.Count; i++)
        {
            if (!chosen.Contains(characters[i]))
                return i;
        }
        return 0; // fallback
    }

    private void FinishAndStartGame()
    {
        if (gameManager == null) { Close(); return; }

        var newPlayers = new List<PlayerData>();
        for (int i = 0; i < chosen.Count; i++)
        {
            var c = chosen[i];
            newPlayers.Add(new PlayerData($"Player {i + 1}", c.passion, c.gender));
        }

        Close();

        // sauber neu starten
        gameManager.ApplySetupPlayersAndRestart(newPlayers);
    }

    private void RefreshUI()
    {
        if (titleText == null || hintText == null) return;

        if (step == Step.PlayerCount)
        {
            titleText.text = "Spieleranzahl";
            hintText.text = $"Gib eine Zahl ein ({minPlayers}-{maxPlayers}) und drücke ENTER.\nESC = Abbrechen";
            if (playerCountText != null)
                playerCountText.text = string.IsNullOrEmpty(typedCount) ? "_" : typedCount;

            // Character UI optional ausblenden (wenn du willst)
            if (portraitImage != null) portraitImage.enabled = false;
            if (characterNameText != null) characterNameText.text = "";
        }
        else if (step == Step.CharacterPick)
        {
            titleText.text = $"Spieler {currentPicker + 1}: Charakter wählen";

            string extra = uniqueCharacters ? " (einzigartig)" : "";
            hintText.text = $"Links/Rechts (A/D) wechseln • ENTER bestätigen • BACKSPACE zurück{extra}\nESC = Abbrechen";

            var c = (characters != null && characters.Count > 0) ? characters[currentCharIndex] : null;
            if (c != null)
            {
                if (portraitImage != null)
                {
                    portraitImage.enabled = true;
                    portraitImage.sprite = c.portrait;
                }
                if (characterNameText != null)
                    characterNameText.text = $"{c.displayName}  •  {c.passion}";
            }
        }
    }
}
