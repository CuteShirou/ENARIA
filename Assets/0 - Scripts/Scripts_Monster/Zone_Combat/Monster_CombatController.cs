using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Monster_CombatController
///   Contrôleur commun des monstres : tempo de tour, déplacement pas-à-pas,
///      lancement de compétences via Entity_SkillCaster, et utilitaires IA.
///      Le "cerveau" (IA) est fourni par un composant séparé implémentant IMonsterAI.
/// </summary>
public class Monster_CombatController : MonoBehaviour
{

    [Header("Références (assignées au spawn / prefab)")]
    public Combat_PhaseManager phaseManager;   // Réf manager combat (injectée dans Phase_EnterSetupCombat)
    public TileGrid_Manager tileGrid;          // Grille (injectée)
    public Entity_StatistiqueCombat stats;     // Stats de l'entité (HP/PA/PM/PO/Team/skillBook)
    public Entity_SkillCaster caster;          // Lanceur de compétences (commun)

    [Header("IA (plug-in)")]
    [Tooltip("Brancher ici le composant IA (ex: IA_Aggressif). Aucun auto-find.")]
    public MonoBehaviour behaviorComponent;    // Doit implémenter IMonsterAI
    private IMonsterAI behavior;               // Interface IA
    public MonsterAIType typeIA;               // Indicatif pour l’inspector

    [Header("Tempo")]
    public float startDelaySec = 1.0f;         // Attente en début de tour
    public float afterMoveDelaySec = 0.1f;     // Petite pause après un pas (lisibilité)
    public float stepDuration = 0.2f;          // Durée d’un pas (lerp court)

    [Header("Debug")]
    public bool verboseLog = false;

    // Flag lu par la phase (elle bloque EndTurn si une entité bouge)
    public bool isMoving { get; private set; } = false;

    // Routine unique par tour
    private bool isPlayingTurn = false;

    // Pop-up pour afficher la consommation de PM
    private Popup_DisplayNumber popup;

    private void Awake()
    {
        // Récupère les composants locaux si absents
        if (!stats) TryGetComponent(out stats);
        if (!caster) TryGetComponent(out caster);
        if (!popup) TryGetComponent(out popup);

        // Vérifie le compos IA
        if (behaviorComponent != null)
        {
            behavior = behaviorComponent as IMonsterAI;
            if (behavior == null)
                Debug.LogError($"[{name}] Le composant IA assigné ne supporte pas IMonsterAI.");
        }
        else
        {
            Debug.LogWarning($"[{name}] Aucun composant IA assigné (IMonsterAI).");
        }

        // Le caster doit connaître ses refs (on push seulement si vides)
        if (caster != null)
        {
            if (!caster.phaseManager) caster.phaseManager = phaseManager;
            if (!caster.tileGrid) caster.tileGrid = tileGrid;
        }
    }

    private void Update()
    {
        // Sécurités
        if (phaseManager == null || phaseManager.phaseTurn == null || tileGrid == null || stats == null || caster == null || behavior == null)
            return;
        if (!phaseManager.isInCombat || stats.isDead) return;

        // Déclenche un tour uniquement si c'est à lui
        if (phaseManager.phaseTurn.IsMyTurn(gameObject) && !isPlayingTurn)
            StartCoroutine(PlayTurnRoutine());
    }

