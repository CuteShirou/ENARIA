using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using Unity.VisualScripting;
using UnityEngine;

public class OnClick3D : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private string champTag = "champ";
    [SerializeField] private string canneTag = "canne";
    [SerializeField] private string arbreTag = "arbre";
    [SerializeField] private string rocherTag = "rocher";
    [SerializeField] private string planteTag = "plante";
    public GameObject cereales;
    public GameObject bois;
    public GameObject minerai;
    public GameObject poisson;
    public GameObject plantes;
    public static bool cerealesIsActive = false;
    public static bool boisIsActive = false;
    public static bool mineraiIsActive = false;
    public static bool poissonIsActive = false;
    public static bool plantesIsActive = false;
    void Start()
    {
        
        
    }

    // Update is called once per frame
    void Update()
    {
        var TPC = this.GetComponent<ThirdPersonController>();
        if (cerealesprogress.height >= 599)
        {
            cereales.SetActive(false);
            cerealesIsActive = false;
            Debug.Log("céréales récoltées");
            cerealesprogress.height = 0; 
        }
        if (minijeuplantes.height >= 599)
        {
            plantes.SetActive(false);
            plantesIsActive = false;
            Debug.Log("plantes récoltées");
            minijeuplantes.height = 0;
            TPC.enabled = true;
        }
        if (boisprogress.height >= 599)
        {
            bois.SetActive(false);
            boisIsActive = false;
            Debug.Log("bois récolté");
            boisgameplay.isstarting = true; 
            boisprogress.height = 0;
            TPC.enabled = true;
        }

        if (pecheprogress.height >= 599)
        {
            poisson.SetActive(false);
            poissonIsActive = false;
            Debug.Log("poisson récolté");
            pecheprogress.height = 0;
            TPC.enabled = true;
        }

        if (minagebutton.Win == true)
        {
            minerai.SetActive(false);
            mineraiIsActive = false;
            Debug.Log("minerai récolté");
            minagebutton.Win = false;
            TPC.enabled = true;
        }
      
    }
    private void OnTriggerEnter(Collider other)
    {
        var TPC = this.GetComponent<ThirdPersonController>();
        
        if (other.CompareTag(rocherTag))
        {
            minerai.SetActive(true);
            Debug.Log("collide avec rocher");
            mineraiIsActive = true;
            
            if (TPC != null)
            {
                
                Debug.Log("rocher tcp");
                TPC.IsInCombat = false;
                TPC.ForceStopMovement();
                TPC.enabled = false;
            }
        }
        if (other.CompareTag(champTag))
        {
            cereales.SetActive(true);
            Debug.Log("collide avec champ");
            cerealesIsActive = true;
            
            if (TPC != null)
            {
                Debug.Log("champ tcp");
                TPC.IsInCombat = false;
                TPC.ForceStopMovement();
            }
            
        }
        if (other.CompareTag(planteTag))
        {
            plantes.SetActive(true);
            Debug.Log("collide avec plante");
            plantesIsActive = true;
           
            if (TPC != null)
            {
                Debug.Log("plante tcp");
                TPC.IsInCombat = false;
                TPC.ForceStopMovement();
                TPC.enabled = false;
            }
        }
        
        if (other.CompareTag(arbreTag))
        {
            bois.SetActive(true);
            Debug.Log("collide avec arbre");
            boisIsActive = true;
            
            if (TPC != null)
            {
                Debug.Log("arbre tcp");
                TPC.IsInCombat = false;
                TPC.ForceStopMovement();
                TPC.enabled = false;
            }
        }
        if (other.CompareTag(canneTag))
        {
            poisson.SetActive(true);
            Debug.Log("collide avec canne");
            poissonIsActive = true;
            
            if (TPC != null)
            {
                
                Debug.Log("canne tcp");
                TPC.IsInCombat = false;
                TPC.ForceStopMovement();
                TPC.enabled = false;
            }
        }
    }
 
}
