using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Monster_CombatController
//  Contrôle le tour du monstre, les déplacements pas-à-pas, le déclenchement des compétences,
//  et expose des helpers pour l'IA.
public class Monster_CombatController : MonoBehaviour
{
    [Header("Références (assignées au spawn / prefab)")]
    public Combat_PhaseManager phaseManager;   // Référence vers le gestionnaire de phases de combat
    public TileGrid_Manager tileGrid;          // Référence vers la grille de combat
    public Entity_StatistiqueCombat stats;     // Statistiques de l'entité (HP/PA/PM/Team/skillBook)
    public Entity_SkillCaster caster;          // Lanceur de compétences commun
    public Entity_Animations anim;             // Contrôleur d’animations 3D (PlayWalk/StopWalk/etc.)

    [Header("IA (plug-in)")]
    [Tooltip("Assigner un composant qui implémente IMonsterAI (ex: IA_Aggressif).")]
    public MonoBehaviour behaviorComponent;    // Doit implémenter IMonsterAI
    private IMonsterAI behavior;               // Interface IA
    public MonsterAIType typeIA;               // Indication dans l'inspector

    [Header("Tempo")]
    public float startDelaySec = 1.0f;         // Délai au début du tour (lisibilité)
    public float afterMoveDelaySec = 0.1f;     // Petite pause après un pas
    public float stepDuration = 0.2f;          // Durée d’un pas (lerp d'une tuile)

    [Header("Orientation")]
    public bool rotateTowardsTarget = true;    // Si true, on oriente le monstre vers la tuile visée
    public bool instantTurnAtStepStart = true; // Si true, on pivote instantanément au début du pas
    public float rotateSpeedDeg = 540f;        // Vitesse de rotation si pivot non instantané (degrés/s)
    public float rotationOffsetY = 0f;         // Offset Y pour corriger un prefab mal orienté

    [Header("Debug")]
    public bool verboseLog = false;

    // Flag utilisé par la phase (évite de finir le tour pendant un déplacement)
    public bool isMoving { get; private set; } = false;

    // Routine unique par tour
    private bool isPlayingTurn = false;

    // Pop-up (affichage PM consommés, etc.)
    private Popup_DisplayNumber popup;

    private void Awake()
    {
        // Récupération de composants locaux si non assignés
        if (!stats) TryGetComponent(out stats);
        if (!caster) TryGetComponent(out caster);
        if (!popup) TryGetComponent(out popup);
        if (!anim) TryGetComponent(out anim); // Permet d'utiliser PlayWalk/StopWalk sans NullRef

        // Vérification du composant IA
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

        // Injection douce des références côté caster si elles sont vides
        if (caster != null)
        {
            if (!caster.phaseManager) caster.phaseManager = phaseManager;
            if (!caster.tileGrid) caster.tileGrid = tileGrid;
        }
    }

    private void Update()
    {
        // Sécurités d'usage
        if (phaseManager == null || phaseManager.phaseTurn == null || tileGrid == null || stats == null || caster == null || behavior == null)
            return;
        if (!phaseManager.isInCombat || stats.isDead) return;

        // Déclencher un tour uniquement si c'est à moi et que je ne joue pas déjà
        if (phaseManager.phaseTurn.IsMyTurn(gameObject) && !isPlayingTurn)
            StartCoroutine(PlayTurnRoutine());
    }

    private IEnumerator PlayTurnRoutine()
    {
        isPlayingTurn = true;

        // Délai d'entrée de tour (lisibilité)
        if (startDelaySec > 0f) yield return new WaitForSeconds(startDelaySec);

        // Prépare le contexte IA
        var ctx = new AIContext(this, phaseManager, tileGrid, stats, caster);

        // Hook IA début de tour
        behavior.OnTurnStart(ctx);

        // Boucle de décisions
        int guard = 16; // sécurité
        while (guard-- > 0)
        {
            AIAction action = behavior.DecideNextAction(ctx);

            if (action.type == AIActionType.EndTurn)
                break;

            if (action.type == AIActionType.MoveStep && action.targetTile != null)
            {
                // Un pas vers la tuile
                yield return StepToTile(action.targetTile);

                // Consommation d'1 PM et feedback
                if (stats.currentPM > 0)
                {
                    stats.SetPM(stats.currentPM - 1);
                    if (popup != null) popup.ShowPM(1);
                }

                // Petite pause lisible
                if (afterMoveDelaySec > 0f) yield return new WaitForSeconds(afterMoveDelaySec);
                continue;
            }

            if (action.type == AIActionType.Cast && action.skill != null && action.targetTile != null)
            {
                // Lancement de sort via le caster (gère ses tempos/fx)
                caster.EquipSkill(action.skill);
                yield return caster.CastAtTileWithFx(action.skill, action.targetTile);

                // Micro-pause pour lisibilité
                yield return new WaitForSeconds(0.05f);
                continue;
            }

            // Action invalide -> fin
            if (verboseLog) Debug.LogWarning($"[{name}] Action IA invalide -> fin de tour.");
            break;
        }

        // Hook IA fin de tour
        behavior.OnTurnEnd(ctx);

        // Sécurité animation : s'assurer que la marche est stoppée
        if (anim != null && anim.isActiveAndEnabled)
            anim.StopWalk();

        // Fin de tour si pas de mouvement en cours
        if (!isMoving && phaseManager != null && phaseManager.phaseTurn != null)
            phaseManager.phaseTurn.EndTurn();

        isPlayingTurn = false;
    }

