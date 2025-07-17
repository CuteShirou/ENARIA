using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pecheprogress : MonoBehaviour
{



    public GameObject amecon;
    public GameObject poisson;

    public float amecon_y;
    public float amecon_height;
    public float poisson_y;
    public float poisson_height;
    public float x;
    public float y;
    public float height;
    
    
    // get the transform of the OuiOuiBaguette object
    
    void Start()
    {
        RectTransform rt = gameObject.GetComponent<RectTransform>();
        amecon_y = amecon.transform.position.y;
         amecon_height = amecon.transform.localScale.y;
         amecon_height = amecon.transform.localScale.y;

         poisson_y = poisson.transform.position.y;
         poisson_height = poisson.transform.localScale.y;
         
         
         y = rt.anchoredPosition.y;
         x = rt.anchoredPosition.x;
         height = 0;
    }

    void Update()
    {
        RectTransform rt = gameObject.GetComponent<RectTransform>();
        
        if(amecon_y> poisson_y- poisson_height/2 && amecon_y < poisson_y + poisson_height/2)
        {
            height += 10f;
        }
        y = -300+height/2;
        
        
        rt.anchoredPosition = new Vector2(x, y);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,height);
    }

}
