using UnityEngine;

public class ButtonSkillSelector : MonoBehaviour
{
    [Header("Compétence à équiper")]
    public SkillData skillToEquip;

    [Header("Références automatiques")]
    public SkillCaster caster; // Peut rester vide dans l'inspecteur, sera trouvé automatiquement

    void Start()
    {
        // Si la référence n'est pas assignée dans l’inspecteur, on essaie de la trouver automatiquement
        if (caster == null)
        {
            // Option 1 : rechercher un GameObject tagué "Player" (recommandé si bien configuré)
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                caster = player.GetComponent<SkillCaster>();

            // Option 2 : fallback global dans la scène (moins optimal)
            if (caster == null)
                caster = Object.FindFirstObjectByType<SkillCaster>();
        }
    }

    public void EquipSkillFromButton()
    {
        if (caster != null && skillToEquip != null)
        {
            caster.SelectSkill(skillToEquip);
            Debug.Log($"Compétence {skillToEquip.skillName} équipée !");
        }
        else
        {
            Debug.LogWarning("Impossible d’équiper la compétence : référence manquante !");
        }
    }
}
