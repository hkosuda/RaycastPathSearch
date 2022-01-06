using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoText : MonoBehaviour
{
    Text text;

    void Start()
    {
        text = gameObject.GetComponent<Text>();
        text.text = "";

        Controller.SolverEnd += UpdateText;
    }

    void UpdateText(object obj, bool mute)
    {
        if (mute) { return; }

        string str = "";

        str += "Seed : " + Kernel.UsedSeed.ToString();
        str += "\n";
        str += "Time : " + (Controller.Sw.Elapsed.TotalMilliseconds * 1000).ToString() + " [us]";
        str += "\n";
        str += "Time : " + (Controller.Sw.Elapsed.TotalMilliseconds).ToString() + " [ms]";

        text.text = str;
    }
}