    private IEnumerator PlayTurnRoutine()
    {
        isPlayingTurn = true;

        // Tempo d'entrée de tour
        if (startDelaySec > 0f) yield return new WaitForSeconds(startDelaySec);

        // Construit le contexte partagé (références + utilitaires)
        var ctx = new AIContext(this, phaseManager, tileGrid, stats, caster);

        // Hook IA début de tour
        behavior.OnTurnStart(ctx);

        // Boucle de décision simple : on exécute les actions jusqu’à EndTurn
        int guard = 16; // Sécurité anti-boucles
        while (guard-- > 0)
        {
            AIAction action = behavior.DecideNextAction(ctx);

            if (action.type == AIActionType.EndTurn)
                break;

            if (action.type == AIActionType.MoveStep && action.targetTile != null)
            {
                // Un pas visuel vers la tuile (cases cardinales)
                yield return StepToTile(action.targetTile);

                // Consomme 1 PM si dispo et affiche la pop-up PM
                if (stats.currentPM > 0)
                {
                    stats.SetPM(stats.currentPM - 1);
                    if (popup != null) popup.ShowPM(1);
                }

                // Petite pause lisible (évite téléport visuelle)
                if (afterMoveDelaySec > 0f) yield return new WaitForSeconds(afterMoveDelaySec);
                continue;
            }

            if (action.type == AIActionType.Cast && action.skill != null && action.targetTile != null)
            {
                // Équipe et caste via l’API du caster (gère le FX selon waitFxBeforeApply)
                caster.EquipSkill(action.skill);
                yield return caster.CastAtTileWithFx(action.skill, action.targetTile);

                // Micro-pause pour lisibilité (la timeline est rafraîchie dans le caster)
                yield return new WaitForSeconds(0.05f);
                continue;
            }

            // Action invalide -> on coupe
            if (verboseLog) Debug.LogWarning($"[{name}] Action IA invalide -> fin de tour.");
            break;
        }

        // Hook IA fin de tour
        behavior.OnTurnEnd(ctx);

        // Fin de tour: assure-toi de ne pas couper pendant un mouvement
        if (!isMoving && phaseManager != null && phaseManager.phaseTurn != null)
            phaseManager.phaseTurn.EndTurn();

        isPlayingTurn = false;
    }

    // =====================================================================
    // ==============         UTILITAIRES COMMUNS IA         ===============
    // =====================================================================

    /// <summary>  Renvoie l’ennemi vivant le plus proche (distance Manhattan).</summary>
    public GameObject GetNearestEnemy()
    {
        var enemies = GetOpponents();
        if (enemies == null || enemies.Count == 0) return null;

        var myTile = tileGrid.GetTileOfEntity(gameObject);
        if (!myTile || !myTile.TryGetComponent(out SetupTile myS)) return null;
        Vector2Int my = new(myS.tileX, myS.tileY);

        GameObject best = null;
        int bestDist = int.MaxValue;
        for (int i = 0; i < enemies.Count; i++)
        {
            var e = enemies[i];
            if (!e || !e.TryGetComponent(out Entity_StatistiqueCombat es) || es.isDead) continue;
            var et = tileGrid.GetTileOfEntity(e);
            if (!et || !et.TryGetComponent(out SetupTile esu)) continue;

            int d = Mathf.Abs(my.x - esu.tileX) + Mathf.Abs(my.y - esu.tileY);
            if (d < bestDist) { bestDist = d; best = e; }
        }
        return best;
    }

    /// <summary>  Retourne la distance Manhattan entre soi et la cible.</summary>
    public int GetDistanceTo(GameObject entity)
    {
        var a = tileGrid.GetTileOfEntity(gameObject);
        var b = tileGrid.GetTileOfEntity(entity);
        if (!a || !b) return int.MaxValue;
        var sa = a.GetComponent<SetupTile>();
        var sb = b.GetComponent<SetupTile>();
        return Mathf.Abs(sa.tileX - sb.tileX) + Mathf.Abs(sa.tileY - sb.tileY);
    }

    /// <summary>  Liste des adversaires dynamiquement, selon ma team (aucun hardcode Rouge/Verte).</summary>
    public List<GameObject> GetOpponents()
    {
        return (stats != null && stats.team == 0)
            ? phaseManager.phaseEnter.redTeam
            : phaseManager.phaseEnter.greenTeam;
    }

    /// <summary>  Liste des alliés dynamiquement, selon ma team.</summary>
    public List<GameObject> GetAllies()
    {
        return (stats != null && stats.team == 0)
            ? phaseManager.phaseEnter.greenTeam
            : phaseManager.phaseEnter.redTeam;
    }

