using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private string pnjTag = "pnj";
    public GameObject dialogueUI;
    public bool dialogueIsActive = false;
    public Text nameText;
    public Text dialogueText;
   

    // Update is called once per frame
    void Update()
    {
        
        
    }

    private void OnTriggerEnter(Collider other)
    {
        var TPC = this.GetComponent<ThirdPersonController>();
        if (other.CompareTag(pnjTag))
        {
            dialogueUI.SetActive(true);
            Debug.Log("collide avec un pnj");
            dialogueIsActive = true;
            
            if (TPC != null)
            {
                
                Debug.Log("rocher tcp");
                TPC.IsInCombat = false;
                TPC.ForceStopMovement();
                TPC.enabled = false;
            }
        }
    }
}
