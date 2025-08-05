using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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