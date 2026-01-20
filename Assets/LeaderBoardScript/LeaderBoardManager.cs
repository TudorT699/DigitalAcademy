using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class LeaderboardManager : MonoBehaviour
{
    [Header("Scrollable root (This moves when you scroll)")]
    public RectTransform rowsRoot; // parent of Row_1..Row_15

    [Header("Viewport (Visible window height)")]
    public RectTransform viewport;

    [Header("Rows in order (Top to Bottom)")]
    public List<GameObject> rowSlots = new List<GameObject>(); // Row_1 ... Row_15

    [Header("Scroll Settings")]
    public float scrollSpeed = 900f;
    public bool invertWheel = false;

    [Header("Settings")]
    public int maxEntries = 50;

    [Header("Rank Icons")]
    public bool useRankIcons = true;
    public Sprite goldIcon;   // Rank 1
    public Sprite silverIcon; // Rank 2
    public Sprite bronzeIcon; // Rank 3
    public Sprite normalIcon; // Rank 4+

    [Header("DEV")]
    public bool resetSavesOnPlay = false;

    private const string SAVE_KEY = "LEADERBOARD_SAVE_V2";

    [System.Serializable]
    public class Entry
    {
        public string name;
        public int score;
    }

    [System.Serializable]
    private class SaveData
    {
        public List<Entry> entries = new List<Entry>();
    }

    private List<Entry> entries = new List<Entry>();

    void Awake()
    {
        // If the checkbox is ticked, wipe saved leaderboard ONCE when entering Play Mode
        if (resetSavesOnPlay)
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerPrefs.Save();

            entries.Clear();

            // Reset scroll so you don't see empty space
            if (rowsRoot != null)
                rowsRoot.anchoredPosition = Vector2.zero;

            resetSavesOnPlay = false;
        }

        Load();
        RefreshRows();
    }

    void Update()
    {
        if (!gameObject.activeInHierarchy) return;
        if (rowsRoot == null) return;

        float wheel = Input.mouseScrollDelta.y;
        if (Mathf.Abs(wheel) < 0.001f) return;

        if (invertWheel) wheel *= -1f;

        Vector2 pos = rowsRoot.anchoredPosition;

        // Scroll up -> rows go up
        // Scroll down -> rows go down
        pos.y += wheel * scrollSpeed * Time.unscaledDeltaTime;

        rowsRoot.anchoredPosition = pos;
    }

    // if something calls AddResult(score) it will generate a username
    public void AddResult(int score)
    {
        AddResult(score, null);
    }

    // lets GameManager pass in the typed username
    public void AddResult(int score, string username)
    {
        // If username is missing, fallback to random name
        if (string.IsNullOrWhiteSpace(username))
            username = GenerateUsernameMax6();

        username = username.Trim();

        entries.Add(new Entry { name = username, score = score });

        entries = entries
            .OrderByDescending(e => e.score)
            .ThenBy(e => e.name)
            .Take(maxEntries)
            .ToList();

        Save();
        RefreshRows();
    }

    public void RefreshRows()
    {
        if (rowSlots == null || rowSlots.Count == 0)
            return;

        // Hide all rows
        for (int i = 0; i < rowSlots.Count; i++)
        {
            if (rowSlots[i] != null)
                rowSlots[i].SetActive(false);
        }

        // Show rows we have data for
        int count = Mathf.Min(entries.Count, rowSlots.Count);

        for (int i = 0; i < count; i++)
        {
            GameObject row = rowSlots[i];
            if (row == null) continue;

            row.SetActive(true);

            // Rank text only for Top 3
            bool isTop3 = (i < 3);

            SetRankTextVisible(row, isTop3);

            // Set rank text content
            if (isTop3)
                SetText(row, "RankText", (i + 1).ToString());
            else
                SetText(row, "RankText", ""); // keeps it empty for 4+

            // Rank icon (Image) for Top 3 (and normal for 4+ if assigned)
            SetRankIcon(row, i);

            // Other texts
            SetText(row, "NameText", entries[i].name);
            SetText(row, "ScoreText", $"{entries[i].score}/10");
        }

        Canvas.ForceUpdateCanvases();
    }

    void SetRankTextVisible(GameObject row, bool visible)
    {
        Transform t = FindDeepChild(row.transform, "RankText");
        if (t == null) return;

        TMP_Text tmp = t.GetComponent<TMP_Text>();
        if (tmp != null)
        {
            tmp.enabled = visible; // hides it completely for ranks 4+
        }
    }

    void SetRankIcon(GameObject row, int index)
    {
        // Looks for child named "RankIcon" with an Image component
        Transform t = FindDeepChild(row.transform, "RankIcon");
        if (t == null) return;

        Image img = t.GetComponent<Image>();
        if (img == null) return;

        if (!useRankIcons)
        {
            img.enabled = false;
            return;
        }

        // Top 3 get special icons
        if (index == 0 && goldIcon != null)
        {
            img.enabled = true;
            img.sprite = goldIcon;
        }
        else if (index == 1 && silverIcon != null)
        {
            img.enabled = true;
            img.sprite = silverIcon;
        }
        else if (index == 2 && bronzeIcon != null)
        {
            img.enabled = true;
            img.sprite = bronzeIcon;
        }
        else
        {
            // Rank 4+ -> use normalIcon if provided, otherwise hide
            if (normalIcon != null)
            {
                img.enabled = true;
                img.sprite = normalIcon;
            }
            else
            {
                img.enabled = false;
                img.sprite = null;
            }
        }
    }

    void SetText(GameObject row, string childName, string value)
    {
        Transform t = FindDeepChild(row.transform, childName);
        if (t == null) return;

        TMP_Text tmp = t.GetComponent<TMP_Text>();
        if (tmp != null) tmp.text = value;
    }

    Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform found = FindDeepChild(child, name);
            if (found != null) return found;
        }
        return null;
    }

    string GenerateUsernameMax6()
    {
        string[] a = { "Cozy", "Tiny", "Swift", "Happy", "Chill", "Nova", "Pixel", "Sunny", "Fuzzy", "Mossy" };
        string[] b = { "Wiz", "Fox", "Owl", "Cat", "Bee", "Arc", "Run", "Leaf", "Wolf", "Duke" };

        string baseName = a[Random.Range(0, a.Length)] + b[Random.Range(0, b.Length)];
        baseName = baseName.Replace(" ", "");

        if (baseName.Length > 6) baseName = baseName.Substring(0, 6);

        if (baseName.Length < 6)
        {
            int num = Random.Range(0, 999);
            baseName = (baseName + num.ToString()).Substring(0, 6);
        }

        return baseName;
    }

    void Save()
    {
        SaveData data = new SaveData { entries = entries };
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    void Load()
    {
        entries.Clear();

        if (!PlayerPrefs.HasKey(SAVE_KEY))
            return;

        string json = PlayerPrefs.GetString(SAVE_KEY);
        if (string.IsNullOrEmpty(json))
            return;

        SaveData data = JsonUtility.FromJson<SaveData>(json);
        if (data != null && data.entries != null)
            entries = data.entries;
    }
}