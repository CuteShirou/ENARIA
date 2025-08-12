using System.Collections;
using System.Collections.Generic;
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
        //hdiheioe
        
    }

    // Update is called once per frame
    void Update()
    {
        if (cerealesprogress.height >= 599)
        {
            cereales.SetActive(false);
            cerealesIsActive = false;
            Debug.Log("céréales récoltées");
        }
        if (minijeuplantes.height >= 599)
        {
            plantes.SetActive(false);
            plantesIsActive = false;
            Debug.Log("plantes récoltées");
        }
        if (boisprogress.height >= 599)
        {
            bois.SetActive(false);
            boisIsActive = false;
            Debug.Log("bois récolté");
        }

        if (pecheprogress.height >= 599)
        {
            poisson.SetActive(false);
            poissonIsActive = false;
            Debug.Log("poisson récolté");
        }

        if (minagebutton.Win == true)
        {
            minerai.SetActive(false);
            mineraiIsActive = false;
            Debug.Log("minerai récolté");
            minagebutton.Win = false; // Reset the win condition for next time
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(champTag))
        {
            cereales.SetActive(true);
            Debug.Log("collide avec champ");
            cerealesIsActive = true;
            var TPC = other.GetComponent<ThirdPersonController>();
            if (TPC != null)
            {
                TPC.IsInCombat = false;
                TPC.ForceStopMovement();
            }
            
        }
        if (other.CompareTag(planteTag))
        {
            plantes.SetActive(true);
            Debug.Log("collide avec plante");
            plantesIsActive = true;
            var TPC = other.GetComponent<ThirdPersonController>();
            if (TPC != null)
            {
                TPC.IsInCombat = false;
                TPC.ForceStopMovement();
            }
        }
        if (other.CompareTag(rocherTag))
        {
            minerai.SetActive(true);
            Debug.Log("collide avec rocher");
            mineraiIsActive = true;
            var TPC = other.GetComponent<ThirdPersonController>();
            if (TPC != null)
            {
                TPC.IsInCombat = false;
                TPC.ForceStopMovement();
            }
        }
        if (other.CompareTag(arbreTag))
        {
            bois.SetActive(true);
            Debug.Log("collide avec arbre");
            boisIsActive = true;
            var TPC = other.GetComponent<ThirdPersonController>();
            if (TPC != null)
            {
                TPC.IsInCombat = false;
                TPC.ForceStopMovement();
            }
        }
        if (other.CompareTag(canneTag))
        {
            poisson.SetActive(true);
            Debug.Log("collide avec canne");
            poissonIsActive = true;
            var TPC = other.GetComponent<ThirdPersonController>();
            if (TPC != null)
            {
                TPC.IsInCombat = false;
                TPC.ForceStopMovement();
            }
        }
    }
 
}
