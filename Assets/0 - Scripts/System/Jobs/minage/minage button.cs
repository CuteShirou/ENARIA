using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class minagebutton : MonoBehaviour
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
    public float x;
    public float y;
    public static bool Win = false;
    
    // Start is called before the first frame update
    void Start()
    {
        
        
    }


    public void onClick()
    {
        RectTransform rt = gameObject.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(-4000, -4000);
    }

    // Update is called once per frame
    void Update()
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
        
        if (rtPepite1.anchoredPosition.x == -4000 && rtPepite1.anchoredPosition.y == -4000 &&
            rtPepite2.anchoredPosition.x == -4000 && rtPepite2.anchoredPosition.y == -4000 &&
            rtPepite3.anchoredPosition.x == -4000 && rtPepite3.anchoredPosition.y == -4000 &&
            rtPepite4.anchoredPosition.x == -4000 && rtPepite4.anchoredPosition.y == -4000 &&
            rtPepite5.anchoredPosition.x == -4000 && rtPepite5.anchoredPosition.y == -4000 &&
            rtPepite6.anchoredPosition.x == -4000 && rtPepite6.anchoredPosition.y == -4000 &&
            rtPepite7.anchoredPosition.x == -4000 && rtPepite7.anchoredPosition.y == -4000 &&
            rtPepite8.anchoredPosition.x == -4000 && rtPepite8.anchoredPosition.y == -4000 &&
            rtPepite9.anchoredPosition.x == -4000 && rtPepite9.anchoredPosition.y == -4000 &&
            rtPepite10.anchoredPosition.x == -4000 && rtPepite10.anchoredPosition.y == -4000 &&
            rtPepite11.anchoredPosition.x == -4000 && rtPepite11.anchoredPosition.y == -4000 &&
            rtPepite12.anchoredPosition.x == -4000 && rtPepite12.anchoredPosition.y == -4000 &&
            rtPepite13.anchoredPosition.x == -4000 && rtPepite13.anchoredPosition.y == -4000 &&
            rtPepite14.anchoredPosition.x == -4000 && rtPepite14.anchoredPosition.y == -4000 &&
            rtPepite15.anchoredPosition.x == -4000 && rtPepite15.anchoredPosition.y == -4000)
        {
            Win = true;
            Debug.Log("réussite");
        }
        
        
    }
}
