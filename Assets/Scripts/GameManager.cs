using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Video;

public class GameManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject startPanel;
    public GameObject gamePanel;
    public GameObject endPanel;
    public GameObject arduinoScript;

    [Header("Intro Pages (before starting game)")]
    public GameObject introPanel;
    public GameObject introPage1;
    public GameObject introPage2;

    [Header("Leaderboard")]
    public LeaderboardManager leaderboardManager;

    [Header("UI")]
    public Image emailImage;
    public TMP_Text timerText;
    public TMP_Text scoreText;

    [Header("Answer Panels (children inside GamePanel)")]
    public GameObject twoAnswerPanel; // the normal Legit/Phishing panel
    public GameObject kahootRound1Panel;
    public GameObject kahootRound2Panel;
    public GameObject kahootRound3Panel;
    public GameObject kahootRound4Panel;

    [Header("Kahoot Audio")]
    public AudioSource kahootAudioSource;

    [Header("Menu Audio")]
    public AudioSource menuAudioSource;
    public AudioClip menuStartClip; // the sound that plays on the INTRO panel

    [Header("Kahoot Videos (Rounds 1–4)")]
    public GameObject kahootVideoObjR1; // RawImage that shows video
    public GameObject kahootVideoObjR2;
    public GameObject kahootVideoObjR3;
    public GameObject kahootVideoObjR4;

    public VideoPlayer kahootVideoPlayerR1; // VideoPlayer for round 1
    public VideoPlayer kahootVideoPlayerR2;
    public VideoPlayer kahootVideoPlayerR3;
    public VideoPlayer kahootVideoPlayerR4;

    [Header("Video Placeholders (visible in Scene only)")]
    public GameObject placeholderR1;
    public GameObject placeholderR2;
    public GameObject placeholderR3;
    public GameObject placeholderR4;

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

        [Header("The 4 buttons for this round")]
        public Button buttonA;
        public Button buttonB;
        public Button buttonC;
        public Button buttonD;

        [Header("Correct Answer (ONLY ONE TRUE)")]
        public bool buttonAIsCorrect;
        public bool buttonBIsCorrect;
        public bool buttonCIsCorrect;
        public bool buttonDIsCorrect;

        public int GetCorrectIndex()
        {
            if (buttonAIsCorrect) return 0;
            if (buttonBIsCorrect) return 1;
            if (buttonCIsCorrect) return 2;
            if (buttonDIsCorrect) return 3;
            return -1; // none selected
        }

        public Button GetButtonByIndex(int idx)
        {
            switch (idx)
            {
                case 0: return buttonA;
                case 1: return buttonB;
                case 2: return buttonC;
                case 3: return buttonD;
                default: return null;
            }
        }
    }

    [Header("Rounds 1–4 (Kahoot)")]
    public List<KahootRoundData> kahootRounds = new List<KahootRoundData>();

    [Header("Rounds 5–10 (Normal)")]
    public List<NormalRoundData> normalRounds = new List<NormalRoundData>();

    private int currentRoundIndex = 0; // 0..9
    private float timer;
    private int score;
    private bool canAnswer;

    // holds which Kahoot round is active (0..3)
    private int currentKahootIndex = -1;

    // Intro pages
    private int introPageIndex = 0; // NEW (0 = page1, 1 = page2)

    void Start()
    {
        startPanel.SetActive(true);
        if (introPanel != null) introPanel.SetActive(false); // NEW
        gamePanel.SetActive(false);
        endPanel.SetActive(false);

        emailImage.preserveAspect = true;
        score = 0;

        HideAllAnswerPanels();
        HideAllKahootVideos(true); // show placeholders in menu

        if (menuAudioSource != null)
            menuAudioSource.Stop();
    }

    void Update()
    {
        if (!gamePanel.activeSelf || !canAnswer)
            return;

        timer -= Time.deltaTime;
        timer = Mathf.Max(timer, 0f);

        UpdateTimerUI();

        if (timer <= 0f)
        {
            canAnswer = false;
            NextRound();
        }
    }

    void UpdateTimerUI()
    {
        int totalSeconds = Mathf.CeilToInt(timer);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    public void OpenIntro()
    {
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
            // back from page 1 -> return to main menu
            if (introPanel != null) introPanel.SetActive(false);
            startPanel.SetActive(true);

            if (menuAudioSource != null && menuAudioSource.isPlaying)
                menuAudioSource.Stop();

            return;
        }

        // page 2 -> page 1
        introPageIndex = 0;
        ShowIntroPage(introPageIndex);
    }

    public void IntroNext()
    {
        if (introPageIndex == 0)
        {
            // page 1 -> page 2
            introPageIndex = 1;
            ShowIntroPage(introPageIndex);
            return;
        }

        // page 2 -> start game
        if (introPanel != null) introPanel.SetActive(false);
        StartDigitalGame();
    }

    void ShowIntroPage(int idx)
    {
        if (introPage1 != null) introPage1.SetActive(idx == 0);
        if (introPage2 != null) introPage2.SetActive(idx == 1);
    }

    public void StartDigitalGame()
    {
        // Stop menu audio when game starts
        if (menuAudioSource != null && menuAudioSource.isPlaying)
            menuAudioSource.Stop();

        // if intro is still open, close it
        if (introPanel != null) introPanel.SetActive(false);

        startPanel.SetActive(false);
        gamePanel.SetActive(true);
        endPanel.SetActive(false);

        score = 0;
        currentRoundIndex = 0;

        if (arduinoScript != null)
            arduinoScript.SetActive(true);

        LoadCurrentRound();
    }

    // NORMAL BUTTONS (Rounds 5–10)
    public void ChooseLegit()
    {
        if (!canAnswer) return;
        if (!IsNormalRound()) return;

        CheckNormalAnswer(true);
    }

    public void ChoosePhishing()
    {
        if (!canAnswer) return;
        if (!IsNormalRound()) return;

        CheckNormalAnswer(false);
    }

    void CheckNormalAnswer(bool playerChoice)
    {
        canAnswer = false;

        int normalIndex = currentRoundIndex - 4; // rounds 5-10 map to 0-5
        if (normalIndex >= 0 && normalIndex < normalRounds.Count)
        {
            if (playerChoice == normalRounds[normalIndex].isLegit)
                score++;
        }

        NextRound();
    }

    // KAHOOT BUTTONS (Rounds 1–4)
    void WireKahootButtons(KahootRoundData data)
    {
        ClearButton(data.buttonA);
        ClearButton(data.buttonB);
        ClearButton(data.buttonC);
        ClearButton(data.buttonD);

        int correct = data.GetCorrectIndex();

        AddKahootListener(data.buttonA, correct == 0);
        AddKahootListener(data.buttonB, correct == 1);
        AddKahootListener(data.buttonC, correct == 2);
        AddKahootListener(data.buttonD, correct == 3);
    }

    void AddKahootListener(Button b, bool isCorrect)
    {
        if (b == null) return;
        b.onClick.AddListener(() => KahootAnswer(isCorrect));
    }

    void ClearButton(Button b)
    {
        if (b == null) return;
        b.onClick.RemoveAllListeners();
    }

    void KahootAnswer(bool isCorrect)
    {
        if (!canAnswer) return;
        if (!IsKahootRound()) return;

        canAnswer = false;

        if (isCorrect)
            score++;

        NextRound();
    }

    // AUDIO
    void PlayKahootStartClip(AudioClip clip)
    {
        if (kahootAudioSource == null) return;
        if (clip == null) return;

        kahootAudioSource.Stop();
        kahootAudioSource.clip = clip;
        kahootAudioSource.Play();
    }

    // VIDEO HELPERS
    void HideAllKahootVideos(bool showPlaceholders)
    {
        if (kahootVideoObjR1 != null) kahootVideoObjR1.SetActive(false);
        if (kahootVideoObjR2 != null) kahootVideoObjR2.SetActive(false);
        if (kahootVideoObjR3 != null) kahootVideoObjR3.SetActive(false);
        if (kahootVideoObjR4 != null) kahootVideoObjR4.SetActive(false);

        if (kahootVideoPlayerR1 != null) kahootVideoPlayerR1.Stop();
        if (kahootVideoPlayerR2 != null) kahootVideoPlayerR2.Stop();
        if (kahootVideoPlayerR3 != null) kahootVideoPlayerR3.Stop();
        if (kahootVideoPlayerR4 != null) kahootVideoPlayerR4.Stop();

        if (placeholderR1 != null) placeholderR1.SetActive(showPlaceholders);
        if (placeholderR2 != null) placeholderR2.SetActive(showPlaceholders);
        if (placeholderR3 != null) placeholderR3.SetActive(showPlaceholders);
        if (placeholderR4 != null) placeholderR4.SetActive(showPlaceholders);
    }

    void PlayKahootVideoForRound(int kahootIndex) // 0..3
    {
        HideAllKahootVideos(false);

        if (kahootIndex == 0)
        {
            if (kahootVideoObjR1 != null) kahootVideoObjR1.SetActive(true);
            if (kahootVideoPlayerR1 != null) { kahootVideoPlayerR1.time = 0; kahootVideoPlayerR1.Play(); }
        }
        else if (kahootIndex == 1)
        {
            if (kahootVideoObjR2 != null) kahootVideoObjR2.SetActive(true);
            if (kahootVideoPlayerR2 != null) { kahootVideoPlayerR2.time = 0; kahootVideoPlayerR2.Play(); }
        }
        else if (kahootIndex == 2)
        {
            if (kahootVideoObjR3 != null) kahootVideoObjR3.SetActive(true);
            if (kahootVideoPlayerR3 != null) { kahootVideoPlayerR3.time = 0; kahootVideoPlayerR3.Play(); }
        }
        else if (kahootIndex == 3)
        {
            if (kahootVideoObjR4 != null) kahootVideoObjR4.SetActive(true);
            if (kahootVideoPlayerR4 != null) { kahootVideoPlayerR4.time = 0; kahootVideoPlayerR4.Play(); }
        }
    }

    // ROUND FLOW
    void LoadCurrentRound()
    {
        timer = 20f;
        canAnswer = true;
        UpdateTimerUI();

        if (IsKahootRound())
        {
            int kahootIndex = currentRoundIndex; // rounds 1-4 map to 0-3
            currentKahootIndex = kahootIndex;

            if (kahootIndex < 0 || kahootIndex >= kahootRounds.Count)
            {
                canAnswer = false;
                NextRound();
                return;
            }

            KahootRoundData data = kahootRounds[kahootIndex];

            emailImage.sprite = data.emailSprite;

            ShowKahootPanel(kahootIndex);

            PlayKahootVideoForRound(kahootIndex);

            PlayKahootStartClip(data.startClip);

            WireKahootButtons(data);
        }
        else
        {
            currentKahootIndex = -1;

            int normalIndex = currentRoundIndex - 4; // rounds 5-10 map to 0-5
            if (normalIndex < 0 || normalIndex >= normalRounds.Count)
            {
                canAnswer = false;
                NextRound();
                return;
            }

            emailImage.sprite = normalRounds[normalIndex].emailSprite;

            HideAllKahootVideos(false);

            ShowTwoAnswerPanel();
        }
    }

    void NextRound()
    {
        currentRoundIndex++;

        if (currentRoundIndex >= 10) // total rounds = 10
        {
            EndGame();
        }
        else
        {
            LoadCurrentRound();
        }
    }

    bool IsKahootRound()
    {
        return currentRoundIndex >= 0 && currentRoundIndex <= 3; // rounds 1-4
    }

    bool IsNormalRound()
    {
        return currentRoundIndex >= 4 && currentRoundIndex <= 9; // rounds 5-10
    }

    void EndGame()
    {
        HideAllKahootVideos(false);

        gamePanel.SetActive(false);
        endPanel.SetActive(true);

        scoreText.text = score + "/10";

        if (leaderboardManager != null)
            leaderboardManager.AddResult(score);
    }

    // PANEL SHOW/HIDE HELPERS
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

    void ShowKahootPanel(int kahootIndex) // 0..3
    {
        HideAllAnswerPanels();

        if (kahootIndex == 0 && kahootRound1Panel != null) kahootRound1Panel.SetActive(true);
        if (kahootIndex == 1 && kahootRound2Panel != null) kahootRound2Panel.SetActive(true);
        if (kahootIndex == 2 && kahootRound3Panel != null) kahootRound3Panel.SetActive(true);
        if (kahootIndex == 3 && kahootRound4Panel != null) kahootRound4Panel.SetActive(true);
    }
}