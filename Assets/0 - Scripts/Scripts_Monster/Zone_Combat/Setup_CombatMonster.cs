using UnityEngine;

public class Setup_CombatMonster : MonoBehaviour
{
    public enum Team { TeamRed, TeamGreen, Neutral }

    [Header("Réglages")]
    [SerializeField] Team team = Team.TeamRed;
    [SerializeField] Transform teamRootOverride; // optionnel : si tu veux forcer un parent précis
    [SerializeField] bool keepWorldPosition = true;

    void Awake()
    {
        Transform targetParent = teamRootOverride ?? FindTeamRoot(team);
        if (targetParent != null)
        {
            transform.SetParent(targetParent, keepWorldPosition);
        }
        else
        {
            Debug.LogWarning($"[Setup_CombatMonster] Aucun parent trouvé pour l'équipe {team}.", this);
        }
    }

    Transform FindTeamRoot(Team t)
    {
        // Ex: root trouvés via tags posés dans la scène
        switch (t)
        {
            case Team.TeamRed: return FindByTagOrNull("TeamRed");
            case Team.TeamGreen: return FindByTagOrNull("TeamGreen");
            default: return null;
        }
    }

    Transform FindByTagOrNull(string tag)
    {
        var go = GameObject.FindGameObjectWithTag(tag);
        return go ? go.transform : null;
    }
}
