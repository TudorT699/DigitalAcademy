using UnityEngine;
using System.IO.Ports;
using UnityEngine.UI;

public class ArduinoUIButton : MonoBehaviour
{
    [Header("Buttons")]
     public Button buttonIntro1_1; // Assign in Inspector
     public Button buttonIntro2_1; // Assign in Inspector
     public Button buttonIntro1_2;
     public Button buttonIntro2_2;
     public Button Question1_1;
     public Button Question1_1_selected;
     public Button Question1_2;
     public Button Question1_2_selected;

     public GameObject GameManager;
     public GameObject IntroPanel;
     public GameObject QuestionPanel;


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
                 
                 if (IntroPanel.active)
                 {
                     buttonIntro1_1.onClick.Invoke();
                 }
                 if (QuestionPanel.active)
                 {
                    Question1_1.onClick.Invoke();
                 }
                 if (buttonA_Selected.gameObject.activeSelf)
                 {
                    KahootAnswer(true);
                 }

             }
             else if (data == "BTN2")
             {
                 
                 if (IntroPanel.active)
                 {
                     buttonIntro2_1.onClick.Invoke();
                 }
                if (QuestionPanel.active)
                {
                    Question1_2.onClick.Invoke();
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
