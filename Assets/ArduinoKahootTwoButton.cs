using UnityEngine;
using UnityEngine.UI;
using System.IO.Ports;

public class ArduinoKahootTwoButton : MonoBehaviour
{
    [Header("Kahoot Round Panels")]
    public GameObject kahootRound1Panel; // Round 1 panel
    public GameObject kahootRound2Panel; // Round 2 panel
    public GameObject kahootRound3Panel; // Round 3 panel
    public GameObject kahootRound4Panel; // Round 4 panel

    [Header("Serial")]
    public string comPort = "COM8"; // Arduino COM port
    public int baudRate = 9600; // Arduino Serial.begin

    [Header("Debounce")]
    public float debounceSeconds = 0.20f; // Prevent double triggers

    // Button name constants
    private const string GOOD_NORMAL_NAME = "Kahoot Answer GOOD";
    private const string GOOD_SELECTED_NAME = "Kahoot Answer GOOD_SELECTED";
    private const string WRONG_NORMAL_NAME = "Kahoot Answer WRONG 1";
    private const string WRONG_SELECTED_NAME = "Kahoot Answer WRONG 1_SELECTED";

    // Serial
    private SerialPort sp; // Serial port object
    private float lastBTN1Time = -999f; // Debounce timer for BTN1
    private float lastBTN2Time = -999f; // Debounce timer for BTN2

    void Start()
    {
        sp = new SerialPort(comPort, baudRate); // Create serial connection
        sp.ReadTimeout = 50; // Avoid freezing when no data
        sp.NewLine = "\n";

        try
        {
            sp.Open(); // Open port
            Debug.Log($"Arduino connected on {comPort}"); // Debug confirmation
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Arduino serial open failed: {e.Message}"); // Print error if COM is wrong
        }
    }

    void Update()
    {
        if (sp == null || !sp.IsOpen) return; // If no serial, do nothing

        try
        {
            string data = sp.ReadLine(); // Read one line from Arduino
            if (string.IsNullOrWhiteSpace(data)) return; // Ignore empty lines
            data = data.Trim(); // remove \r and whitespace

            if (data == "BTN1") // Physical button 1
            {
                if (Time.time - lastBTN1Time < debounceSeconds) return; // Debounce
                lastBTN1Time = Time.time; // Save time of press
                HandleGood(); // BTN1 controls GOOD
            }
            else if (data == "BTN2") // Physical button 2
            {
                if (Time.time - lastBTN2Time < debounceSeconds) return; // Debounce
                lastBTN2Time = Time.time; // Save time of press
                HandleWrong(); // BTN2 controls WRONG
            }
        }
        catch
        {
        }
    }

    void HandleWrong()
    {
        if (!IsKahootActive()) return; // Only react when Kahoot panel is active

        Button wrongNormal = FindButtonByName(WRONG_NORMAL_NAME); // Find normal WRONG button
        Button wrongSelected = FindButtonByName(WRONG_SELECTED_NAME); // Find selected WRONG button

        // If selected version is visible, that means it's already selected -> confirm
        if (wrongSelected != null && wrongSelected.gameObject.activeInHierarchy)
        {
            wrongSelected.onClick.Invoke(); // CONFIRM wrong
            return;
        }

        // Otherwise select the normal button
        if (wrongNormal != null && wrongNormal.gameObject.activeInHierarchy)
        {
            wrongNormal.onClick.Invoke(); // SELECT wrong
            return;
        }

        Debug.LogWarning("WRONG buttons not found or not active in this Kahoot panel.");
    }

    void HandleGood()
    {
        if (!IsKahootActive()) return; // Only react when Kahoot panel is active

        Button goodNormal = FindButtonByName(GOOD_NORMAL_NAME); // Find normal GOOD button
        Button goodSelected = FindButtonByName(GOOD_SELECTED_NAME); // Find selected GOOD button

        // If selected version is visible, that means it's already selected -> confirm
        if (goodSelected != null && goodSelected.gameObject.activeInHierarchy)
        {
            goodSelected.onClick.Invoke(); // CONFIRM good
            return;
        }

        // Otherwise select the normal button
        if (goodNormal != null && goodNormal.gameObject.activeInHierarchy)
        {
            goodNormal.onClick.Invoke(); // SELECT good
            return;
        }

        Debug.LogWarning("GOOD buttons not found or not active in this Kahoot panel.");
    }

    bool IsKahootActive()
    {
        // Kahoot is active if ANY round panel is active 
        if (kahootRound1Panel != null && kahootRound1Panel.activeInHierarchy) return true;
        if (kahootRound2Panel != null && kahootRound2Panel.activeInHierarchy) return true;
        if (kahootRound3Panel != null && kahootRound3Panel.activeInHierarchy) return true;
        if (kahootRound4Panel != null && kahootRound4Panel.activeInHierarchy) return true;

        // If not, assume you always want it active.
        return true;
    }

    Button FindButtonByName(string exactName)
    {
        // Search only inside the CURRENT ACTIVE round panel to avoid duplicate-name conflicts
        GameObject activePanel = GetActiveKahootRoundPanel(); // get active round panel
        if (activePanel != null)
        {
            Transform[] allChildren = activePanel.GetComponentsInChildren<Transform>(true); // Include inactive
            for (int i = 0; i < allChildren.Length; i++) // Loop children
            {
                if (allChildren[i].name == exactName) // Name match
                {
                    return allChildren[i].GetComponent<Button>(); // Return Button component
                }
            }
            return null; // Not found
        }

        // Global search (if you forgot to assign the panels)
        GameObject obj = GameObject.Find(exactName); // Find by name in scene
        if (obj == null) return null; // Not found
        return obj.GetComponent<Button>(); // Return Button component
    }

    // helper to find which round panel is currently active
    GameObject GetActiveKahootRoundPanel()
    {
        if (kahootRound1Panel != null && kahootRound1Panel.activeInHierarchy) return kahootRound1Panel;
        if (kahootRound2Panel != null && kahootRound2Panel.activeInHierarchy) return kahootRound2Panel;
        if (kahootRound3Panel != null && kahootRound3Panel.activeInHierarchy) return kahootRound3Panel;
        if (kahootRound4Panel != null && kahootRound4Panel.activeInHierarchy) return kahootRound4Panel;
        return null;
    }

    void OnApplicationQuit()
    {
        if (sp != null && sp.IsOpen) sp.Close(); // Close serial on quit
    }
}