using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class SkillTreeManager : MonoBehaviour
{
    public int availablePoints = 5;
    public List<SkillTreeBranch> branches;
    public GameObject skillButtonPrefab;
    public Transform skillListParent;

    [Tooltip("Reference to the player's stats (used to check level). Assign in inspector or it will auto-find on Start).")]
    public PlayerStats playerStats;

    private List<SkillButtonUI> allSkillButtons = new List<SkillButtonUI>();

    void Awake()
    {
        if (playerStats == null)
            playerStats = FindObjectOfType<PlayerStats>();
    }

    public void UnlockSkill(SkillNode node)
    {
        if (node == null) return;
        if (node.isUnlocked) return;
        if (!ArePrerequisitesMet(node)) return;

        if (playerStats != null && playerStats.level < node.requiredLevel)
        {
            Debug.Log($"Niveau insuffisant (requis {node.requiredLevel}, actuel {playerStats.level}) pour {node.SkillName}");
            return;
        }

        if (availablePoints >= node.cost)
        {
            availablePoints -= node.cost;
            node.isUnlocked = true;
            node.onUnlock?.Invoke();
            UpdateAllSkillButtons();
        }
    }

    bool ArePrerequisitesMet(SkillNode node)
    {
        SkillTreeBranch branch = branches.Find(b => b.nodes.Contains(node));
        if (branch == null)
            return false;

        int index = branch.nodes.IndexOf(node);
        if (index < 0) return false;

        // Si c'est la première node de la branche, pas de prérequis de node
        if (index == 0)
            return true;

        // Sinon vérifier la node précédente
        SkillNode previous = branch.nodes[index - 1];
        return previous != null && previous.isUnlocked;
    }

    public bool CanUnlock(SkillNode node)
    {
        if (node == null) return false;
        if (node.isUnlocked) return false;
        if (!ArePrerequisitesMet(node)) return false;
        if (availablePoints < node.cost) return false;
        if (playerStats != null && playerStats.level < node.requiredLevel) return false;
        return true;
    }

    public void UpdateAllSkillButtons()
    {
        foreach (SkillButtonUI btn in allSkillButtons)
        {
            if (btn != null)
                btn.UpdateVisual();
        }
    }

    void Start()
    {
        // ton code existant pour générer l'UI — inchangé
        foreach (Transform child in skillListParent)
            Destroy(child.gameObject);

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
            }
        }
    }
}











//using System.Collections.Generic;
//using Mirror;
//using UnityEngine;
//using UnityEngine.UI;

//public class SkillTreeManager : NetworkBehaviour
//{
//    [SyncVar(hook = nameof(OnPointsChanged))]
//    [SerializeField] private int availablePoints = 5;
//    public int AvailablePoints => availablePoints;

//    [SerializeField] private List<SkillTreeBranch> branches;
//    [SerializeField] private GameObject skillButtonPrefab;
//    [SerializeField] private Transform skillListParent;

//    // SyncList for unlocked flags
//    public readonly SyncList<bool> unlockedFlags = new SyncList<bool>();
//    private readonly List<SkillButtonUI> allSkillButtons = new List<SkillButtonUI>();

//    // Référence à ton API Unity
//    private SkillApi skillApi;
//    private string playerId = "1"; // à configurer dynamiquement si besoin

//    public override void OnStartServer()
//    {
//        base.OnStartServer();
//        foreach (var branch in branches)
//            foreach (var node in branch.nodes)
//                unlockedFlags.Add(node.isUnlocked);
//    }

//    public override void OnStartClient()
//    {
//        base.OnStartClient();

//        skillApi = FindFirstObjectByType<SkillApi>();

//        BuildUI();
//        unlockedFlags.Callback += OnUnlockedFlagChanged;
//        UpdateAllSkillButtons();

//        // 1) Fetch initial unlocked skills depuis le serveur
//        skillApi.FetchUnlockedSkills(playerId,
//            skills => {
//                // pour chaque skill côté client, on met à jour unlockedFlags
//                HashSet<int> ids = new HashSet<int>();
//                foreach (var s in skills) ids.Add(s.id);
//                for (int i = 0; i < allSkillButtons.Count; i++)
//                    CmdSetFlag(i, ids.Contains(branches[GetBranchIndex(i)].nodes[GetNodeIndexInBranch(i)].skillData.ID));
//            },
//            error => Debug.LogError("Fetch skills failed: " + error)
//        );
//    }

//    // Hook SyncVar
//    private void OnPointsChanged(int oldPoints, int newPoints) => UpdateAllSkillButtons();

