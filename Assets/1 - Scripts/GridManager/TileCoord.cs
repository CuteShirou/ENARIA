using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCoord : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] public Material highlight;
    [SerializeField] public Material normal;
    [SerializeField] public Material activeFighter;
    [SerializeField] public Material red;
    [SerializeField] public Material blue;
    [SerializeField] public Material green;

    public Material currentMaterial;  // Le matériau de base (vert, rouge, bleu ou normal)
    public int X { get; private set; }
    public int Y { get; private set; }
    public Vector2Int Coord;
    public GameObject occupant;
    public bool IsOccupied => occupant != null;

    public void SetCoord(int x, int y)
    {
        X = x;
        Y = y;
        Coord = new Vector2Int(X, Y);
    }

    public void SetTeamColor(string team)
    {
        Renderer rend = GetComponent<Renderer>();

        if (team == "green")
        {
            rend.sharedMaterial = green;
            currentMaterial = green;
        }
        else if (team == "red")
        {
            rend.sharedMaterial = red;
            currentMaterial = red;
        }
        else if (team == "blue")
        {
            rend.sharedMaterial = blue;
            currentMaterial = blue;
        }
        else
        {
            rend.sharedMaterial = normal;
            currentMaterial = normal;
        }
    }

    public void SetToNormal()
    {
        currentMaterial = normal;
        GetComponent<Renderer>().sharedMaterial = normal;
    }

    private void OnMouseDown()
    {
        CombatPreparationManager prep = FindAnyObjectByType<CombatPreparationManager>();
        if (prep != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && player.GetComponent<CombatController>()?.enabled == false)
            {
                prep.TryMovePlayerTo(this);
            }

            return;
        }
    }

    public void SetOccupant(GameObject entity)
    {
        occupant = entity;
    }

    public void ClearOccupant()
    {
        occupant = null;
        GetComponent<Renderer>().sharedMaterial = currentMaterial;
    }

    private void Update()
    {
        if (occupant != null)
        {
            CombatController ctrl = occupant.GetComponent<CombatController>();
            if (ctrl != null && ctrl.enabled)
            {
                GetComponent<Renderer>().sharedMaterial = activeFighter;
            }


            // 11/07/2025
            CombatStats stats = occupant.GetComponent<CombatStats>();
            if (stats != null && stats.isDead)
            {
                ClearOccupant();
            }
        }
    }


    private void OnMouseEnter()
    {
        if (GetComponent<Renderer>().sharedMaterial != activeFighter)
        {
            GetComponent<Renderer>().sharedMaterial = highlight;
        }
    }

    private void OnMouseExit()
    {
        if (GetComponent<Renderer>().sharedMaterial == highlight)
        {
            GetComponent<Renderer>().sharedMaterial = currentMaterial;
        }
    }


}
