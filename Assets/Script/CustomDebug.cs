using System.Collections.Generic;
using UnityEngine;

public class CustomDebug : MonoBehaviour
{
    private GUIStyle style;
    private Rect rect;
    private string text;

    static private List<string> listLog;

    private void Awake()
    {
        listLog = new List<string>();
        int h = Screen.height;

        style = new GUIStyle();
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 100;
        style.normal.textColor = Color.red;
    }

    static public void SendLog(string _log)
    {
        listLog.Add(_log);

        if(listLog.Count > 10)
            listLog.RemoveAt(0);
    }

    private void OnGUI()
    {
        text = string.Empty;

        foreach(string log in listLog)
        {
            text += log + "\n";
        }

        GUI.Label(rect, text, style);
    }
}