//    // Hook SyncList
//    private void OnUnlockedFlagChanged(SyncList<bool>.Operation op, int idx, bool oldVal, bool newVal)
//    {
//        allSkillButtons[idx].SetUnlocked(newVal);
//        UpdateAllSkillButtons();
//    }

//    private void BuildUI()
//    {
//        foreach (Transform child in skillListParent) Destroy(child.gameObject);
//        allSkillButtons.Clear();

//        var row = new GameObject("BranchesRow");
//        row.transform.SetParent(skillListParent, false);
//        var hLayout = row.AddComponent<HorizontalLayoutGroup>();
//        hLayout.childForceExpandWidth = false;
//        hLayout.childForceExpandHeight = true;
//        hLayout.spacing = 150;
//        var fitter = row.AddComponent<ContentSizeFitter>();
//        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
//        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

//        int idx = 0;
//        foreach (var branch in branches)
//        {
//            var panel = new GameObject(branch.branchName);
//            panel.transform.SetParent(row.transform, false);
//            var rect = panel.AddComponent<RectTransform>();
//            rect.sizeDelta = new Vector2(300, 0);
//            var vLayout = panel.AddComponent<VerticalLayoutGroup>();
//            vLayout.childAlignment = TextAnchor.UpperCenter;
//            vLayout.childForceExpandWidth = true;
//            vLayout.childForceExpandHeight = false;
//            vLayout.spacing = 50;
//            var fitterBranch = panel.AddComponent<ContentSizeFitter>();
//            fitterBranch.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

//            var labelGO = new GameObject("Label");
//            labelGO.transform.SetParent(panel.transform, false);
//            var label = labelGO.AddComponent<Text>();
//            label.text = branch.branchName.ToUpper();
//            label.alignment = TextAnchor.MiddleCenter;
//            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
//            label.fontSize = 24;

//            foreach (var node in branch.nodes)
//            {
//                var go = Instantiate(skillButtonPrefab, panel.transform);
//                var btnUI = go.GetComponent<SkillButtonUI>();
//                bool isUnlocked = unlockedFlags.Count > idx && unlockedFlags[idx];
//                btnUI.Initialize(node, this, idx, isUnlocked);
//                allSkillButtons.Add(btnUI);
//                idx++;
//            }
//        }
//    }

//    // Client -> Server pour synchroniser drapeaux initiaux
//    [Command]
//    private void CmdSetFlag(int index, bool isUnlocked)
//    {
//        if (index >= 0 && index < unlockedFlags.Count)
//            unlockedFlags[index] = isUnlocked;
//    }

//    /// <summary>
//    /// Appelée par le SkillButtonUI lors du clic
//    /// </summary>
//    public void RequestUnlock(int index)
//    {
//        // 1) on poste au serveur Unity (Mirror) pour diminution de points et maj SyncList
//        CmdTryUnlock(index);

//        // 2) on poste aussi à l’API HTTP pour persistance en BDD
//        int skillId = branches[GetBranchIndex(index)].nodes[GetNodeIndexInBranch(index)].skillData.ID;
//        skillApi.UnlockSkill(playerId, skillId.ToString(), success => {
//            if (!success) Debug.LogError("API Unlock failed");
//        });
//    }

//    [Command]
//    public void CmdTryUnlock(int index)
//    {
//        if (index < 0 || index >= unlockedFlags.Count) return;
//        int cost = GetNodeCost(index);
//        if (!unlockedFlags[index] && availablePoints >= cost)
//        {
//            availablePoints -= cost;
//            unlockedFlags[index] = true;
//        }
//    }

//    private int GetNodeCost(int index)
//    {
//        int offset = 0;
//        foreach (var branch in branches)
//        {
//            if (index - offset < branch.nodes.Count)
//                return branch.nodes[index - offset].cost;
//            offset += branch.nodes.Count;
//        }
//        return int.MaxValue;
//    }

//    private void UpdateAllSkillButtons()
//    {
//        for (int i = 0; i < allSkillButtons.Count; i++)
//            allSkillButtons[i].UpdateVisual(availablePoints, unlockedFlags[i]);
//    }

//    // Helpers pour retrouver branche + index interne
//    private int GetBranchIndex(int flatIndex)
//    {
//        int sum = 0;
//        for (int b = 0; b < branches.Count; b++)
//        {
//            if (flatIndex < sum + branches[b].nodes.Count) return b;
//            sum += branches[b].nodes.Count;
//        }
//        return 0;
//    }
//    private int GetNodeIndexInBranch(int flatIndex)
//    {
//        int sum = 0;
//        foreach (var branch in branches)
//        {
//            if (flatIndex < sum + branch.nodes.Count)
//                return flatIndex - sum;
//            sum += branch.nodes.Count;
//        }
//        return 0;
//    }
//}