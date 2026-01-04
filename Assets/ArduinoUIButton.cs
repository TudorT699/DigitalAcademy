using UnityEngine;
using System.IO.Ports;
using UnityEngine.UI;

public class ArduinoUIButton : MonoBehaviour
{
    public Button button1; // Assign in Inspector
    public Button button2; // Assign in Inspector

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
            }
            else if (data == "BTN2")
            {
                button2.onClick.Invoke();
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
