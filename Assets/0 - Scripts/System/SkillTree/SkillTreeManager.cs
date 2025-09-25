using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

// alias propre pour éviter l'ambiguïté "Object"
using UnityObject = UnityEngine.Object;

public class SkillTreeManager : MonoBehaviour
{
    public int availablePoints = 5;
    public List<SkillTreeBranch> branches;
    public GameObject skillButtonPrefab;
    public Transform skillListParent;
    public SkillPointsUI skillPointsUI;

    [Tooltip("Reference to the player's stats (used to check level). Assign in inspector or it will auto-find on Start).")]
    public Entity_Info entityInfo;

    private List<SkillButtonUI> allSkillButtons = new List<SkillButtonUI>();

    void Awake()
    {
        if (entityInfo == null)
        {
            // 1) Essaye par tag "Player" (recommande d'ajouter ce tag sur ton joueur)
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO != null)
                entityInfo = playerGO.GetComponent<Entity_Info>();

            // 2) Si Mirror est utilisé et qu'on est client, récupère l'identité locale si possible
#if MIRROR
        if (entityInfo == null && NetworkClient.active && NetworkClient.connection != null && NetworkClient.connection.identity != null)
        {
            var localGO = NetworkClient.connection.identity.gameObject;
            entityInfo = localGO.GetComponent<Entity_Info>() ?? entityInfo;
        }
#endif

            // 3) Fallback général (prend la première instance trouvée si rien d'autre)
            if (entityInfo == null)
                entityInfo = FindObjectOfType<Entity_Info>();

            Debug.Log($"[SkillTreeManager.Awake] entityInfo assigné -> {(entityInfo != null ? entityInfo.name : "NULL")}");
        }
    }


    void Start()
    {
        allSkillButtons.Clear();

        GameObject row = new GameObject("BranchesRow");
        row.transform.SetParent(skillListParent, false);
        var hLayout = row.AddComponent<HorizontalLayoutGroup>();
        hLayout.childForceExpandWidth = false;
        hLayout.childForceExpandHeight = true;
        hLayout.spacing = 150;
        var fitter = row.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        foreach (SkillTreeBranch branch in branches)
        {
            GameObject branchPanel = new GameObject(branch.branchName);
            branchPanel.transform.SetParent(row.transform, false);
            RectTransform rect = branchPanel.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300, 0);

            VerticalLayoutGroup vLayout = branchPanel.AddComponent<VerticalLayoutGroup>();
            vLayout.childAlignment = TextAnchor.UpperCenter;
            vLayout.childForceExpandHeight = false;
            vLayout.childForceExpandWidth = true;
            vLayout.spacing = 50;

            ContentSizeFitter fitterBranch = branchPanel.AddComponent<ContentSizeFitter>();
            fitterBranch.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            GameObject labelGO = new GameObject("Label");
            labelGO.transform.SetParent(branchPanel.transform, false);
            Text label = labelGO.AddComponent<Text>();
            label.text = branch.branchName.ToUpper();
            label.alignment = TextAnchor.MiddleCenter;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 24;

            foreach (SkillNode node in branch.nodes)
            {
                GameObject go = Instantiate(skillButtonPrefab, branchPanel.transform);
                SkillButtonUI btnUI = go.GetComponent<SkillButtonUI>();
                btnUI.Initialize(node, this);
                allSkillButtons.Add(btnUI);

                // Forcer le premier skill de chaque branche à être débloqué runtime
                if (branch.nodes.IndexOf(node) == 0)
                {
                    node.isUnlockedRuntime = true;
                    node.onUnlock?.Invoke();
                }
            }
        }

        SyncAllUnlockedSkills();
    }

    void Update()
    {
        UpdateAllSkillButtons();
    }


    private Entity_StatistiqueCombat GetCombatStats()
    {
        if (entityInfo != null)
            return entityInfo.GetComponent<Entity_StatistiqueCombat>() ?? FindObjectOfType<Entity_StatistiqueCombat>();
        return FindObjectOfType<Entity_StatistiqueCombat>();
    }

    // ---------------- UTILS DE CLONAGE GÉNÉRIQUE ----------------
    private T TryClone<T>(T original)
    {
        if (original == null) return default;

        object o = original;
        UnityObject uo = o as UnityObject;
        if (uo != null)
        {
            try
            {
                var instObj = UnityObject.Instantiate(uo);
                if (instObj is T tInst)
                    return tInst;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SkillTreeManager] Instantiate failed: {e.Message}");
            }
        }

        MethodInfo mi = original.GetType().GetMethod("Clone", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
        if (mi != null)
        {
            try
            {
                object cloned = mi.Invoke(original, null);
                if (cloned is T t) return t;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SkillTreeManager] Clone() a lancé une exception : {e.Message}");
            }
        }

        return original;
    }

    // ---------------- CLONAGE ET APPLICATION DES OVERRIDES ----------------
    private Data_Skill CloneSkill(Data_Skill original)
    {
        if (original == null) return null;

        Data_Skill clone = ScriptableObject.CreateInstance<Data_Skill>();
        clone.name = original.name + "_runtime";
        clone.ID = original.ID;
        clone.skillName = original.skillName;
        clone.description = original.description;
        clone.skillType = original.skillType;
        clone.skillElement = original.skillElement;
        clone.damageMin = original.damageMin;
        clone.damageMax = original.damageMax;
        clone.costPA = original.costPA;
        clone.rangeMin = original.rangeMin;
        clone.rangeMax = original.rangeMax;
        clone.cooldown = original.cooldown;
        clone.maxPerTargetPerTurn = original.maxPerTargetPerTurn;

        clone.impactZone = TryClone(original.impactZone);

        object izOrigObj = original.impactZone;
        UnityObject origIzUo = izOrigObj as UnityObject;
        object izCloneObj = clone.impactZone;
        UnityObject izCloneUo = izCloneObj as UnityObject;
        if (origIzUo != null && izCloneUo != null)
        {
            try { izCloneUo.name = origIzUo.name + "_runtime"; } catch { }
        }

        clone.effects = new List<SkillEffect>(original.effects.Count);
        foreach (var e in original.effects)
        {
            var cloneE = TryClone(e);
            clone.effects.Add(cloneE);
        }

        clone.critChance = original.critChance;

        clone.critEffects = new List<SkillEffect>(original.critEffects.Count);
        foreach (var ce in original.critEffects)
        {
            var cloneCe = TryClone(ce);
            clone.critEffects.Add(cloneCe);
        }

        clone.fxData = TryClone(original.fxData);      // TryClone gère UnityObject / ScriptableObject
        clone.fxPrefab = original.fxPrefab;            // on garde le prefab de base (instancier à l'usage)
        clone.fxYOffset = original.fxYOffset;

        clone.icon = original.icon;

        Debug.Log($"[SkillTreeManager] Cloned skill '{original.skillName}' -> instanceID {clone.GetInstanceID()}, ID {clone.ID}");
        return clone;
    }

    private void ApplyOverrides(Data_Skill skill, SkillNode.SkillUpgrade upgrade)
    {
        if (upgrade == null || skill == null) return;

        if (upgrade.overrideName) skill.skillName = upgrade.skillName;
        if (upgrade.overrideDescription) skill.description = upgrade.description;
        if (upgrade.overrideSkillType) skill.skillType = upgrade.skillType;
        if (upgrade.overrideSkillElement) skill.skillElement = upgrade.skillElement;
        if (upgrade.overrideDamageMin) skill.damageMin = upgrade.damageMin;
        if (upgrade.overrideDamageMax) skill.damageMax = upgrade.damageMax;
        if (upgrade.overrideCostPA) skill.costPA = upgrade.costPA;
        if (upgrade.overrideRangeMin) skill.rangeMin = upgrade.rangeMin;
        if (upgrade.overrideRangeMax) skill.rangeMax = upgrade.rangeMax;
        if (upgrade.overrideCooldown) skill.cooldown = upgrade.cooldown;
        if (upgrade.overrideMaxPerTargetPerTurn) skill.maxPerTargetPerTurn = upgrade.maxPerTargetPerTurn;

        if (upgrade.overrideImpactZone && upgrade.impactZone != null)
        {
            skill.impactZone = TryClone(upgrade.impactZone);

            object origIzObj = upgrade.impactZone;
            UnityObject origIzUo = origIzObj as UnityObject;
            object newIzObj = skill.impactZone;
            UnityObject newIzUo = newIzObj as UnityObject;
            if (origIzUo != null && newIzUo != null)
            {
                try { newIzUo.name = origIzUo.name + "_override_runtime"; } catch { }
            }
        }

        if (upgrade.overrideEffects)
        {
            skill.effects = new List<SkillEffect>(upgrade.effects.Count);
            foreach (var e in upgrade.effects)
            {
                var cloneE = TryClone(e);
                skill.effects.Add(cloneE);
            }
        }

        if (upgrade.overrideCritChance) skill.critChance = upgrade.critChance;

        if (upgrade.overrideCritEffects)
        {
            skill.critEffects = new List<SkillEffect>(upgrade.critEffects.Count);
            foreach (var ce in upgrade.critEffects)
            {
                var cloneCe = TryClone(ce);
                skill.critEffects.Add(cloneCe);
            }
        }

        if (upgrade.overrideIcon) skill.icon = upgrade.icon;
    }

    // ----------- IMPORTANT : on tient compte de targetSkill si présent -------------
    private Data_Skill GetRuntimeSkill(SkillNode node)
    {
        if (node == null) return null;

        // si node.targetSkill est renseigné, on clone/cible cette skill (c'est un "upgrade node")
        Data_Skill baseSkill = node.targetSkill != null ? node.targetSkill : node.skillData;

        if (baseSkill == null) return null;

        Debug.Log($"[SkillTreeManager] GetRuntimeSkill : node '{node.SkillName}' baseSkill = '{baseSkill.skillName}' (ID={baseSkill.ID}). Overrides actifs ? {node.upgrade?.HasAnyOverride()}");

        // Clone la skill de base (targetSkill ou skillData)
        Data_Skill runtimeSkill = CloneSkill(baseSkill);

        // Applique tous les overrides disponibles (sur la clone)
        ApplyOverrides(runtimeSkill, node.upgrade);

        return runtimeSkill;
    }

    // ---------------- SYNCHRONISATION AVEC LE SKILLBOOK ----------------
    private void SyncAllUnlockedSkills()
    {
        var combatStats = GetCombatStats();
        if (combatStats == null) return;

        foreach (var branch in branches)
        {
            if (branch == null || branch.nodes == null) continue;

            foreach (var node in branch.nodes)
            {
                if (node == null) continue;

                // Déterminer la base (targetSkill si présent sinon skillData)
                Data_Skill baseSkill = node.targetSkill != null ? node.targetSkill : node.skillData;
                if (baseSkill == null) continue;
                if (!node.IsUnlocked) continue;

                Data_Skill runtimeSkill = GetRuntimeSkill(node);
                if (runtimeSkill == null) continue;

                // Recherche par ID en utilisant la baseSkill.ID (targetSkill.ID si upgrade node)
                int existingIndex = combatStats.skillBook.FindIndex(b => b.skill != null && b.skill.ID == baseSkill.ID);

                if (existingIndex >= 0)
                {
                    combatStats.skillBook[existingIndex].skill = runtimeSkill;
                    Debug.Log($"[SkillTreeManager] Skill ID {baseSkill.ID} mise à jour dans le SkillBook par '{runtimeSkill.skillName}'");
                }
                else
                {
                    combatStats.skillBook.Add(new Skill_Binding
                    {
                        skill = runtimeSkill,
                        fxData = null,
                        fxPrefabOverride = runtimeSkill.fxPrefab != null
    ? runtimeSkill.fxPrefab.GetComponent<Sprite_AnimationRunner>()
    : null,
                        fxYOffset = runtimeSkill.fxYOffset
                    });
                    Debug.Log($"[SkillTreeManager] Skill '{runtimeSkill.skillName}' ajoutée au SkillBook de {combatStats.name} (base ID {baseSkill.ID})");
                }
            }
        }
    }

    // ---------------- DÉBLOCAGE DES SKILLS ----------------
    public void UnlockSkill(SkillNode node)
    {
        if (node == null || node.IsUnlocked || !ArePrerequisitesMet(node)) return;
        if (entityInfo != null && entityInfo.entity_Level < node.requiredLevel)
        {
            Debug.Log($"Niveau insuffisant pour {node.SkillName}");
            return;
        }

        if (availablePoints >= node.cost)
        {
            availablePoints -= node.cost;
            skillPointsUI?.UpdatePointsDisplay();

            node.isUnlockedRuntime = true;
            node.onUnlock?.Invoke();

            var combatStats = GetCombatStats();
            if (combatStats != null)
            {
                // Déterminer la base (targetSkill si présent sinon skillData)
                Data_Skill baseSkill = node.targetSkill != null ? node.targetSkill : node.skillData;

                Data_Skill runtimeSkill = GetRuntimeSkill(node);
                if (runtimeSkill != null && baseSkill != null)
                {
                    if (NetworkServer.active == false && NetworkClient.active)
                    {
                        Debug.LogWarning("[SkillTreeManager] Client-side unlock : si ton skillBook est serveur-authoritative, " +
                                         "le serveur doit appliquer la modification (implémente un Command/ServerRpc).");
                    }

                    int existingIndex = combatStats.skillBook.FindIndex(b => b.skill != null && b.skill.ID == baseSkill.ID);
                    if (existingIndex >= 0)
                    {
                        combatStats.skillBook[existingIndex].skill = runtimeSkill;
                        Debug.Log($"[SkillTreeManager] Skill ID {baseSkill.ID} mise à jour (unlock) dans SkillBook par '{runtimeSkill.skillName}'");
                    }
                    else
                    {
                        combatStats.skillBook.Add(new Skill_Binding
                        {
                            skill = runtimeSkill,
                            fxData = null,
                            fxPrefabOverride = baseSkill.fxPrefab != null
    ? baseSkill.fxPrefab.GetComponent<Sprite_AnimationRunner>()
    : null,
                            fxYOffset = baseSkill.fxYOffset,
                            //fxYOffset = 0f
                        });
                        Debug.Log($"[SkillTreeManager] Skill '{runtimeSkill.skillName}' ajoutée (unlock) au SkillBook de {combatStats.name} (base ID {baseSkill.ID})");
                    }
                }
            }

            UpdateAllSkillButtons();
        }
    }

    // ---------------- LOGIQUE DE PRÉREQUIS ----------------
    bool ArePrerequisitesMet(SkillNode node)
    {
        SkillTreeBranch branch = branches.Find(b => b.nodes.Contains(node));
        if (branch == null) return false;

        int index = branch.nodes.IndexOf(node);
        if (index <= 0) return true;

        SkillNode previous = branch.nodes[index - 1];
        return previous != null && (previous.IsUnlocked || previous.isUnlockedRuntime);
    }

    public bool CanUnlock(SkillNode node)
    {
        if (node == null || node.IsUnlocked || !ArePrerequisitesMet(node)) return false;
        if (availablePoints < node.cost) return false;
        if (entityInfo != null && entityInfo.entity_Level < node.requiredLevel) return false;
        return true;
    }

    public void UpdateAllSkillButtons()
    {
        foreach (SkillButtonUI btn in allSkillButtons)
            btn?.UpdateVisual();
    }

    public void EnsureSyncedToCombatBook()
    {
        SyncAllUnlockedSkills();
    }
}
