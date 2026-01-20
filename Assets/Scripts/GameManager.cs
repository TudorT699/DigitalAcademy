using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // input choice (so you know what is picked)
    public enum InputMode { Digital, Physical, Phone }

    [Header("Panels")]
    public GameObject startPanel; // MAIN MENU PANEL (Start Game / Settings / Quit)
    public GameObject introPanel;
    public GameObject introPage1;
    public GameObject introPage2;
    public GameObject gamePanel;
    public GameObject endPanel;
    public GameObject arduinoScript;

    // extra menu pages
    [Header("Start Flow Pages")]
    public GameObject chooseButtonsPanel; // Digital Buttons / Physical Buttons / PhoneButtons
    public GameObject usernamePanel; // Username input + confirm
    public GameObject settingsPanel; // Settings page with back button

    // username UI
    [Header("Username UI")]
    public TMP_InputField usernameInput;
    public TMP_Text usernameErrorText;

    // phone buttons script object (set active when Phone is chosen)
    [Header("Optional Input Scripts")]
    public GameObject phoneButtonsScript;

    // state
    private InputMode selectedInputMode = InputMode.Digital;
    private string playerUsername = "";

    [Header("Leaderboard")]
    public LeaderboardManager leaderboardManager;

    [Header("UI")]
    public Image emailImage;
    public TMP_Text timerText;
    public TMP_Text scoreText;

    [Header("Answer Panels (children inside GamePanel)")]
    public GameObject twoAnswerPanel;
    public GameObject kahootRound1Panel;
    public GameObject kahootRound2Panel;
    public GameObject kahootRound3Panel;
    public GameObject kahootRound4Panel;

    [Header("Kahoot Audio")]
    public AudioSource kahootAudioSource;

    [Header("Menu Audio (plays on INTRO panel)")]
    public AudioSource menuAudioSource;
    public AudioClip menuStartClip;

    // Round transition + end sounds
    [Header("Game Sounds")]
    public AudioSource gameSfxAudioSource;
    public AudioClip reachedRound5Clip; // Plays when Round 5 starts
    public AudioClip endPanelClip; // Plays when EndPanel shows

    [System.Serializable]
    public class NormalRoundData
    {
        public Sprite emailSprite;
        public bool isLegit;
    }

    [System.Serializable]
    public class KahootRoundData
    {
        public Sprite emailSprite;

        [Header("Audio (plays at start of this Kahoot round)")]
        public AudioClip startClip;

        [Header("Button pairs for this round (Normal + Selected)")]
        public Button buttonA_Normal;
        public Button buttonA_Selected;

        public Button buttonB_Normal;
        public Button buttonB_Selected;

        public Button buttonC_Normal;
        public Button buttonC_Selected;

        public Button buttonD_Normal;
        public Button buttonD_Selected;

        [Header("Correct Answer (ONLY ONE TRUE)")]
        public bool A_IsCorrect;
        public bool B_IsCorrect;
        public bool C_IsCorrect;
        public bool D_IsCorrect;

        // Mascot animator + audio assigned manually
        [Header("Mascot Feedback (manual clips per round)")]
        public Animator mascotAnimator; // Character_1 Animator
        public AudioSource mascotAudioSource; // Character_1 AudioSource
        public AudioClip correctSfx; // Sound for correct answer (round-specific)
        public AudioClip wrongSfx; // Sound for wrong answer (round-specific)

        public int GetCorrectIndex()
        {
            if (A_IsCorrect) return 0;
            if (B_IsCorrect) return 1;
            if (C_IsCorrect) return 2;
            if (D_IsCorrect) return 3;
            return -1;
        }
    }

    [Header("Rounds 1–4 (Kahoot)")]
    public List<KahootRoundData> kahootRounds = new List<KahootRoundData>();

    [Header("Rounds 5–10 (Normal)")]
    public List<NormalRoundData> normalRounds = new List<NormalRoundData>();

    private int currentRoundIndex = 0;
    private float timer;
    private int score;
    private bool canAnswer;

    private int introPageIndex = 0;

    // selection state for KAHOOT only
    private Button currentSelectedButton;
    private Button currentSelectedCorrespondingNormal;

    // lock while mascot feedback is playing
    private bool waitingForMascotFeedback = false;
    private Coroutine mascotCoroutine;

    // ensures the round 5 sound plays only once
    private bool playedRound5Sound = false;

    void Start()
    {
        // MAIN MENU ON START
        startPanel.SetActive(true);

        // extra pages are hidden at start
        if (chooseButtonsPanel != null) chooseButtonsPanel.SetActive(false);
        if (usernamePanel != null) usernamePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        if (introPanel != null) introPanel.SetActive(false);
        gamePanel.SetActive(false);
        endPanel.SetActive(false);

        emailImage.preserveAspect = true;
        score = 0;

        HideAllAnswerPanels();
        ResetSelectionState();

        waitingForMascotFeedback = false;

        // default input scripts off until player picks a mode
        ApplyInputMode(InputMode.Digital);

        // clear error text if used
        if (usernameErrorText != null) usernameErrorText.text = "";

        // stop menu audio until intro opens
        if (menuAudioSource != null)
            menuAudioSource.Stop();
    }

    void Update()
    {
        // block timer while mascot feedback plays
        if (!gamePanel.activeSelf || !canAnswer || waitingForMascotFeedback)
            return;

        timer -= Time.deltaTime;
        timer = Mathf.Max(timer, 0f);

        UpdateTimerUI();

        if (timer <= 0f)
        {
            canAnswer = false;
            NextRound(); // question switch on timeout
        }
    }

    void UpdateTimerUI()
    {
        int t = Mathf.CeilToInt(timer);
        timerText.text = $"{t / 60:00}:{t % 60:00}";
    }

    // MAIN MENU BUTTONS
    public void OnClickStartGameButton()
    {
        startPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        if (chooseButtonsPanel != null) chooseButtonsPanel.SetActive(true);
        if (usernamePanel != null) usernamePanel.SetActive(false);

        if (introPanel != null) introPanel.SetActive(false);
        gamePanel.SetActive(false);
        endPanel.SetActive(false);

        if (usernameErrorText != null) usernameErrorText.text = "";
    }

    public void OnClickSettingsButton()
    {
        startPanel.SetActive(false);

        if (chooseButtonsPanel != null) chooseButtonsPanel.SetActive(false);
        if (usernamePanel != null) usernamePanel.SetActive(false);

        if (settingsPanel != null) settingsPanel.SetActive(true);

        if (introPanel != null) introPanel.SetActive(false);
        gamePanel.SetActive(false);
        endPanel.SetActive(false);
    }

    public void OnClickQuitButton()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // SETTINGS BACK BUTTON
    public void OnClickSettingsBackButton()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        startPanel.SetActive(true);
    }

    // CHOOSE INPUT BUTTONS PAGE
    public void ChooseDigitalButtons()
    {
        selectedInputMode = InputMode.Digital;
        GoToUsernamePage();
    }

    public void ChoosePhysicalButtons()
    {
        selectedInputMode = InputMode.Physical;
        GoToUsernamePage();
    }

    public void ChoosePhoneButtons()
    {
        selectedInputMode = InputMode.Phone;
        GoToUsernamePage();
    }

    // BACK from choose buttons page to main menu
    public void OnClickChooseButtonsBackToMenu()
    {
        if (chooseButtonsPanel != null) chooseButtonsPanel.SetActive(false);
        startPanel.SetActive(true);
    }

    // username page navigation
    void GoToUsernamePage()
    {
        if (chooseButtonsPanel != null) chooseButtonsPanel.SetActive(false);
        if (usernamePanel != null) usernamePanel.SetActive(true);

        if (usernameInput != null) usernameInput.text = "";
        if (usernameErrorText != null) usernameErrorText.text = "";
    }

    public void OnClickUsernameBackToChooseButtons()
    {
        if (usernamePanel != null) usernamePanel.SetActive(false);
        if (chooseButtonsPanel != null) chooseButtonsPanel.SetActive(true);

        if (usernameErrorText != null) usernameErrorText.text = "";
    }

    // confirm username -> intro
    public void OnClickConfirmUsername()
    {
        string entered = (usernameInput != null) ? usernameInput.text : "";
        entered = entered.Trim();

        if (string.IsNullOrEmpty(entered))
        {
            if (usernameErrorText != null) usernameErrorText.text = "Please enter a username.";
            return;
        }

        playerUsername = entered;

        // Save username for leaderboard to use 
        PlayerPrefs.SetString("PLAYER_USERNAME", playerUsername);
        PlayerPrefs.SetInt("INPUT_MODE", (int)selectedInputMode);
        PlayerPrefs.Save();

        ApplyInputMode(selectedInputMode);

        if (usernamePanel != null) usernamePanel.SetActive(false);

        OpenIntro(); // goes to intro panel, then IntroNext starts game
    }

    // enable/disable input scripts based on mode
    void ApplyInputMode(InputMode mode)
    {
        // Arduino input only active for Physical
        if (arduinoScript != null) arduinoScript.SetActive(mode == InputMode.Physical);

        // phone buttons object
        if (phoneButtonsScript != null) phoneButtonsScript.SetActive(mode == InputMode.Phone);

        // Digital = both off
        if (mode == InputMode.Digital)
        {
            if (arduinoScript != null) arduinoScript.SetActive(false);
            if (phoneButtonsScript != null) phoneButtonsScript.SetActive(false);
        }
    }

    // KAHOOT SELECTION SYSTEM (Normal -> Selected -> Confirm)
    void ResetSelectionState()
    {
        currentSelectedButton = null;
        currentSelectedCorrespondingNormal = null;
    }

    void SelectButtonPair(Button normalBtn, Button selectedBtn)
    {
        if (normalBtn == null || selectedBtn == null) return;

        UnselectCurrent();

        SafeSetActive(normalBtn, false);
        SafeSetActive(selectedBtn, true);

        currentSelectedButton = selectedBtn;
        currentSelectedCorrespondingNormal = normalBtn;
    }

    void UnselectCurrent()
    {
        if (currentSelectedButton != null && currentSelectedCorrespondingNormal != null)
        {
            SafeSetActive(currentSelectedButton, false);
            SafeSetActive(currentSelectedCorrespondingNormal, true);
        }

        ResetSelectionState();
    }

    void SafeSetActive(Button b, bool active)
    {
        if (b != null && b.gameObject != null)
            b.gameObject.SetActive(active);
    }

    void ClearOnClick(Button b)
    {
        if (b == null) return;
        b.onClick.RemoveAllListeners();
    }

    void WirePair(Button normalBtn, Button selectedBtn, System.Action onConfirm, bool requireCanAnswer)
    {
        if (normalBtn == null || selectedBtn == null) return;

        ClearOnClick(normalBtn);
        ClearOnClick(selectedBtn);

        // 1st click = select
        normalBtn.onClick.AddListener(() =>
        {
            if (requireCanAnswer && (!canAnswer || waitingForMascotFeedback)) return;
            SelectButtonPair(normalBtn, selectedBtn);
        });

        // 2nd click = confirm
        selectedBtn.onClick.AddListener(() =>
        {
            if (requireCanAnswer && (!canAnswer || waitingForMascotFeedback)) return;
            onConfirm?.Invoke();
        });
    }

    // INTRO FLOW
    public void OpenIntro()
    {
        // ensure menu pages are hidden
        if (chooseButtonsPanel != null) chooseButtonsPanel.SetActive(false);
        if (usernamePanel != null) usernamePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        startPanel.SetActive(false);
        if (introPanel != null) introPanel.SetActive(true);
        gamePanel.SetActive(false);
        endPanel.SetActive(false);

        introPageIndex = 0;
        ShowIntroPage(introPageIndex);

        if (menuAudioSource != null && menuStartClip != null)
        {
            menuAudioSource.Stop();
            menuAudioSource.clip = menuStartClip;
            menuAudioSource.Play();
        }
    }

    public void IntroBack()
    {
        if (introPageIndex == 0)
        {
            if (introPanel != null) introPanel.SetActive(false);

            // go back to main menu
            startPanel.SetActive(true);

            if (chooseButtonsPanel != null) chooseButtonsPanel.SetActive(false);
            if (usernamePanel != null) usernamePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);

            if (menuAudioSource != null)
                menuAudioSource.Stop();

            return;
        }

        introPageIndex = 0;
        ShowIntroPage(introPageIndex);
    }

    public void IntroNext()
    {
        if (introPageIndex == 0)
        {
            introPageIndex = 1;
            ShowIntroPage(introPageIndex);
            return;
        }

        if (introPanel != null) introPanel.SetActive(false);
        StartDigitalGame();
    }

    void ShowIntroPage(int idx)
    {
        if (introPage1 != null) introPage1.SetActive(idx == 0);
        if (introPage2 != null) introPage2.SetActive(idx == 1);
    }

    // GAME START
    public void StartDigitalGame()
    {
        UnselectCurrent();
        StopMascotFeedbackIfRunning();

        if (menuAudioSource != null)
            menuAudioSource.Stop();

        startPanel.SetActive(false);
        if (introPanel != null) introPanel.SetActive(false);
        gamePanel.SetActive(true);
        endPanel.SetActive(false);

        score = 0;
        currentRoundIndex = 0;

        // reset this every time a new game starts
        playedRound5Sound = false;

        LoadCurrentRound();
    }

    // NORMAL ROUNDS (one-click)
    public void ChooseLegit() => CheckNormalAnswer(true);
    public void ChoosePhishing() => CheckNormalAnswer(false);

    void CheckNormalAnswer(bool choice)
    {
        if (!canAnswer || waitingForMascotFeedback || !IsNormalRound()) return;
        canAnswer = false;

        int idx = currentRoundIndex - 4;
        if (idx >= 0 && idx < normalRounds.Count && choice == normalRounds[idx].isLegit)
            score++;

        NextRound();
    }

    // KAHOOT (two-click: select then confirm)
    void WireKahootButtons(KahootRoundData data)
    {
        ForceKahootPairDefaults(data);

        int correct = data.GetCorrectIndex();

        WirePair(data.buttonA_Normal, data.buttonA_Selected, () => KahootAnswer(correct == 0), true);
        WirePair(data.buttonB_Normal, data.buttonB_Selected, () => KahootAnswer(correct == 1), true);
        WirePair(data.buttonC_Normal, data.buttonC_Selected, () => KahootAnswer(correct == 2), true);
        WirePair(data.buttonD_Normal, data.buttonD_Selected, () => KahootAnswer(correct == 3), true);
    }

    void ForceKahootPairDefaults(KahootRoundData d)
    {
        SafeSetActive(d.buttonA_Normal, true);
        SafeSetActive(d.buttonB_Normal, true);
        SafeSetActive(d.buttonC_Normal, true);
        SafeSetActive(d.buttonD_Normal, true);

        SafeSetActive(d.buttonA_Selected, false);
        SafeSetActive(d.buttonB_Selected, false);
        SafeSetActive(d.buttonC_Selected, false);
        SafeSetActive(d.buttonD_Selected, false);

        UnselectCurrent();
    }

    void KahootAnswer(bool correct)
    {
        if (!canAnswer || waitingForMascotFeedback || !IsKahootRound()) return;

        canAnswer = false;
        waitingForMascotFeedback = true;

        if (correct) score++;

        // remove selection visuals immediately
        UnselectCurrent();

        // play mascot correct/wrong animation + audio, then advance
        StopMascotFeedbackIfRunning();
        mascotCoroutine = StartCoroutine(PlayMascotFeedbackThenNext(correct));
    }

    IEnumerator PlayMascotFeedbackThenNext(bool wasCorrect)
    {
        KahootRoundData data = GetCurrentKahootData();

        // If missing setup, just go next safely
        if (data == null)
        {
            waitingForMascotFeedback = false;
            NextRound(); // (question switch after confirmation), happens after mascot animation + audio finishes
            yield break;
        }

        // trigger animator state
        if (data.mascotAnimator != null)
        {
            data.mascotAnimator.SetBool("Correct", false);
            data.mascotAnimator.SetBool("Wrong", false);

            data.mascotAnimator.SetBool("Correct", wasCorrect);
            data.mascotAnimator.SetBool("Wrong", !wasCorrect);
        }

        // play audio clip manually
        float waitTime = 0f;
        AudioClip clip = wasCorrect ? data.correctSfx : data.wrongSfx;

        if (data.mascotAudioSource != null && clip != null)
        {
            data.mascotAudioSource.Stop();
            data.mascotAudioSource.clip = clip;
            data.mascotAudioSource.Play();
            waitTime = clip.length;
        }

        // Wait for audio to finish (or 0 if missing)
        if (waitTime > 0f)
            yield return new WaitForSeconds(waitTime);
        else
            yield return null;

        // reset animator bools for clean next use
        if (data.mascotAnimator != null)
        {
            data.mascotAnimator.SetBool("Correct", false);
            data.mascotAnimator.SetBool("Wrong", false);
        }

        waitingForMascotFeedback = false;
        NextRound();
    }

    void StopMascotFeedbackIfRunning()
    {
        waitingForMascotFeedback = false;

        if (mascotCoroutine != null)
        {
            StopCoroutine(mascotCoroutine);
            mascotCoroutine = null;
        }

        // also stop any mascot audio currently playing
        KahootRoundData data = GetCurrentKahootData();
        if (data != null && data.mascotAudioSource != null)
            data.mascotAudioSource.Stop();
    }

    KahootRoundData GetCurrentKahootData()
    {
        if (currentRoundIndex < 0 || currentRoundIndex >= kahootRounds.Count) return null;
        return kahootRounds[currentRoundIndex];
    }

    // AUDIO (round start clip)
    void PlayKahootStartClip(AudioClip clip)
    {
        if (kahootAudioSource == null || clip == null) return;
        kahootAudioSource.Stop();
        kahootAudioSource.clip = clip;
        kahootAudioSource.Play();
    }

    void PlayGameSfx(AudioClip clip)
    {
        if (gameSfxAudioSource == null) return;
        if (clip == null) return;

        gameSfxAudioSource.PlayOneShot(clip);
    }

    // ROUND FLOW
    void LoadCurrentRound()
    {
        UnselectCurrent();
        StopMascotFeedbackIfRunning();

        timer = 90000f;
        canAnswer = true;
        UpdateTimerUI();

        // round 5 trigger (Round 5 = index 4)
        if (currentRoundIndex == 4 && !playedRound5Sound)
        {
            PlayGameSfx(reachedRound5Clip);
            playedRound5Sound = true;
        }

        if (IsKahootRound())
        {
            KahootRoundData data = kahootRounds[currentRoundIndex];
            emailImage.sprite = data.emailSprite;

            ShowKahootPanel(currentRoundIndex);
            PlayKahootStartClip(data.startClip);
            WireKahootButtons(data);

            // start clean
            if (data.mascotAnimator != null)
            {
                data.mascotAnimator.SetBool("Correct", false);
                data.mascotAnimator.SetBool("Wrong", false);
            }
        }
        else
        {
            int idx = currentRoundIndex - 4;
            emailImage.sprite = normalRounds[idx].emailSprite;
            ShowTwoAnswerPanel();
        }
    }

    void NextRound()
    {
        currentRoundIndex++; // this is the actual switch
        if (currentRoundIndex >= 10) EndGame();
        else LoadCurrentRound();
    }

    bool IsKahootRound() => currentRoundIndex <= 3;
    bool IsNormalRound() => currentRoundIndex >= 4;

    void EndGame()
    {
        UnselectCurrent();
        StopMascotFeedbackIfRunning();

        gamePanel.SetActive(false);
        endPanel.SetActive(true);

        // play end panel sound
        PlayGameSfx(endPanelClip);

        scoreText.text = score + "/10";

        if (leaderboardManager != null)
            leaderboardManager.AddResult(score, playerUsername); // use the typed username
    }

    // UI HELPERS
    void HideAllAnswerPanels()
    {
        if (twoAnswerPanel != null) twoAnswerPanel.SetActive(false);
        if (kahootRound1Panel != null) kahootRound1Panel.SetActive(false);
        if (kahootRound2Panel != null) kahootRound2Panel.SetActive(false);
        if (kahootRound3Panel != null) kahootRound3Panel.SetActive(false);
        if (kahootRound4Panel != null) kahootRound4Panel.SetActive(false);
    }

    void ShowTwoAnswerPanel()
    {
        HideAllAnswerPanels();
        if (twoAnswerPanel != null) twoAnswerPanel.SetActive(true);
    }

    void ShowKahootPanel(int idx)
    {
        HideAllAnswerPanels();
        if (idx == 0 && kahootRound1Panel != null) kahootRound1Panel.SetActive(true);
        if (idx == 1 && kahootRound2Panel != null) kahootRound2Panel.SetActive(true);
        if (idx == 2 && kahootRound3Panel != null) kahootRound3Panel.SetActive(true);
        if (idx == 3 && kahootRound4Panel != null) kahootRound4Panel.SetActive(true);
    }
}