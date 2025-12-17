using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject startPanel;
    public GameObject gamePanel;
    public GameObject endPanel;

    [Header("UI")]
    public Image emailImage;
    public TMP_Text timerText;
    public TMP_Text scoreText;

    [Header("Email Data (EXACTLY 10)")]
    public List<EmailData> emails = new List<EmailData>();

    private List<EmailData> shuffledEmails = new List<EmailData>();
    private int currentEmailIndex = 0;

    private EmailData currentEmail;
    private float timer;
    private int score;
    private bool canAnswer;

    void Start()
    {
        startPanel.SetActive(true);
        gamePanel.SetActive(false);
        endPanel.SetActive(false);

        emailImage.preserveAspect = true;

        score = 0;
    }

    void Update()
    {
        if (!gamePanel.activeSelf || !canAnswer)
            return;

        timer -= Time.deltaTime;
        timerText.text = Mathf.Ceil(timer).ToString();

        if (timer <= 0f)
        {
            canAnswer = false;
            NextEmail();
        }
    }

    public void StartDigitalGame()
    {
        startPanel.SetActive(false);
        gamePanel.SetActive(true);
        endPanel.SetActive(false);

        PrepareEmails();
        LoadNextEmail();
    }

    void PrepareEmails()
    {
        shuffledEmails = new List<EmailData>(emails);
        ShuffleList(shuffledEmails);
        currentEmailIndex = 0;
    }

    void LoadNextEmail()
    {
        timer = 20f;
        canAnswer = true;

        currentEmail = shuffledEmails[currentEmailIndex];
        emailImage.sprite = currentEmail.emailSprite;
    }

    public void ChooseLegit()
    {
        CheckAnswer(true);
    }

    public void ChoosePhishing()
    {
        CheckAnswer(false);
    }

    void CheckAnswer(bool playerChoice)
    {
        if (!canAnswer)
            return;

        canAnswer = false;

        if (playerChoice == currentEmail.isLegit)
        {
            score++;
        }

        NextEmail();
    }

    void NextEmail()
    {
        currentEmailIndex++;

        if (currentEmailIndex >= shuffledEmails.Count)
        {
            EndGame();
        }
        else
        {
            LoadNextEmail();
        }
    }

    void EndGame()
    {
        gamePanel.SetActive(false);
        endPanel.SetActive(true);

        scoreText.text = score + "/10";
    }

    void ShuffleList(List<EmailData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            EmailData temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}