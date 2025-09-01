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

    // ─────────────────────────────────────────────────────────
    // AJOUT — refs de scène à propager automatiquement aux tuiles
    [Header("Injection vers Tile_Visual")]
    public InfoEntityPanelUI infoPanelForTiles; //   Assigne ici ton Panel_Info_Bubble (InfoEntityPanelUI)
    // ─────────────────────────────────────────────────────────

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
            allTiles.Add(tile);

        if (!tileToEntity.ContainsKey(tile))
            tileToEntity[tile] = null; // libre par défaut

        // ─────────────────────────────────────────────────────
        // AJOUT — Injection auto des refs sur cette tuile
        //   Pas d'auto-find : on passe "this" + le panel assigné dans l'inspector
        var visual = tile.GetComponent<Tile_Visual>();
        if (visual != null)
            visual.SetShared(this, infoPanelForTiles);
        // ─────────────────────────────────────────────────────
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

        // Libère l'ancienne tuile occupée par cette entité
        if (entityToTile.TryGetValue(entity, out GameObject oldTile) && oldTile != null)
            tileToEntity[oldTile] = null;

        entityToTile[entity] = tile;
        tileToEntity[tile] = entity;
    }

    //------------------------------------------------------------
    // Libère la tuile occupée par l’entité
    public void UnregisterEntity(GameObject entity)
    {
        if (entity == null) return;

        if (entityToTile.TryGetValue(entity, out GameObject tile) && tile != null)
            tileToEntity[tile] = null;

        entityToTile.Remove(entity);
    }

    //------------------------------------------------------------
    // Libère toutes les entités (ne détruit rien)
    public void UnregisterAllEntities()
    {
        var copy = new List<GameObject>(entityToTile.Keys);
        foreach (var e in copy)
            UnregisterEntity(e);
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

    //------------------------------------------------------------
    // RESET COMPLET : détruit les GameObjects de tuiles et vide les dictionnaires
    public void ClearGrid(bool destroyTilesGameObjects = true)
    {
        // 1) Libère toutes les entités (aucun occupant sur aucune tuile)
        UnregisterAllEntities();

        // 2) Détruit les tuiles si demandé
        if (destroyTilesGameObjects)
        {
            foreach (var tile in allTiles)
            {
                if (tile == null) continue;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Object.DestroyImmediate(tile);
                else
#endif
                    Object.Destroy(tile);
            }
        }

        // 3) Vide les structures
        allTiles.Clear();
        tileToEntity.Clear();
        entityToTile.Clear();

        Debug.Log("[TileGrid] Grille nettoyée (tuiles " + (destroyTilesGameObjects ? "détruites" : "préservées") + ").");
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

    // ─────────────────────────────────────────────────────────────────
    // AJOUT — utilitaire: ré-injecter aux tuiles déjà enregistrées (facultatif)
    [ContextMenu("Reinject Shared Refs To All Tiles")]
    private void ReinjectSharedToAllTiles()
    {
        foreach (var tile in allTiles)
        {
            if (!tile) continue;
            var visual = tile.GetComponent<Tile_Visual>();
            if (visual != null)
                visual.SetShared(this, infoPanelForTiles);
        }
    }
    // ─────────────────────────────────────────────────────────────────
}
