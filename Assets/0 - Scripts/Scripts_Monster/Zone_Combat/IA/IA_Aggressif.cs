using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// IA_Aggressif
///   Vise l'adjacence (Manhattan=1) à un ennemi, puis attaque avec la source de dégâts la plus élevée
///      selon le SkillBook (PA restants, portée, nombre de cibles touchées par la zone).
///      - Si déjà adjacent à un ennemi → ne bouge pas, tente d'attaquer.
///      - Sinon cherche une case LIBRE parmi les 4 autour de la cible et avance vers elle (un pas).
///      - Si aucune case adjacente libre ET/OU bloqué → tente un tir à distance si possible.
/// </summary>
public class IA_Aggressif : MonoBehaviour, IMonsterAI
{
    public IA_Aggressif() { }
    ~IA_Aggressif() { }

    public void OnTurnStart(AIContext ctx)
    {
        //   Cible par défaut : ennemi le plus proche
        ctx.currentTarget = ctx.GetNearestEnemy();
    }

    public AIAction DecideNextAction(AIContext ctx)
    {
        if (ctx.currentTarget == null) return AIAction.End();

        //   1) Si un ennemi est déjà adjacent (Manhattan=1), on ne bouge pas
        bool isAdjacent = GetAnyAdjacentEnemy(ctx) != null;

        //   2) S'il n'est PAS adjacent : viser une CASE ADJACENTE LIBRE de la cible
        if (!isAdjacent)
        {
            GameObject goalAdjTile = FindBestFreeAdjacentTile(ctx, ctx.currentTarget);

            //   a) Si une case adjacente libre existe et qu'on a des PM → un pas vers CETTE case
            if (goalAdjTile != null && ctx.stats.currentPM > 0)
            {
                var step = ctx.controller.GetBestNeighborTowardsTile(goalAdjTile);
                if (step != null) return AIAction.MoveTo(step);
            }

            //   b) Sinon (aucune case libre ou PM=0) → on autorise un tir à distance s'il est possible
            var rangedTry = ChooseBestCastOption(ctx, ctx.currentTarget);
            if (rangedTry.type == AIActionType.Cast)
                return rangedTry;

            //   c) Rien à faire
            return AIAction.End();
        }

        //   3) Déjà adjacent → on tente d'attaquer (meilleur DPS)
        var bestCast = ChooseBestCastOption(ctx, ctx.currentTarget);
        if (bestCast.type == AIActionType.Cast)
            return bestCast;

        return AIAction.End();
    }

    public void OnTurnEnd(AIContext ctx) { }

    // =====================================================================
    // ============================  HELPERS  ===============================
    // =====================================================================

    /// <summary>
    ///   Retourne un ennemi adjacent (Manhattan=1) s'il existe, sinon null.
    /// </summary>
    private GameObject GetAnyAdjacentEnemy(AIContext ctx)
    {
        int myTeam = ctx.stats.team;
        List<GameObject> enemies = (myTeam == 0) ? ctx.phaseManager.phaseEnter.redTeam
                                                 : ctx.phaseManager.phaseEnter.greenTeam;
        if (enemies == null || enemies.Count == 0) return null;

        var myTile = ctx.tileGrid.GetTileOfEntity(ctx.stats.gameObject);
        if (!myTile || !myTile.TryGetComponent(out SetupTile myS)) return null;
        Vector2Int my = new Vector2Int(myS.tileX, myS.tileY);

        for (int i = 0; i < enemies.Count; i++)
        {
            var e = enemies[i];
            if (!e) continue;
            if (!e.TryGetComponent(out Entity_StatistiqueCombat es) || es.isDead) continue;

            var et = ctx.tileGrid.GetTileOfEntity(e);
            if (!et || !et.TryGetComponent(out SetupTile esu)) continue;

            int d = Mathf.Abs(my.x - esu.tileX) + Mathf.Abs(my.y - esu.tileY);
            if (d == 1) return e; //   Adjacent (4-dir)
        }
        return null;
    }

    /// <summary>
    ///   Trouve la meilleure CASE ADJACENTE (4-dir) LIBRE autour de 'targetEntity',
    ///      en privilégiant celle la plus proche du monstre.
    /// </summary>
    private GameObject FindBestFreeAdjacentTile(AIContext ctx, GameObject targetEntity)
    {
        if (!targetEntity) return null;

        var tTile = ctx.tileGrid.GetTileOfEntity(targetEntity);
        var mTile = ctx.tileGrid.GetTileOfEntity(ctx.stats.gameObject);
        if (!tTile || !mTile) return null;

        var ts = tTile.GetComponent<SetupTile>();
        var ms = mTile.GetComponent<SetupTile>();
        if (!ts || !ms) return null;

        Vector2Int[] dirs = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };

        GameObject best = null;
        int bestDist = int.MaxValue;

