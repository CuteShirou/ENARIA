using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class peche : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject progressBar; // Reference to the progress bar GameObject
    public float x ;
    public float y;
    public float speed;
    public RectTransform poissonRect; // Assigne le RectTransform du poisson dans l’inspecteur
    public pecheprogress progressBarScript;
    void Start()
    {
        RectTransform rt = gameObject.GetComponent<RectTransform>();
        x = rt.anchoredPosition.x;
        y = rt.anchoredPosition.y;
        speed = 3f; 
        rt.anchoredPosition = new Vector2(x, y);
    }

    // Update is called once per frame
    void Update()
    {
        RectTransform rt = gameObject.GetComponent<RectTransform>();
        
        
        
        if (Input.GetMouseButton(0) && y <= 229)
        {
            y -= speed;
            if (y < -227) y = -227; // Ne pas descendre sous 229
        }
        else if (!Input.GetMouseButton(0) && y  <= 229)
        {
            y += speed;
            if (y > 229) y = 229;
        }
        rt.anchoredPosition = new Vector2(x, y);
        
        if (RectsOverlap(rt, poissonRect))
        {
            progressBarScript.AugmenterHauteur(1f);
        }
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
