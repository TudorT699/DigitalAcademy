using UnityEngine;
using System.IO.Ports;
using UnityEngine.UI;

public class ArduinoUIButton : MonoBehaviour
{
    [Header("Buttons")]
    public Button button1; // Assign in Inspector
    public Button button2; // Assign in Inspector
    public Button buttonIntro;
    public Button buttonIntro2;
    public Button Question1_1;
    public Button Question1_1_selected;
    public Button Question1_2;
    public Button Question1_2_selected;

    public GameObject Answer1;
    public GameObject Answer2;

    public GameObject IntroPanel;
    public GameObject QuestionPanel1;
    public GameObject QuestionPanel2;


    SerialPort sp = new SerialPort("COM8", 9600); // CHANGE COM PORT

    void Start()
    {
        sp.Open();
        sp.ReadTimeout = 50;
    }

    void Update()
    {
        if (!sp.IsOpen) return;

        try
        {
            string data = sp.ReadLine();

            if (data == "BTN1")
            {
                button1.onClick.Invoke(); // trigger UI button
                if (IntroPanel.active)
                {
                    buttonIntro.onClick.Invoke();
                }
                if (QuestionPanel1.active && Answer1.active)
                {
                    Question1_1.onClick.Invoke();
                    if (!Answer1.active)
                    {
                        Question1_1_selected.onClick.Invoke();
                    }
                }
                
            }
            else if (data == "BTN2")
            {
                button2.onClick.Invoke();
                if (IntroPanel.active)
                {
                    buttonIntro2.onClick.Invoke();
                }
                if (QuestionPanel1.active && Answer2.active)
                {
                    Question1_2.onClick.Invoke();
                    if (!Answer2.active)
                    {
                        Question1_2_selected.onClick.Invoke();
                    }
                }
                
            }
        }
        catch { }
    }

    void OnApplicationQuit()
    {
        if (sp.IsOpen)
            sp.Close();
    }
}
