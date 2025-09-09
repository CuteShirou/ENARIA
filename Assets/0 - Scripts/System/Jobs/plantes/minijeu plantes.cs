using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class minijeuplantes : MonoBehaviour
{
    // Start is called before the first frame update
    
    public float x;
    public static float y;
    public static float height;
    public float gain;
    public bool lastIsA;
    public GameObject KeyA;
    public GameObject KeyE;
    public Image KeyAImage;
    public Image KeyEImage;
    public Sprite downSpriteA;
    public Sprite upSpriteA;
    public Sprite downSpriteE;
    public Sprite upSpriteE;
    public float move;
    public GameObject timing;
    public float timingheight;

    public static float timerloose = 0f;
    public static bool isGameOver = false;
    void Start()
    {
        RectTransform rt = gameObject.GetComponent<RectTransform>();
      
        y = rt.anchoredPosition.y;
        x = rt.anchoredPosition.x;
        lastIsA = false;
        gain = 10f;
        KeyAImage = KeyA.GetComponent<Image>();
        KeyEImage = KeyE.GetComponent<Image>();
        move = 10f;


    }

    // Update is called once per frame
    void Update()
    {
        
        timerloose += Time.deltaTime;
        if (timerloose >= 10f)
        {
            isGameOver = true;
            timerloose = 0f;
        }
        RectTransform rt = gameObject.GetComponent<RectTransform>();
        if (lastIsA == false && Input.GetKeyDown(KeyCode.Q))
        {
            lastIsA = true;
            y += gain;
            if (height >= 20) height = 20; 
            if (KeyAImage != null && downSpriteA != null)
                KeyAImage.sprite = downSpriteA;

            if (KeyEImage != null && upSpriteE != null)
                KeyEImage.sprite = upSpriteE;
        }
        else if (lastIsA == true && Input.GetKeyDown(KeyCode.E))
        {
            lastIsA = false;
            y += gain;
            if (height >= 20) height = 20; 
            
            if (KeyAImage != null && upSpriteA != null)
                KeyAImage.sprite = upSpriteA;

            if (KeyEImage != null && downSpriteE != null)
                KeyEImage.sprite = downSpriteE;
        }
        y -= move * Time.deltaTime;
        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y);
        
        
        
        RectTransform rtTiming = timing.GetComponent<RectTransform>();
        timingheight = 600 * (timerloose / 10);
        if (timingheight>= 600) timingheight = 600; 
        rtTiming.anchoredPosition = new Vector2(rtTiming.anchoredPosition.x, -300 + timingheight / 2);
        rtTiming.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, timingheight);
      
        
    }
}
