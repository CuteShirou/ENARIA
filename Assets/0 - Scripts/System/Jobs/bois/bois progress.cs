using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boisprogress : MonoBehaviour
{
    public float x;
    public float y;
    public float height;
    
    
    // get the transform of the OuiOuiBaguette object
    
    void Start()
    {
        RectTransform rt = gameObject.GetComponent<RectTransform>();
      
        y = rt.anchoredPosition.y;
        x = rt.anchoredPosition.x;
        height = 0;
        
    }

    void Update()
    {
       
    }

    public void AugmenterHauteur(float valeur)
    {
        RectTransform rt = gameObject.GetComponent<RectTransform>();
        height += valeur ;
        if (height> 600) height = 600; 
        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -300 + height / 2);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }
}
