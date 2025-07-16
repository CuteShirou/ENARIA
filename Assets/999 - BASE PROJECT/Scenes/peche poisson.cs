using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pechepoisson : MonoBehaviour
{
    // Start is called before the first frame update
    public float x ;
    public float y;
    public float a;
    public float b;
    public int c;
    public float height;
    public float speed;
    public int uord;
    
    
    private int frameCounter = 0;
    public int updateRate = 30; // Nombre de frames entre chaque update
    void Start()
    {
        float a = Random.Range(50f, 300f);
        
        RectTransform rt = gameObject.GetComponent<RectTransform>();
        x = rt.anchoredPosition.x;
        y = rt.anchoredPosition.y;
        height = a;
        
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,height);
    }

    
    void UpdateEvery10Frames()
    {
        b = Random.Range(1f, 6f);
        c = Random.Range(1, 3);
        
       
    }
    // Update is called once per frame
    void Update()
    {
        
        frameCounter++;

        if (frameCounter >= updateRate)
        {
            frameCounter = 0;
            UpdateEvery10Frames();
        }
        
        RectTransform rt = gameObject.GetComponent<RectTransform>();
                
        speed = b; // Speed of movement*
        uord = c;
        if (uord == 1 && y >= -297+height/2)
        {
            y += speed;
            if (y > 303-height/2) y = 303-height/2;
        }
        if (uord == 2 && y <= 303-height/2)
        {
            y -= speed;
            if (y < -297+height/2) y = -297+height/2;
        }
        rt.anchoredPosition = new Vector2(x, y);
        
        
        
    }
    
}
