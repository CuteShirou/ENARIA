using UnityEngine;

[AddComponentMenu("Combat/Setup Combat Player (Drag & Drop)")]
public class Setup_CombatPlayer : MonoBehaviour
{
    public enum Team { Red, Green, Neutral }

    [Header("Équipe & Parentage")]
    [Tooltip("Équipe du joueur. Sert à déterminer le parent si aucun override n'est spécifié.")]
    [SerializeField] private Team team = Team.Green;

    [Tooltip("Si défini, ce Transform sera utilisé comme parent directement (sans chercher par tag).")]
    [SerializeField] private Transform teamRootOverride;

    [Tooltip("Conserver la TRS monde lors du reparenting (position/rotation/échelle).")]
    [SerializeField] private bool keepWorldPosition = true;

    [Header("Fallback par Tag (si pas d'override)")]
    [SerializeField] private string redTeamRootTag = "TeamRedRoot";
    [SerializeField] private string greenTeamRootTag = "TeamGreenRoot";

    [Header("Position initiale (optionnelle)")]
    [Tooltip("Si coché, on positionne le joueur au démarrage.")]
    [SerializeField] private bool applyStartPosition = false;

    [Tooltip("Si défini, on utilise ce Transform comme point de spawn (prioritaire).")]
    [SerializeField] private Transform startPointOverride;

    [Tooltip("Si aucun startPointOverride, on utilisera cette position sérialisée.")]
    [SerializeField] private Vector3 serializedStartPosition;

    private void Awake()
    {
        ApplyParenting();
        ApplyStartPositionIfNeeded();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(redTeamRootTag)) redTeamRootTag = "TeamRedRoot";
        if (string.IsNullOrWhiteSpace(greenTeamRootTag)) greenTeamRootTag = "TeamGreenRoot";
    }
#endif

    private void ApplyParenting()
    {
        Transform targetParent = teamRootOverride;
        if (targetParent == null)
            targetParent = FindTeamRootByTag(team);

        if (targetParent != null && transform.parent != targetParent)
        {
            transform.SetParent(targetParent, keepWorldPosition);
        }
        else if (targetParent == null)
        {
            Debug.LogWarning(
                $"[Setup_CombatPlayer] Aucun parent trouvé. " +
                $"Définis 'teamRootOverride' ou place un objet taggé '{GetExpectedTag(team)}' dans la scène.",
                this
            );
        }
    }

    private void ApplyStartPositionIfNeeded()
    {
        if (!applyStartPosition) return;

        if (startPointOverride != null)
        {
            transform.position = startPointOverride.position;
            transform.rotation = startPointOverride.rotation;
            return;
        }

        // Fallback sur la position sérialisée
        transform.position = serializedStartPosition;
        // on ne force pas la rotation si pas de point (garde la rotation actuelle)
    }

    private Transform FindTeamRootByTag(Team t)
    {
        string tagToUse = GetExpectedTag(t);
        if (string.IsNullOrEmpty(tagToUse)) return null;

        var go = GameObject.FindGameObjectWithTag(tagToUse);
        return go ? go.transform : null;
    }

    private string GetExpectedTag(Team t)
    {
        switch (t)
        {
            case Team.Red: return redTeamRootTag;
            case Team.Green: return greenTeamRootTag;
            default: return null;
        }
    }
}
