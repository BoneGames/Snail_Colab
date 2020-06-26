using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Button : MonoBehaviour
{
    public Text totalText;
    public int total;
    public void ButtonPress(int numberChange)
    {
        total += numberChange;
        totalText.text = "Total: " + total;
    }

    public void Reset()
    {
        total = 0;
        totalText.text = "Total: " + total;
    }
}