    // =====================================================================
    // =====================    HELPERS ET OUTILS IA    ====================
    // =====================================================================

    // Renvoie l’ennemi vivant le plus proche en distance Manhattan
    public GameObject GetNearestEnemy()
    {
        var enemies = GetOpponents();
        if (enemies == null || enemies.Count == 0) return null;

        var myTile = tileGrid.GetTileOfEntity(gameObject);
        if (!myTile || !myTile.TryGetComponent(out SetupTile myS)) return null;
        Vector2Int my = new Vector2Int(myS.tileX, myS.tileY);

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

    // Distance Manhattan jusqu'à une entité cible
    public int GetDistanceTo(GameObject entity)
    {
        var a = tileGrid.GetTileOfEntity(gameObject);
        var b = tileGrid.GetTileOfEntity(entity);
        if (!a || !b) return int.MaxValue;
        var sa = a.GetComponent<SetupTile>();
        var sb = b.GetComponent<SetupTile>();
        return Mathf.Abs(sa.tileX - sb.tileX) + Mathf.Abs(sa.tileY - sb.tileY);
    }

    // Liste des adversaires selon ma team
    public List<GameObject> GetOpponents()
    {
        return (stats != null && stats.team == 0)
            ? phaseManager.phaseEnter.redTeam
            : phaseManager.phaseEnter.greenTeam;
    }

    // Liste des alliés selon ma team
    public List<GameObject> GetAllies()
    {
        return (stats != null && stats.team == 0)
            ? phaseManager.phaseEnter.greenTeam
            : phaseManager.phaseEnter.redTeam;
    }

    // Sélectionne des skills d'attaque : meilleur mêlée et meilleur distance
    public void GetBestAttackSkills(out Data_Skill bestMelee, out Data_Skill bestRanged)
    {
        bestMelee = null;
        bestRanged = null;

        List<Data_Skill> book = new List<Data_Skill>();
        if (stats != null && stats.skillBook != null)
        {
            for (int i = 0; i < stats.skillBook.Count; i++)
            {
                var b = stats.skillBook[i];
                if (b != null && b.skill != null) book.Add(b.skill);
            }
        }

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

    // Teste si un skill peut être lancé sur la tuile d'une entité cible
    public bool CanCastOnEntity(Data_Skill skill, GameObject target)
    {
        if (!skill || !target) return false;
        var tile = tileGrid.GetTileOfEntity(target);
        if (!tile) return false;
        return caster.CanCastAtTile(skill, tile, out _);
    }

    // Renvoie la meilleure tuile voisine libre qui rapproche d'une entité cible
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
        return best;
    }

    // Choisit la meilleure tuile voisine libre pour se rapprocher d'une tuile objectif
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

    // Un pas visuel vers la tuile donnée + mise à jour de la grille + rotation vers la cible
    public IEnumerator StepToTile(GameObject tile)
    {
        if (!tile) yield break;

        isMoving = true;

        // Lance l'animation de marche au début du pas
        if (anim != null && anim.isActiveAndEnabled)
            anim.PlayWalk();

        Vector3 start = transform.position;
        Vector3 end = tile.transform.position;

        // Verrouillage vertical : on conserve le Y de départ
        end.y = start.y;

        // Orientation initiale (instantanée ou non) vers la tuile cible
        if (rotateTowardsTarget)
        {
            RotateTowards(end, instantTurnAtStepStart);
        }

        float t = 0f;
        float dur = Mathf.Max(0.01f, stepDuration);
        while (t < 1f)
        {
            t += Time.deltaTime / dur;

            // Interpolation lissée sur X/Z
            Vector3 pos = Vector3.Lerp(start, end, Mathf.Clamp01(t));
            pos.y = start.y;
            transform.position = pos;

            // Maintient l'état de marche tant que l'on se déplace
            if (anim != null && anim.isActiveAndEnabled)
                anim.SetWalk(true);

            // Si on n'a pas pivoté instantanément, on continue de tourner progressivement vers la cible
            if (rotateTowardsTarget && !instantTurnAtStepStart)
            {
                RotateTowards(end, false);
            }

            yield return null;
        }

        // Position finale
        transform.position = end;

        // Mise à jour mapping entité ↔ tuile
        tileGrid.RegisterEntity(gameObject, tile);

        isMoving = false;

        // Stoppe l'animation de marche une fois la case atteinte
        if (anim != null && anim.isActiveAndEnabled)
            anim.StopWalk();
    }

    // Oriente le GameObject vers une position monde cible (Y ignoré), avec option instantanée
    // - Calcul du yaw sur le plan XZ
    // - Ajout d'un offset Y pour corriger l'orientation de base du prefab
    private void RotateTowards(Vector3 worldTarget, bool instant)
    {
        Vector3 dir = worldTarget - transform.position;
        dir.y = 0f; // ignore la pente verticale

        // Si la direction est trop faible, on évite les NaN
        if (dir.sqrMagnitude < 0.000001f) return;

        float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        yaw += rotationOffsetY;

        Quaternion targetRot = Quaternion.Euler(0f, yaw, 0f);

        if (instant || rotateSpeedDeg <= 0f)
        {
            // Pivot franc immédiat
            transform.rotation = targetRot;
        }
        else
        {
            // Rotation progressive
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                rotateSpeedDeg * Time.deltaTime
            );
        }
    }
}

