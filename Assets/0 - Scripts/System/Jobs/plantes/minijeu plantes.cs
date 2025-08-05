using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class minijeuplantes : MonoBehaviour
{
    // Start is called before the first frame update
    
    public float x;
    public float y;
    public float height;
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
    void Start()
    {
        RectTransform rt = gameObject.GetComponent<RectTransform>();
      
        y = rt.anchoredPosition.y;
        x = rt.anchoredPosition.x;
        lastIsA = false;
        gain = 20f;
        height = 0;
        KeyAImage = KeyA.GetComponent<Image>();
        KeyEImage = KeyE.GetComponent<Image>();
        move = 50f;


    }

    // Update is called once per frame
    void Update()
    {
        RectTransform rt = gameObject.GetComponent<RectTransform>();
        if (lastIsA == false && Input.GetKeyDown(KeyCode.Q))
        {
            lastIsA = true;
            height += gain;
            if (height >= 600) height = 600; 
            if (KeyAImage != null && downSpriteA != null)
                KeyAImage.sprite = downSpriteA;

            if (KeyEImage != null && upSpriteE != null)
                KeyEImage.sprite = upSpriteE;
        }
        else if (lastIsA == true && Input.GetKeyDown(KeyCode.E))
        {
            lastIsA = false;
            height += gain;
            if (height >= 600) height = 600; 
            
            if (KeyAImage != null && upSpriteA != null)
                KeyAImage.sprite = upSpriteA;

            if (KeyEImage != null && downSpriteE != null)
                KeyEImage.sprite = downSpriteE;
        }
        height -= move * Time.deltaTime;
        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -300 + height / 2);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        
    }
}
