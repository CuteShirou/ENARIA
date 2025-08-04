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

    private List<SkillButtonUI> allSkillButtons = new List<SkillButtonUI>();

    public void UnlockSkill(SkillNode node)
    {
        if (node.isUnlocked || !ArePrerequisitesMet(node))
            return;

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
        if (branch == null) return false;

        int index = branch.nodes.IndexOf(node);

        if (index == 0)
            node.isUnlocked = true;
        return true;

        SkillNode previous = branch.nodes[index - 1];
        return previous.isUnlocked;
    }

    public bool CanUnlock(SkillNode node)
    {
        return !node.isUnlocked && ArePrerequisitesMet(node) && availablePoints >= node.cost;
    }

    public void UpdateAllSkillButtons()
    {
        foreach (SkillButtonUI btn in allSkillButtons)
        {
            btn.UpdateVisual();
        }
    }

    void Start()
    {
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

//    public override void OnStartServer()
//    {
//        base.OnStartServer();
//        // initialize unlocked flags from branches order
//        foreach (var branch in branches)
//            foreach (var node in branch.nodes)
//                unlockedFlags.Add(node.isUnlocked);
//    }

//    public override void OnStartClient()
//    {
//        base.OnStartClient();
//        BuildUI();
//        unlockedFlags.Callback += OnUnlockedFlagChanged;
//        UpdateAllSkillButtons();
//    }

//    private void OnPointsChanged(int oldPoints, int newPoints)
//    {
//        UpdateAllSkillButtons();
//    }

//    private void OnUnlockedFlagChanged(SyncList<bool>.Operation op, int index, bool oldValue, bool newValue)
//    {
//        allSkillButtons[index].SetUnlocked(newValue);
//        UpdateAllSkillButtons();
//    }

//    private void BuildUI()
//    {
//        // clear previous
//        foreach (Transform child in skillListParent) Destroy(child.gameObject);
//        allSkillButtons.Clear();

//        // container
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

//            // label
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
//                btnUI.Initialize(node, this, idx, unlockedFlags.Count > idx && unlockedFlags[idx]);
//                allSkillButtons.Add(btnUI);
//                idx++;
//            }
//        }
//    }

//    [Command]
//    public void CmdTryUnlock(int index)
//    {
//        // Commands are only executed on server; no need for authority check here
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

//    public void OnSkillButtonClicked(int index)
//    {
//        // Client invokes command
//        CmdTryUnlock(index);
//    }

//    private void UpdateAllSkillButtons()
//    {
//        for (int i = 0; i < allSkillButtons.Count; i++)
//            allSkillButtons[i].UpdateVisual(availablePoints, unlockedFlags[i]);
//    }
//}