// Types et interfaces liés à l'IA
public enum MonsterAIType { Agressif, Passif, Fuyard }
public enum AIActionType { None, MoveStep, Cast, EndTurn }

// Action retournée par l'IA
public struct AIAction
{
    public AIActionType type;
    public Data_Skill skill;      // pour Cast
    public GameObject targetTile; // pour MoveStep / Cast

    public static AIAction End() => new AIAction { type = AIActionType.EndTurn };
    public static AIAction MoveTo(GameObject tile) => new AIAction { type = AIActionType.MoveStep, targetTile = tile };
    public static AIAction Cast(Data_Skill s, GameObject tile) => new AIAction { type = AIActionType.Cast, skill = s, targetTile = tile };
}

// Contexte fourni à l'IA (références + helpers)
public class AIContext
{
    public Monster_CombatController controller;
    public Combat_PhaseManager phaseManager;
    public TileGrid_Manager tileGrid;
    public Entity_StatistiqueCombat stats;
    public Entity_SkillCaster caster;

    public GameObject currentTarget; // mémoire transitoire IA

    public AIContext(Monster_CombatController c, Combat_PhaseManager pm, TileGrid_Manager grid, Entity_StatistiqueCombat s, Entity_SkillCaster cast)
    {
        controller = c; phaseManager = pm; tileGrid = grid; stats = s; caster = cast;
    }

    public GameObject GetNearestEnemy() => controller.GetNearestEnemy();
    public int GetDistanceTo(GameObject e) => controller.GetDistanceTo(e);
    public void GetBestAttackSkills(out Data_Skill melee, out Data_Skill ranged) => controller.GetBestAttackSkills(out melee, out ranged);
    public bool CanCastOnEntity(Data_Skill sk, GameObject target) => controller.CanCastOnEntity(sk, target);
    public GameObject GetBestNeighborTowards(GameObject target) => controller.GetBestNeighborTowards(target);
    public GameObject GetBestNeighborTowardsTile(GameObject goalTile) => controller.GetBestNeighborTowardsTile(goalTile);
    public List<GameObject> GetOpponents() => controller.GetOpponents();
    public List<GameObject> GetAllies() => controller.GetAllies();
}

// Interface minimale pour une IA de monstre
public interface IMonsterAI
{
    void OnTurnStart(AIContext ctx);          // Initialisation du tour
    AIAction DecideNextAction(AIContext ctx); // Choix de l'action suivante
    void OnTurnEnd(AIContext ctx);            // Nettoyage de fin de tour
}
