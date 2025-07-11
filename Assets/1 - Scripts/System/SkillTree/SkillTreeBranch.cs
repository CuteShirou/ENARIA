using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillTreeBranch", menuName = "Game Creation Tool/SkillTreeBranch")]
public class SkillTreeBranch : ScriptableObject
{
    public string branchName;
    public List<SkillNode> nodes;
}