        for (int i = 0; i < dirs.Length; i++)
        {
            int ax = ts.tileX + dirs[i].x;
            int ay = ts.tileY + dirs[i].y;
            var adj = ctx.tileGrid.GetTileAtCoordinates(ax, ay);
            if (!adj) continue;
            if (!ctx.tileGrid.IsTileFree(adj)) continue; //   doit être LIBRE

            int d = Mathf.Abs(ms.tileX - ax) + Mathf.Abs(ms.tileY - ay);
            if (d < bestDist) { bestDist = d; best = adj; }
        }

        return best; //   null si aucune des 4 cases n'est libre
    }

    /// <summary>
    ///   Évalue toutes les attaques disponibles et renvoie le meilleur "Cast" contre targetEntity
    ///      (score = dégâts moyens × nb d'ennemis touchés par la zone centrée sur la tuile de la cible).
    /// </summary>
    private AIAction ChooseBestCastOption(AIContext ctx, GameObject targetEntity)
    {
        if (!targetEntity) return AIAction.End();

        //   Récupère les Data_Skill depuis le SkillBook (List<Skill_Binding>)
        List<Data_Skill> book = BuildSkillListFromBindings(ctx);
        if (book == null || book.Count == 0)
        {
            book = new List<Data_Skill>();
            if (ctx.caster.equippedSkill) book.Add(ctx.caster.equippedSkill);
        }

        var targetTile = ctx.tileGrid.GetTileOfEntity(targetEntity);
        if (!targetTile) return AIAction.End();

        float bestScore = float.NegativeInfinity;
        Data_Skill bestSkill = null;

        for (int i = 0; i < book.Count; i++)
        {
            var sk = book[i];
            if (!sk) continue;
            if (sk.skillType != SkillType.Attack) continue;
            if (ctx.stats.currentPA < sk.costPA) continue; //   PA

            if (!ctx.caster.CanCastAtTile(sk, targetTile, out _)) continue;

            float score = EstimateDamageScore(ctx, sk, targetTile);
            if (score <= 0f) continue;

            if (score > bestScore) { bestScore = score; bestSkill = sk; }
        }

        if (bestSkill != null)
            return AIAction.Cast(bestSkill, targetTile);

        return AIAction.End();
    }

    /// <summary>
    ///   Construit une liste de Data_Skill à partir des Skill_Binding du monstre.
    /// </summary>
    private List<Data_Skill> BuildSkillListFromBindings(AIContext ctx)
    {
        var res = new List<Data_Skill>();
        if (ctx.stats != null && ctx.stats.skillBook != null)
        {
            for (int i = 0; i < ctx.stats.skillBook.Count; i++)
            {
                var b = ctx.stats.skillBook[i];
                if (b != null && b.skill != null) res.Add(b.skill);
            }
        }
        return res;
    }

    /// <summary>
    ///   Score dégât = moyenne (min+max)/2 × nb d'ennemis touchés par la zone centrée sur targetTile.
    /// </summary>
    private float EstimateDamageScore(AIContext ctx, Data_Skill skill, GameObject targetTile)
    {
        if (skill == null || targetTile == null) return 0f;
        float avg = (skill.damageMin + skill.damageMax) * 0.5f;
        int enemiesHit = CountEnemiesInZone(ctx, skill, targetTile);
        if (enemiesHit <= 0) return 0f;
        return avg * enemiesHit;
    }

    private int CountEnemiesInZone(AIContext ctx, Data_Skill skill, GameObject targetTile)
    {
        if (!targetTile.TryGetComponent(out SetupTile center)) return 0;

        bool isMono = (skill.impactZone == null || skill.impactZone.zone == null
                       || skill.impactZone.zone.Length == 0
                       || (skill.impactZone.zone.Length == 1 && skill.impactZone.zone[0] == Vector2Int.zero)
                       || skill.impactZone.zone.Length == 1);

        if (isMono)
        {
            var occ = ctx.tileGrid.GetEntityOnTile(targetTile);
            if (!occ) return 0;
            var os = occ.GetComponent<Entity_StatistiqueCombat>();
            return (os != null && os.team != ctx.stats.team && !os.isDead) ? 1 : 0;
        }

        int count = 0;
        var zone = skill.impactZone.zone;
        for (int i = 0; i < zone.Length; i++)
        {
            int tx = center.tileX + zone[i].x;
            int ty = center.tileY + zone[i].y;
            var t = ctx.tileGrid.GetTileAtCoordinates(tx, ty);
            if (!t) continue;

            var occ = ctx.tileGrid.GetEntityOnTile(t);
            if (!occ) continue;

            var os = occ.GetComponent<Entity_StatistiqueCombat>();
            if (os != null && os.team != ctx.stats.team && !os.isDead)
                count++;
        }
        return count;
    }
}
