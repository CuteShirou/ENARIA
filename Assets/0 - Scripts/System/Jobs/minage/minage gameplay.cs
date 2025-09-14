using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class minagegameplay : MonoBehaviour
{

    public GameObject pepite1;
    public GameObject pepite2;
    public GameObject pepite3;
    public GameObject pepite4;
    public GameObject pepite5;
    public GameObject pepite6;
    public GameObject pepite7;
    public GameObject pepite8;
    public GameObject pepite9;
    public GameObject pepite10;
    public GameObject pepite11;
    public GameObject pepite12;
    public GameObject pepite13;
    public GameObject pepite14;
    public GameObject pepite15;
    public GameObject timing;
    public float timingheight;
    
    
    public static float timerloose = 0f;
    public static bool isGameOver = false;
    
    // Start is called before the first frame update
    void Start()
    {
        RectTransform rtPepite1 = pepite1.GetComponent<RectTransform>();
        RectTransform rtPepite2 = pepite2.GetComponent<RectTransform>();
        RectTransform rtPepite3 = pepite3.GetComponent<RectTransform>();
        RectTransform rtPepite4 = pepite4.GetComponent<RectTransform>();
        RectTransform rtPepite5 = pepite5.GetComponent<RectTransform>();
        RectTransform rtPepite6 = pepite6.GetComponent<RectTransform>();
        RectTransform rtPepite7 = pepite7.GetComponent<RectTransform>();
        RectTransform rtPepite8 = pepite8.GetComponent<RectTransform>();
        RectTransform rtPepite9 = pepite9.GetComponent<RectTransform>();
        RectTransform rtPepite10 = pepite10.GetComponent<RectTransform>();
        RectTransform rtPepite11 = pepite11.GetComponent<RectTransform>();
        RectTransform rtPepite12 = pepite12.GetComponent<RectTransform>();
        RectTransform rtPepite13 = pepite13.GetComponent<RectTransform>();
        RectTransform rtPepite14 = pepite14.GetComponent<RectTransform>();
        RectTransform rtPepite15 = pepite15.GetComponent<RectTransform>();
        for (int i = 0; i < 15; i++)
        {
            int x = Random.Range(-600, 600);
            int y = Random.Range(-300, 300);
            int height = Random.Range(25, 100);
            int width = Random.Range(25, 100);
            if (i == 0)
            {
                rtPepite1.anchoredPosition = new Vector2(x, y);
                rtPepite1.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                rtPepite1.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }
            else if (i == 1)
            {
                rtPepite2.anchoredPosition = new Vector2(x, y);
                rtPepite2.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                rtPepite2.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }
            else if (i == 2)
            {
                rtPepite3.anchoredPosition = new Vector2(x, y);
                rtPepite3.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                rtPepite3.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }
            else if (i == 3)
            {
                rtPepite4.anchoredPosition = new Vector2(x, y);
                rtPepite4.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                rtPepite4.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }
            else if (i == 4)
            {
                rtPepite5.anchoredPosition = new Vector2(x, y);
                rtPepite5.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                rtPepite5.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }
            else if (i == 5)
            {
                rtPepite6.anchoredPosition = new Vector2(x, y);
                rtPepite6.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                rtPepite6.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }
            else if (i == 6)
            {
                rtPepite7.anchoredPosition = new Vector2(x, y);
                rtPepite7.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                rtPepite7.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }
            else if (i == 7)
            {
                rtPepite8.anchoredPosition = new Vector2(x, y);
                rtPepite8.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                rtPepite8.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }
            else if (i == 8)
            {
                rtPepite9.anchoredPosition = new Vector2(x, y);
                rtPepite9.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                rtPepite9.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }
            else if (i == 9)
            {
                rtPepite10.anchoredPosition = new Vector2(x, y);
                rtPepite10.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                rtPepite10.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }
            else if (i == 10)
            {
                rtPepite11.anchoredPosition = new Vector2(x, y);
                rtPepite11.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                rtPepite11.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }
            else if (i == 11)
            {
                rtPepite12.anchoredPosition = new Vector2(x, y);
                rtPepite12.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                rtPepite12.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }
            else if (i == 12)
            {
                rtPepite13.anchoredPosition = new Vector2(x, y);
                rtPepite13.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                rtPepite13.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }
            else if (i == 13)
            {
                rtPepite14.anchoredPosition = new Vector2(x, y);
                rtPepite14.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                rtPepite14.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }
            else if (i == 14)
            {
                rtPepite15.anchoredPosition = new Vector2(x, y);
                rtPepite15.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                rtPepite15.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        timerloose += Time.deltaTime;
        if (timerloose >= 15f)
        {
            isGameOver = true;
            timerloose = 0f;
        }
        RectTransform rtTiming = timing.GetComponent<RectTransform>();
        timingheight = (timerloose / 15)*600;
        if (timingheight>= 600) timingheight = 600; 
        rtTiming.anchoredPosition = new Vector2(rtTiming.anchoredPosition.x, -300 + timingheight / 2);
        rtTiming.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, timingheight);
        
    }
}