    /// <summary>  Sélectionne les meilleurs skills d'attaque (mêlée et distance) depuis le skillBook.</summary>
    public void GetBestAttackSkills(out Data_Skill bestMelee, out Data_Skill bestRanged)
    {
        bestMelee = null;
        bestRanged = null;

        // Construit une liste de Data_Skill à partir des Skill_Binding
        List<Data_Skill> book = new List<Data_Skill>();
        if (stats != null && stats.skillBook != null)
        {
            for (int i = 0; i < stats.skillBook.Count; i++)
            {
                var b = stats.skillBook[i];
                if (b != null && b.skill != null) book.Add(b.skill);
            }
        }

        // Fallback: on utilise la skill équipée si jamais la liste est vide
        if (book.Count == 0 && caster != null && caster.equippedSkill != null)
            book.Add(caster.equippedSkill);

        for (int i = 0; i < book.Count; i++)
        {
            var sk = book[i];
            if (!sk || sk.skillType != SkillType.Attack) continue;

            if (sk.rangeMax <= 1)
            {
                if (bestMelee == null || sk.damageMax > bestMelee.damageMax) bestMelee = sk;
            }
            else
            {
                if (bestRanged == null || sk.damageMax > bestRanged.damageMax) bestRanged = sk;
            }
        }
    }

    /// <summary>  Teste si un skill peut être lancé sur la tuile de l'entité 'target' (mono-cible).</summary>
    public bool CanCastOnEntity(Data_Skill skill, GameObject target)
    {
        if (!skill || !target) return false;
        var tile = tileGrid.GetTileOfEntity(target);
        if (!tile) return false;
        return caster.CanCastAtTile(skill, tile, out _);
    }

    /// <summary>  Renvoie la "meilleure" tuile voisine libre qui rapproche de 'target'. Null si bloqué.</summary>
    public GameObject GetBestNeighborTowards(GameObject target)
    {
        var myT = tileGrid.GetTileOfEntity(gameObject);
        var tgT = tileGrid.GetTileOfEntity(target);
        if (!myT || !tgT) return null;

        var myS = myT.GetComponent<SetupTile>();
        var tgS = tgT.GetComponent<SetupTile>();
        int bestDist = Mathf.Abs(myS.tileX - tgS.tileX) + Mathf.Abs(myS.tileY - tgS.tileY);

        Vector2Int[] dirs = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };
        GameObject best = null;

        for (int i = 0; i < dirs.Length; i++)
        {
            int nx = myS.tileX + dirs[i].x;
            int ny = myS.tileY + dirs[i].y;
            var n = tileGrid.GetTileAtCoordinates(nx, ny);
            if (!n) continue;
            if (!tileGrid.IsTileFree(n)) continue;

            int d = Mathf.Abs(nx - tgS.tileX) + Mathf.Abs(ny - tgS.tileY);
            if (d < bestDist) { bestDist = d; best = n; }
        }
        return best; // peut être null (bloqué)
    }

    /// <summary>
    ///   Choisit, parmi MES 4 voisins libres, la tuile qui RAPPROCHE le plus d'une tuile cible 'goalTile'.
    /// Retourne null si bloqué.
    /// </summary>
    public GameObject GetBestNeighborTowardsTile(GameObject goalTile)
    {
        var myT = tileGrid.GetTileOfEntity(gameObject);
        if (!myT || !goalTile) return null;

        var myS = myT.GetComponent<SetupTile>();
        var gS = goalTile.GetComponent<SetupTile>();
        if (!myS || !gS) return null;

        int bestDist = Mathf.Abs(myS.tileX - gS.tileX) + Mathf.Abs(myS.tileY - gS.tileY);
        GameObject best = null;

        Vector2Int[] dirs = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };
        for (int i = 0; i < dirs.Length; i++)
        {
            int nx = myS.tileX + dirs[i].x;
            int ny = myS.tileY + dirs[i].y;
            var n = tileGrid.GetTileAtCoordinates(nx, ny);
            if (!n) continue;
            if (!tileGrid.IsTileFree(n)) continue;

            int d = Mathf.Abs(nx - gS.tileX) + Mathf.Abs(ny - gS.tileY);
            if (d < bestDist) { bestDist = d; best = n; }
        }
        return best;
    }

    // ---------------------------------------------------------------------
    /// <summary>  Un pas visuel vers 'tile' + mise à jour de la grille (verrou Y).</summary>
    public IEnumerator StepToTile(GameObject tile)
    {
        if (!tile) yield break;
        isMoving = true;

        Vector3 start = transform.position;
        Vector3 end = tile.transform.position;

        // Verrouillage axe vertical : on conserve le Y de départ (pas de mouvement en hauteur)
        end.y = start.y;

        float t = 0f;
        float dur = Mathf.Max(0.01f, stepDuration);
        while (t < 1f)
        {
            t += Time.deltaTime / dur;

            // Interpolation lissée sur X/Z, Y reste constant
            Vector3 pos = Vector3.Lerp(start, end, Mathf.Clamp01(t));
            pos.y = start.y;
            transform.position = pos;

            yield return null;
        }

        // Position finale (Y toujours verrouillé)
        transform.position = end;

        // Mise à jour mapping entité ↔ tuile
        tileGrid.RegisterEntity(gameObject, tile);
        isMoving = false;
    }
}

