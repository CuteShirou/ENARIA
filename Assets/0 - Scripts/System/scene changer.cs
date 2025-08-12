using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scenechanger : MonoBehaviour
{
    
    public string sceneName ;
    public void change_scene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
