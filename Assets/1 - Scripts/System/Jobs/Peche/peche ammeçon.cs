using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class peche : MonoBehaviour
{
    // Start is called before the first frame update
    
    public float x ;
    public float y;
    public float speed;
    void Start()
    {
        RectTransform rt = gameObject.GetComponent<RectTransform>();
        x = rt.anchoredPosition.x;
        y = rt.anchoredPosition.y;
       speed = 3f; // Speed of movement
        

        // Set the initial position of the game object
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
    }
}
