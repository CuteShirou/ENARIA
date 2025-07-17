using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TileCoord : NetworkBehaviour
{
    [Header("Materials")]
    [SerializeField] public Material highlight;
    [SerializeField] public Material normal;
    [SerializeField] public Material activeFighter;
    [SerializeField] public Material red;
    [SerializeField] public Material blue;
    [SerializeField] public Material green;

    public Material currentMaterial;
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
        if (!Application.isPlaying) return;

        // Détection du joueur local
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && player.GetComponent<CombatController>()?.enabled == false)
        {
            var ctrl = player.GetComponent<CombatController>();
            if (ctrl != null && ctrl.isLocalPlayer)
            {
                // Demande au serveur de tenter le déplacement
                ctrl.CmdRequestTileChange(GetComponent<NetworkIdentity>());
            }
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
        if (!Application.isPlaying) return;

        if (occupant != null)
        {
            CombatController ctrl = occupant.GetComponent<CombatController>();
            if (ctrl != null && ctrl.enabled)
            {
                GetComponent<Renderer>().sharedMaterial = activeFighter;
            }

            CombatStats stats = occupant.GetComponent<CombatStats>();
            if (stats != null && stats.isDead)
            {
                if (NetworkServer.active) // Seulement côté serveur
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
