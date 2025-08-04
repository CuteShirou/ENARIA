using System.Collections.Generic;
using UnityEngine;
using Mirror;

#if UNITY_EDITOR
using UnityEditor;
#endif

//------------------------------------------------------------
// Gère les relations entité ↔ tuile pour une arène donnée
// À attacher à TileGridRoot (un par arène)
//------------------------------------------------------------
public class TileGrid_Manager : NetworkBehaviour
{
    [Header("Liste des tuiles de la grille")]
    [SerializeField] private List<GameObject> allTiles = new();

    // Dictionnaire entité → tuile
    private Dictionary<GameObject, GameObject> entityToTile = new();

    // Dictionnaire tuile → entité
    private Dictionary<GameObject, GameObject> tileToEntity = new();

    //------------------------------------------------------------
    // Enregistre une tuile dans la grille
    public void RegisterTile(GameObject tile)
    {
        if (!allTiles.Contains(tile))
        {
            allTiles.Add(tile);
            tileToEntity[tile] = null;
        }
    }

    //------------------------------------------------------------
    // Associe une entité à une tuile
    public void RegisterEntity(GameObject entity, GameObject tile)
    {
        if (entity == null || tile == null)
        {
            Debug.LogWarning("[TileGrid] Tentative d’enregistrement avec une entité ou tuile null.");
            return;
        }

        // Libère l’ancienne tuile si existante
        if (entityToTile.TryGetValue(entity, out GameObject oldTile) && oldTile != null)
        {
            tileToEntity[oldTile] = null;
        }

        entityToTile[entity] = tile;
        tileToEntity[tile] = entity;

        // Marque la tuile comme occupée (si possible)
        if (tile.TryGetComponent(out Info_NetworkTile info))
        {
            info.SetOccupied();
        }
    }

    //------------------------------------------------------------
    // Libère la tuile occupée par l’entité
    public void UnregisterEntity(GameObject entity)
    {
        if (entityToTile.TryGetValue(entity, out GameObject tile) && tile != null)
        {
            tileToEntity[tile] = null;

            if (tile.TryGetComponent(out Info_NetworkTile info))
            {
                info.SetFree();
            }
        }

        entityToTile.Remove(entity);
    }

    //------------------------------------------------------------
    // Vérifie si une tuile est libre
    public bool IsTileFree(GameObject tile)
    {
        if (tile == null) return false;

        if (tileToEntity.TryGetValue(tile, out GameObject occupant))
            return occupant == null;

        return true;
    }

    //------------------------------------------------------------
    // Retourne la tuile actuelle d’une entité
    public GameObject GetTileOfEntity(GameObject entity)
    {
        if (entityToTile.TryGetValue(entity, out GameObject tile))
            return tile;

        return null;
    }

    //------------------------------------------------------------
    // Retourne l’entité placée sur une tuile
    public GameObject GetEntityOnTile(GameObject tile)
    {
        if (tileToEntity.TryGetValue(tile, out GameObject occupant))
            return occupant;

        return null;
    }

    //------------------------------------------------------------
    // Retourne toutes les tuiles de la grille
    public List<GameObject> GetAllTiles()
    {
        return allTiles;
    }

    //------------------------------------------------------------
    // Retourne la tuile aux coordonnées X/Y (utile pour Cmd de placement)
    public GameObject GetTileAtCoordinates(int x, int y)
    {
        foreach (GameObject tile in allTiles)
        {
            if (tile.TryGetComponent(out Setup_NetworkTile setup))
            {
                if (setup.tileX == x && setup.tileY == y)
                    return tile;
            }
        }

        return null;
    }

    //==================================================================
    //==========           DEBUG INSPECTOR VISUEL             ==========
    //==================================================================

    [Header("DEBUG: État des cases (lecture seule)")]
    [SerializeField] private List<string> entityToTileDebug = new();
    [SerializeField] private List<string> tileToEntityDebug = new();

    [ContextMenu("Update Debug Info (Play Mode Only)")]
    private void UpdateDebugVisuals()
    {
        entityToTileDebug.Clear();
        tileToEntityDebug.Clear();

        foreach (var pair in entityToTile)
        {
            string entity = pair.Key != null ? pair.Key.name : "NULL";
            string tile = pair.Value != null ? pair.Value.name : "NULL";
            entityToTileDebug.Add($"{entity} → {tile}");
        }

        foreach (var pair in tileToEntity)
        {
            string tile = pair.Key != null ? pair.Key.name : "NULL";
            string entity = pair.Value != null ? pair.Value.name : "VIDE";
            tileToEntityDebug.Add($"{tile} ← {entity}");
        }
    }
}
