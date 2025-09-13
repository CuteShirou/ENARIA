using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cerealesgameplay : MonoBehaviour
{
    public GameObject cerealeA1;
    public GameObject cerealeA2;
    public GameObject cerealeA3;
    public GameObject cerealeA4;
    public GameObject cerealeA5;
    public GameObject cerealeE1;
    public GameObject cerealeE2;
    public GameObject cerealeE3;
    public GameObject cerealeE4;
    public GameObject cerealeE5;
    public GameObject ATrigger;
    public GameObject ETrigger;

    public cerealesprogress cerealesprogressScript;

    public bool A1IsActive = false;
    public bool A2IsActive = false;
    public bool A3IsActive = false;
    public bool A4IsActive = false;
    public bool A5IsActive = false;
    public bool E1IsActive = false;
    public bool E2IsActive = false;
    public bool E3IsActive = false;
    public bool E4IsActive = false;
    public bool E5IsActive = false; 

    public int x;
    public float timer = 0f;
    public float updateInterval;
    public static bool reset = false;

    public float height;

    // Start is called before the first frame update
    void Start()
    {
        timer = 0;
        updateInterval = 0.5f;

    }

    // Update is called once per frame
    void Update()
    {
        RectTransform rtA1 = cerealeA1.GetComponent<RectTransform>();
        RectTransform rtA2 = cerealeA2.GetComponent<RectTransform>();
        RectTransform rtA3 = cerealeA3.GetComponent<RectTransform>();
        RectTransform rtA4 = cerealeA4.GetComponent<RectTransform>();
        RectTransform rtA5 = cerealeA5.GetComponent<RectTransform>();
        RectTransform rtE1 = cerealeE1.GetComponent<RectTransform>();
        RectTransform rtE2 = cerealeE2.GetComponent<RectTransform>();
        RectTransform rtE3 = cerealeE3.GetComponent<RectTransform>();
        RectTransform rtE4 = cerealeE4.GetComponent<RectTransform>();
        RectTransform rtE5 = cerealeE5.GetComponent<RectTransform>();
        RectTransform rtATrigger = ATrigger.GetComponent<RectTransform>();
        RectTransform rtETrigger = ETrigger.GetComponent<RectTransform>();

        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            UpdateEveryInterval();
        }

        if (x == 0 && !A1IsActive)
        {
            rtA1.anchoredPosition = new Vector2(rtA1.anchoredPosition.x, 300 );
            cerealeA1.GetComponent<cerealestarget>().GoDown();
            x = 10;
            A1IsActive = true;
        }
        else if (x == 1 && !A2IsActive)
        {
            rtA2.anchoredPosition = new Vector2(rtA2.anchoredPosition.x, 300 );
            cerealeA2.GetComponent<cerealestarget>().GoDown();
            x = 10;
            A2IsActive = true;
        }
        else if (x == 2 && !A3IsActive)
        {
            rtA3.anchoredPosition = new Vector2(rtA3.anchoredPosition.x, 300 );
            cerealeA3.GetComponent<cerealestarget>().GoDown();
            x = 10;
            A3IsActive = true;
        }
        else if (x == 3 && !A4IsActive)
        {
            rtA4.anchoredPosition = new Vector2(rtA4.anchoredPosition.x, 300 );
            cerealeA4.GetComponent<cerealestarget>().GoDown();
            x = 10;
            A4IsActive = true;
        }
        else if (x == 4 && !A5IsActive)
        {
            rtA5.anchoredPosition = new Vector2(rtA5.anchoredPosition.x, 300 );
            cerealeA5.GetComponent<cerealestarget>().GoDown();
            x = 10;
            A5IsActive = true;
        }
        else if (x == 5 && !E1IsActive)
        {
            rtE1.anchoredPosition = new Vector2(rtE1.anchoredPosition.x, 300 );
            cerealeE1.GetComponent<cerealestarget>().GoDown();
            x = 10;
            E1IsActive = true;
        }
        else if (x == 6 && !E2IsActive)
        {
            rtE2.anchoredPosition = new Vector2(rtE2.anchoredPosition.x, 300 );
            cerealeE2.GetComponent<cerealestarget>().GoDown();
            x = 10;
            E2IsActive = true;
        }
        else if (x == 7 && !E3IsActive)
        {
            rtE3.anchoredPosition = new Vector2(rtE3.anchoredPosition.x, 300 );
            cerealeE3.GetComponent<cerealestarget>().GoDown();
            x = 10;
            E3IsActive = true;
        }
        else if (x == 8 && !E4IsActive)
        {
            rtE4.anchoredPosition = new Vector2(rtE4.anchoredPosition.x, 300 );
            cerealeE4.GetComponent<cerealestarget>().GoDown();
            x = 10;
            E4IsActive = true;
        }
        else if (x == 9 && !E5IsActive)
        {
            rtE5.anchoredPosition = new Vector2(rtE5.anchoredPosition.x, 300 );
            cerealeE5.GetComponent<cerealestarget>().GoDown();
            x = 10;
            E5IsActive = true;
        }
        
        
        
        
        
        if (RectsOverlap( rtA1, rtATrigger) && Input.GetKeyDown(KeyCode.Q))
        {
            cerealesprogressScript.AugmenterHauteur(100f);
            cerealeA1.GetComponent<cerealestarget>().triggered();
            Debug.Log("Trigger A activated");
            A1IsActive = false;
        }
        else if (RectsOverlap(rtE1, rtETrigger) && Input.GetKeyDown(KeyCode.E))
        {
            cerealesprogressScript.AugmenterHauteur(100f);
            cerealeE1.GetComponent<cerealestarget>().triggered();
            Debug.Log("Trigger E activated");
            E1IsActive = false;
        }
        else if (RectsOverlap( rtA2, rtATrigger) && Input.GetKeyDown(KeyCode.Q))
        {
            cerealesprogressScript.AugmenterHauteur(100f);
            cerealeA2.GetComponent<cerealestarget>().triggered();
            Debug.Log("Trigger A activated");
            A2IsActive = false;
                
        }
        else if (RectsOverlap(rtE2, rtETrigger) && Input.GetKeyDown(KeyCode.E))
        {
            cerealesprogressScript.AugmenterHauteur(100f);
            cerealeE2.GetComponent<cerealestarget>().triggered();
            Debug.Log("Trigger E activated");
            E2IsActive = false;
        }
        else if (RectsOverlap( rtA3, rtATrigger) && Input.GetKeyDown(KeyCode.Q))
        {
            cerealesprogressScript.AugmenterHauteur(100f);
            cerealeA3.GetComponent<cerealestarget>().triggered();
            Debug.Log("Trigger A activated");
            A3IsActive = false;
                
        }
        else if (RectsOverlap(rtE3, rtETrigger) && Input.GetKeyDown(KeyCode.E))
        {
            cerealesprogressScript.AugmenterHauteur(100f);
            cerealeE3.GetComponent<cerealestarget>().triggered();
            Debug.Log("Trigger E activated");
            E3IsActive = false;
        }
        else if (RectsOverlap( rtA4, rtATrigger) && Input.GetKeyDown(KeyCode.Q))
        {
            cerealesprogressScript.AugmenterHauteur(100f);
            cerealeA4.GetComponent<cerealestarget>().triggered();
            Debug.Log("Trigger A activated");
            A4IsActive = false;
                
        }
        else if (RectsOverlap(rtE4, rtETrigger) && Input.GetKeyDown(KeyCode.E))
        {
            cerealesprogressScript.AugmenterHauteur(100f);
            cerealeE4.GetComponent<cerealestarget>().triggered();
            Debug.Log("Trigger E activated");
            E4IsActive = false;
        }
        else if (RectsOverlap( rtA5, rtATrigger) && Input.GetKeyDown(KeyCode.Q))
        {
            cerealesprogressScript.AugmenterHauteur(100f);
            cerealeA5.GetComponent<cerealestarget>().triggered();
            Debug.Log("Trigger A activated");
            A5IsActive = false;
                
        }
        else if (RectsOverlap(rtE5, rtETrigger) && Input.GetKeyDown(KeyCode.E))
        {
            cerealesprogressScript.AugmenterHauteur(100f);
            cerealeE5.GetComponent<cerealestarget>().triggered();
            Debug.Log("Trigger E activated");
            E5IsActive = false;
        }
        
        if (rtA1.anchoredPosition.y< -600 && A1IsActive)
        {
            A1IsActive = false;
        }
        if (rtA2.anchoredPosition.y < -600 && A2IsActive)
        {
            A2IsActive = false;
        }
        if (rtA3.anchoredPosition.y < -600 && A3IsActive)
        {
            A3IsActive = false;
        }
        if (rtA4.anchoredPosition.y < -600 && A4IsActive)
        {
            A4IsActive = false;
        }
        if (rtA5.anchoredPosition.y < -600 && A5IsActive)
        {
            A5IsActive = false;
        }
        if (rtE1.anchoredPosition.y < -600 && E1IsActive)
        {
            E1IsActive = false;
        }
        if (rtE2.anchoredPosition.y < -600 && E2IsActive)
        {
            E2IsActive = false;
        }
        if (rtE3.anchoredPosition.y < -600 && E3IsActive)
        {
            E3IsActive = false;
        }
        if (rtE4.anchoredPosition.y < -600 && E4IsActive)
        {
            E4IsActive = false;
        }
        if (rtE5.anchoredPosition.y < -600 && E5IsActive)
        {
            E5IsActive = false;
        }

        if (reset == true)
        {
            ResetMiniGame();
            reset = false;
        }
    }

    void UpdateEveryInterval()
    {
        x = Random.Range(0, 9);

    }

    void ResetMiniGame()
    {
        RectTransform rtA1 = cerealeA1.GetComponent<RectTransform>();
        RectTransform rtA2 = cerealeA2.GetComponent<RectTransform>();
        RectTransform rtA3 = cerealeA3.GetComponent<RectTransform>();
        RectTransform rtA4 = cerealeA4.GetComponent<RectTransform>();
        RectTransform rtA5 = cerealeA5.GetComponent<RectTransform>();
        RectTransform rtE1 = cerealeE1.GetComponent<RectTransform>();
        RectTransform rtE2 = cerealeE2.GetComponent<RectTransform>();
        RectTransform rtE3 = cerealeE3.GetComponent<RectTransform>();
        RectTransform rtE4 = cerealeE4.GetComponent<RectTransform>();
        RectTransform rtE5 = cerealeE5.GetComponent<RectTransform>();
        rtA1.anchoredPosition = new Vector2(rtA1.anchoredPosition.x, -2000);
        rtA2.anchoredPosition = new Vector2(rtA2.anchoredPosition.x, -2000);
        rtA3.anchoredPosition = new Vector2(rtA3.anchoredPosition.x, -2000);
        rtA4.anchoredPosition = new Vector2(rtA4.anchoredPosition.x, -2000);
        rtA5.anchoredPosition = new Vector2(rtA5.anchoredPosition.x, -2000);
        rtE1.anchoredPosition = new Vector2(rtE1.anchoredPosition.x, -2000);
        rtE2.anchoredPosition = new Vector2(rtE2.anchoredPosition.x, -2000);
        rtE3.anchoredPosition = new Vector2(rtE3.anchoredPosition.x, -2000);
        rtE4.anchoredPosition = new Vector2(rtE4.anchoredPosition.x, -2000);
        rtE5.anchoredPosition = new Vector2(rtE5.anchoredPosition.x, -2000);
        A1IsActive = false;
        A2IsActive = false;
        A3IsActive = false;
        A4IsActive = false;
        A5IsActive = false;
        E1IsActive = false;
        E2IsActive = false;
        E3IsActive = false;
        E4IsActive = false;
        E5IsActive = false;
        
        
    }

    bool RectsOverlap(RectTransform a, RectTransform b)
    {
        Vector3[] aCorners = new Vector3[4];
        Vector3[] bCorners = new Vector3[4];
        a.GetWorldCorners(aCorners);
        b.GetWorldCorners(bCorners);

        Rect rectA = new Rect(aCorners[0], aCorners[2] - aCorners[0]);
        Rect rectB = new Rect(bCorners[0], bCorners[2] - bCorners[0]);

        return rectA.Overlaps(rectB);
    }
}