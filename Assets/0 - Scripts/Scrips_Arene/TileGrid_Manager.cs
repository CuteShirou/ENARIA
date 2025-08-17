using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

//------------------------------------------------------------
// Gère les relations entité ↔ tuile pour une arène donnée
// À attacher à TileGridRoot (un par arène)
// Version locale (sans Mirror)
//------------------------------------------------------------
[AddComponentMenu("Combat/Tile Grid Manager (Local)")]
public class TileGrid_Manager : MonoBehaviour
{
    [Header("Liste des tuiles de la grille")]
    [SerializeField] private List<GameObject> allTiles = new();

    // Dictionnaire entité → tuile
    private readonly Dictionary<GameObject, GameObject> entityToTile = new();

    // Dictionnaire tuile → entité
    private readonly Dictionary<GameObject, GameObject> tileToEntity = new();

    //------------------------------------------------------------
    // Enregistre une tuile dans la grille
    public void RegisterTile(GameObject tile)
    {
        if (tile == null) return;

        if (!allTiles.Contains(tile))
        {
            allTiles.Add(tile);
        }

        if (!tileToEntity.ContainsKey(tile))
        {
            tileToEntity[tile] = null; // libre par défaut
        }
    }

    //------------------------------------------------------------
    // Associe une entité à une tuile (libère l’ancienne si besoin)
    public void RegisterEntity(GameObject entity, GameObject tile)
    {
        if (entity == null || tile == null)
        {
            Debug.LogWarning("[TileGrid] Tentative d’enregistrement avec une entité ou une tuile null.");
            return;
        }

        // Libère l’ancienne tuile
        if (entityToTile.TryGetValue(entity, out GameObject oldTile) && oldTile != null)
        {
            tileToEntity[oldTile] = null;
        }

        entityToTile[entity] = tile;
        tileToEntity[tile] = entity;
    }

    //------------------------------------------------------------
    // Libère la tuile occupée par l’entité
    public void UnregisterEntity(GameObject entity)
    {
        if (entity == null) return;

        if (entityToTile.TryGetValue(entity, out GameObject tile) && tile != null)
        {
            tileToEntity[tile] = null;
        }

        entityToTile.Remove(entity);
    }

    //------------------------------------------------------------
    // Vérifie si une tuile est libre
    public bool IsTileFree(GameObject tile)
    {
        if (tile == null) return false;
        return !tileToEntity.TryGetValue(tile, out GameObject occupant) || occupant == null;
    }

    //------------------------------------------------------------
    // Retourne la tuile actuelle d’une entité
    public GameObject GetTileOfEntity(GameObject entity)
    {
        if (entity == null) return null;
        return entityToTile.TryGetValue(entity, out GameObject tile) ? tile : null;
    }

    //------------------------------------------------------------
    // Retourne l’entité placée sur une tuile
    public GameObject GetEntityOnTile(GameObject tile)
    {
        if (tile == null) return null;
        return tileToEntity.TryGetValue(tile, out GameObject occupant) ? occupant : null;
    }

    //------------------------------------------------------------
    // Retourne toutes les tuiles de la grille
    public List<GameObject> GetAllTiles() => allTiles;

    //------------------------------------------------------------
    // Retourne la tuile aux coordonnées X/Y
    public GameObject GetTileAtCoordinates(int x, int y)
    {
        foreach (GameObject tile in allTiles)
        {
            if (tile != null && tile.TryGetComponent(out SetupTile setup))
            {
                if (setup.tileX == x && setup.tileY == y)
                    return tile;
            }
        }
        return null;
    }

    //==================================================================
    //=====================   DEBUG INSPECTOR   =========================
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
