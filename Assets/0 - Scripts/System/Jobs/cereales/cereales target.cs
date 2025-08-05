using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cerealestarget : MonoBehaviour
{
    public float x;
    public float y;
	public float speed;
    public float height;
    public bool hastogodown = false;
    public float move;
    public bool trigger = false;

   
    // Start is called before the first frame update
    void Start()
    {
	    
		RectTransform rt = gameObject.GetComponent<RectTransform>();
        height = rt.rect.height;
        speed = 200f;

    }

    // Update is called once per frame
    void Update()
    {
	    
	    move = speed * Time.deltaTime;
	    RectTransform rt = gameObject.GetComponent<RectTransform>();
	    if (hastogodown == true )
	    {
		    y -= move;
		    rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y); 
	    }
	    
    }

 	public void GoDown ()
    {
	    y = 300+ height / 2;
	    hastogodown = true;
    }

    public void triggered()
    {
	    y = -2000;
	    RectTransform rt = gameObject.GetComponent<RectTransform>();
	    rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y);
    }
}
