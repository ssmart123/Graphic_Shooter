using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlickText : MonoBehaviour
{
    public Text text;
    private bool isBlinkOnOff = false;
    private int blickSpeed = 160;
    private float a = 0;

    private void Start()
    {
        a = 0;

        if (Time.timeScale <= 0)
            Time.timeScale = 1;
    }

    // Update is called once per frame
    void Update()
    {
        // 명암 증가값
        if (isBlinkOnOff == false)
            a += blickSpeed * Time.deltaTime;
        else
            a -= blickSpeed * Time.deltaTime;


        // 증가값 전환
        if (a > 255)
            isBlinkOnOff = true;
        else if (a < 0)
            isBlinkOnOff = false;

        text.color = new Color32(255, 255, 255, (byte)a);

    }
}