// ========================================================================
// ======================   CONTRAT & CONTEXTE IA   =======================
// ========================================================================

public enum MonsterAIType { Agressif, Passif, Fuyard }

public enum AIActionType { None, MoveStep, Cast, EndTurn }

/// <summary>
///   Action renvoyée par une IA : un pas, un cast, ou fin de tour.
/// </summary>
public struct AIAction
{
    public AIActionType type;
    public Data_Skill skill;      // pour Cast
    public GameObject targetTile; // pour MoveStep / Cast (mono)

    public static AIAction End() => new AIAction { type = AIActionType.EndTurn };
    public static AIAction MoveTo(GameObject tile) => new AIAction { type = AIActionType.MoveStep, targetTile = tile };
    public static AIAction Cast(Data_Skill s, GameObject tile) => new AIAction { type = AIActionType.Cast, skill = s, targetTile = tile };
}

/// <summary>
///   Contexte fourni à l'IA (réfs + helpers via le contrôleur).
/// </summary>
public class AIContext
{
    public Monster_CombatController controller;
    public Combat_PhaseManager phaseManager;
    public TileGrid_Manager tileGrid;
    public Entity_StatistiqueCombat stats;
    public Entity_SkillCaster caster;

    // Mémoire transitoire (ex: cible courante)
    public GameObject currentTarget;

    public AIContext(Monster_CombatController c, Combat_PhaseManager pm, TileGrid_Manager grid, Entity_StatistiqueCombat s, Entity_SkillCaster cast)
    {
        controller = c; phaseManager = pm; tileGrid = grid; stats = s; caster = cast;
    }

    // Helpers exposés aux IA (wrappers vers le contrôleur)
    public GameObject GetNearestEnemy() => controller.GetNearestEnemy();
    public int GetDistanceTo(GameObject e) => controller.GetDistanceTo(e);
    public void GetBestAttackSkills(out Data_Skill melee, out Data_Skill ranged) => controller.GetBestAttackSkills(out melee, out ranged);
    public bool CanCastOnEntity(Data_Skill sk, GameObject target) => controller.CanCastOnEntity(sk, target);
    public GameObject GetBestNeighborTowards(GameObject target) => controller.GetBestNeighborTowards(target);
    public GameObject GetBestNeighborTowardsTile(GameObject goalTile) => controller.GetBestNeighborTowardsTile(goalTile);
    public List<GameObject> GetOpponents() => controller.GetOpponents();
    public List<GameObject> GetAllies() => controller.GetAllies();
}

/// <summary>
///   Contrat minimal d’une IA de monstre.
/// </summary>
public interface IMonsterAI
{
    void OnTurnStart(AIContext ctx);          // Init / choix de cible initial
    AIAction DecideNextAction(AIContext ctx); // Renvoie l'action suivante à exécuter
    void OnTurnEnd(AIContext ctx);            // Nettoyage si besoin
}
