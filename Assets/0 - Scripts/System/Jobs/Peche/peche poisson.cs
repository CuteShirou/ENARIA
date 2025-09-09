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
	public float d;
    public float height;
    public float speed;
    public int uord;
    public GameObject timing;
    public float timingheight;
    
    
   	public float timer = 0f;
    public static float timerloose = 0f;
    public float updateInterval; 
    public static bool isGameOver = false;


    void Start()
    {
        float a = Random.Range(50f, 300f);
        
        RectTransform rt = gameObject.GetComponent<RectTransform>();
        x = rt.anchoredPosition.x;
        y = rt.anchoredPosition.y;
        height = a;
        updateInterval = 0.5f;
        
        Vector3 scale = transform.localScale;
        scale.y = height;
        transform.localScale = scale;
    }

    
    void UpdateEveryInterval()
    {
        b = Random.Range(200f, 500f);
        c = Random.Range(1, 3);
		d = Random.Range(0.3f,1.3f);
        
       
    }
    // Update is called once per frame
    void Update()
    {
        
        timer += Time.deltaTime;
        timerloose += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            UpdateEveryInterval();
        }
        if (timerloose >= 10f)
        {
            isGameOver = true;
            timerloose = 0f;
        }

       
        RectTransform rt = gameObject.GetComponent<RectTransform>();
                
        speed = b; // Speed of movement*
        uord = c;
		updateInterval = d;
		float move = speed * Time.deltaTime;
        if (uord == 1 && y >= -300+height/2)
        {
            y += move;
            if (y > 300-height/2) y = 300-height/2;
        }
        if (uord == 2 && y <= 300-height/2)
        {
            y -= move;
            if (y < -300+height/2) y = -300+height/2;
        }
        rt.anchoredPosition = new Vector2(x, y);
        
        
        RectTransform rtTiming = timing.GetComponent<RectTransform>();
        timingheight = 600 * (timerloose / 10);
        if (timingheight>= 600) timingheight = 600; 
        rtTiming.anchoredPosition = new Vector2(rtTiming.anchoredPosition.x, -300 + timingheight / 2);
        rtTiming.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, timingheight);
        
        
    }
    
}
