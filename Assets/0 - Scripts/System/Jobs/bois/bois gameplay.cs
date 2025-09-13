using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

public class boisgameplay : MonoBehaviour
{

    public GameObject minijeu;
    public boisprogress progressBarScript;
    public  float gain; 
    public static bool isstarting = false;
    
    public float x;
    public float y;
    public  float height;
    public  float speed;
    public  bool goup;
    public  float b;
    public static int life;
    public static bool isGameOver = false;
    public Sprite fullHeart;
    public Sprite emptyHeart;
    public GameObject heart2;
    public GameObject heart3;
    public GameObject heart4;
    public GameObject heart5;
    public Image imgHeart2;
    public Image imgHeart3;
    public Image imgHeart4;
    public Image imgHeart5;
    
    
    public  GameObject top;
    public  GameObject bottom;
    public  float heightTB;
    public  float a;
    public  float topx;
    public  float topy;
    public  float bottomx;
    public  float bottomy;

    public  bool lastIsTop ;
    // Start is called before the first frame update
    void Start()
    {
        life = 5; 
        gain = 100f; 
        lastIsTop = false;
        a = Random.Range(70f, 150f);
        goup = true;
        heightTB = a;
        b = Random.Range(100f, 500f);
        speed = b;
        RectTransform rt = gameObject.GetComponent<RectTransform>();
        height = rt.rect.height;
        
        
        RectTransform rtTop = top.GetComponent<RectTransform>();
        topx = rtTop.anchoredPosition.x;
        topy = rtTop.anchoredPosition.y;
        RectTransform rtBottom = bottom.GetComponent<RectTransform>();
        bottomx = rtBottom.anchoredPosition.x;
        bottomy = rtBottom.anchoredPosition.y;
        
        
        Vector3 scaletop = top.transform.localScale;
        scaletop.y = heightTB;
        top.transform.localScale = scaletop;
        Vector3 scalebottom = bottom.transform.localScale;
        scalebottom.y = heightTB;
        bottom.transform.localScale = scalebottom;
        rtTop.anchoredPosition = new Vector2(rtTop.anchoredPosition.x, 300 - heightTB / 2);
        rtBottom.anchoredPosition = new Vector2(rtBottom.anchoredPosition.x, -300 + heightTB / 2);
        
        imgHeart2 = heart2.GetComponent<Image>();
        imgHeart3 = heart3.GetComponent<Image>();
        imgHeart4 = heart4.GetComponent<Image>();
        imgHeart5 = heart5.GetComponent<Image>();
        
    }

    // Update is called once per frame
    void Update()
    {
       
        if (minijeu.activeSelf == true && isstarting == true )
        {
            Start(); // Skip update if the mini-game is active
            isstarting = false;
        }
        
        
        
        RectTransform rt = gameObject.GetComponent<RectTransform>();
        float move = speed * Time.deltaTime;
        x = rt.anchoredPosition.x;
        y = rt.anchoredPosition.y;
        if (goup == true && y <= 300 - height / 2)
        {
            y += move;
            if (y > 300 - height / 2)
            {
                y = 300 - height / 2; 
                goup = false;
                b = Random.Range(100f, 500f);
                speed = b;
            }
        }
        if (goup == false && y >= -300 + height / 2)
        {
            y -= move;
            if (y < -300 + height / 2)
            {
                y = -300 + height / 2; 
                goup = true;
                b = Random.Range(100f, 500f);
                speed = b;
            }
        }
        
        if (lastIsTop == false && Input.GetMouseButtonDown(0) && RectsOverlap(rt, top.GetComponent<RectTransform>()))
        { 
            progressBarScript.AugmenterHauteur(gain); 
            lastIsTop = true;
        }
        else if (lastIsTop == true && Input.GetMouseButtonDown(0) && RectsOverlap(rt, bottom.GetComponent<RectTransform>()))
        {
            progressBarScript.AugmenterHauteur(gain);
            lastIsTop = false;
        }
        else if (lastIsTop == false && Input.GetMouseButtonDown(0) && !RectsOverlap(rt, top.GetComponent<RectTransform>()))
        {
            life -= 1;
        }
        else if (lastIsTop == true && Input.GetMouseButtonDown(0) && !RectsOverlap(rt, bottom.GetComponent<RectTransform>()))
        {
            life -= 1;
        }
        if (life == 5)
        {
            if (imgHeart2 != null && fullHeart != null)
                imgHeart2.sprite = fullHeart;
            if (imgHeart3 != null && fullHeart != null)
                imgHeart3.sprite = fullHeart;
            if (imgHeart4 != null && fullHeart != null)
                imgHeart4.sprite = fullHeart;
            if (imgHeart5 != null && fullHeart != null)
                imgHeart5.sprite = fullHeart;
        }
        else if (life == 4)
        {
            if (imgHeart5 != null && emptyHeart != null)
                imgHeart5.sprite = emptyHeart;
        }
        else if (life == 3)
        {
            if (imgHeart4 != null && emptyHeart != null)
                imgHeart4.sprite = emptyHeart;
        }
        else if (life == 2)
        {
            if (imgHeart3 != null && emptyHeart != null)
                imgHeart3.sprite = emptyHeart;
        }
        else if (life == 1)
        {
            if (imgHeart2 != null && emptyHeart != null)
                imgHeart2.sprite = emptyHeart;
        }
        else if (life <= 0 && isGameOver == false)
        {
            if (imgHeart5 != null && fullHeart != null)
                imgHeart5.sprite = fullHeart;
            if (imgHeart4 != null && fullHeart != null)
                imgHeart4.sprite = fullHeart;
            if (imgHeart3 != null && fullHeart != null)
                imgHeart3.sprite = fullHeart;
            if (imgHeart2 != null && fullHeart != null)
                imgHeart2.sprite = fullHeart;
            isGameOver = true;
        }
        rt.anchoredPosition = new Vector2(x, y);

        
        
        
        
        
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
