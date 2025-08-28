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
    [SerializeField] private string pnjspawnTag = "pnjspawn";
    [SerializeField] private string pnjforge1Tag = "pnjforge1";
    [SerializeField] private string pnjforge2Tag = "pnjforge2";
    [SerializeField] private string pnjshopTag = "pnjshop";
    [SerializeField] private string pnjtaverneTag = "pnjtaverne";
    public GameObject cereales;
    public GameObject bois;
    public GameObject minerai;
    public GameObject poisson;
    public GameObject plantes;
    public GameObject dialoguespawn;
    public GameObject dialogueforge1;
    public GameObject dialogueforge2;
    public GameObject dialogueshop;
    public GameObject dialoguetaverne;
    public static bool cerealesIsActive = false;
    public static bool boisIsActive = false;
    public static bool mineraiIsActive = false;
    public static bool poissonIsActive = false;
    public static bool plantesIsActive = false;
    public static bool dialoguespawnIsActive = false;
    public static bool dialogueforge1IsActive = false;
    public static bool dialogueforge2IsActive = false;
    public static bool dialogueshopIsActive = false;
    public static bool dialoguetaverneIsActive = false;
    public Item bletoguive;
    public Item saumontoguive;
    public Item tulipetoguive;
    public Item ortoguive;
    public Item boistoguive;
    
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
            InventoryUtil.AddItemToFirstEmpty(bletoguive);
        }
        if (minijeuplantes.height >= 599)
        {
            plantes.SetActive(false);
            plantesIsActive = false;
            Debug.Log("plantes récoltées");
            minijeuplantes.height = 0;
            TPC.enabled = true;
            InventoryUtil.AddItemToFirstEmpty(tulipetoguive);
        }
        if (boisprogress.height >= 599)
        {
            bois.SetActive(false);
            boisIsActive = false;
            Debug.Log("bois récolté");
            boisgameplay.isstarting = true; 
            boisprogress.height = 0;
            TPC.enabled = true;
            InventoryUtil.AddItemToFirstEmpty(boistoguive);
        }

        if (pecheprogress.height >= 599)
        {
            poisson.SetActive(false);
            poissonIsActive = false;
            Debug.Log("poisson récolté");
            pecheprogress.height = 0;
            TPC.enabled = true;
            InventoryUtil.AddItemToFirstEmpty(saumontoguive);
        }

        if (minagebutton.Win == true)
        {
            minerai.SetActive(false);
            mineraiIsActive = false;
            Debug.Log("minerai récolté");
            minagebutton.Win = false;
            TPC.enabled = true;
            InventoryUtil.AddItemToFirstEmpty(ortoguive);
        }
        
        if (ButtonEndDialogue.isclicked == true)
        {
            dialogueforge1.SetActive(false);
            dialogueforge2.SetActive(false);
            dialoguespawn.SetActive(false);
            dialogueshop.SetActive(false);
            dialoguetaverne.SetActive(false);
            dialoguespawnIsActive = false;
            dialogueforge1IsActive = false;
            dialogueforge2IsActive = false;
            dialogueshopIsActive = false;
            dialoguetaverneIsActive = false;
            Debug.Log("dialogue terminé");
            ButtonEndDialogue.isclicked = false;
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
        if (other.CompareTag(pnjspawnTag))
        {
            dialoguespawn.SetActive(true);
            Debug.Log("collide avec pnj spawn");
            dialoguespawnIsActive = true;
            
            if (TPC != null)
            {
                Debug.Log("pnj spawn tcp");
                TPC.IsInCombat = false;
                TPC.ForceStopMovement();
                TPC.enabled = false;
            }
        }

        if (other.CompareTag(pnjforge1Tag))
        {
            dialogueforge1.SetActive(true);
            Debug.Log("collide avec pnj forge1");
            dialogueforge1IsActive = true;
            if (TPC != null)
            {
                Debug.Log("pnj forge1 tcp");
                TPC.IsInCombat = false;
                TPC.ForceStopMovement();
                TPC.enabled = false;
            }
        }
        if (other.CompareTag(pnjforge2Tag))
        {
            dialogueforge2.SetActive(true);
            Debug.Log("collide avec pnj forge2");
            dialogueforge2IsActive = true;
            if (TPC != null)
            {
                Debug.Log("pnj forge2 tcp");
                TPC.IsInCombat = false;
                TPC.ForceStopMovement();
                TPC.enabled = false;
            }
        }
        if (other.CompareTag(pnjshopTag))
        {
            dialogueshop.SetActive(true);
            Debug.Log("collide avec pnj shop");
            dialogueshopIsActive = true;
            if (TPC != null)
            {
                Debug.Log("pnj shop tcp");
                TPC.IsInCombat = false;
                TPC.ForceStopMovement();
                TPC.enabled = false;
            }
        }
        if (other.CompareTag(pnjtaverneTag))
        {
            dialoguetaverne.SetActive(true);
            Debug.Log("collide avec pnj taverne");
            dialoguetaverneIsActive = true;
            if (TPC != null)
            {
                Debug.Log("pnj taverne tcp");
                TPC.IsInCombat = false;
                TPC.ForceStopMovement();
                TPC.enabled = false;
            }
        }

    }
 
}
