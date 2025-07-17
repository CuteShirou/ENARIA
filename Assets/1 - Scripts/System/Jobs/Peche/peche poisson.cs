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
    
    
    public int _frameCounter = 0;
    public int updateRate ; // Nombre de frames entre chaque update
    void Start()
    {
        float a = Random.Range(50f, 300f);
        
        RectTransform rt = gameObject.GetComponent<RectTransform>();
        x = rt.anchoredPosition.x;
        y = rt.anchoredPosition.y;
        height = a;
        updateRate = 60;
        
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,height);
    }

    
    void UpdateEvery10Frames()
    {
        b = Random.Range(2f, 4f);
        c = Random.Range(1, 3);
        
       
    }
    // Update is called once per frame
    void Update()
    {
        
        _frameCounter++;

        if (_frameCounter >= updateRate)
        {
            _frameCounter = 0;
            UpdateEvery10Frames();
        }
        
        RectTransform rt = gameObject.GetComponent<RectTransform>();
                
        speed = b; // Speed of movement*
        uord = c;
        if (uord == 1 && y >= -300+height/2)
        {
            y += speed;
            if (y > 300-height/2) y = 300-height/2;
        }
        if (uord == 2 && y <= 300-height/2)
        {
            y -= speed;
            if (y < -300+height/2) y = -300+height/2;
        }
        rt.anchoredPosition = new Vector2(x, y);
        
        
        
    }
    
}
