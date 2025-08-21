using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[AddComponentMenu("Combat/UI/Timeline Profil UI")]
public class Timeline_ProfilUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Réfs UI (Drag & Drop depuis le prefab)")]
    [SerializeField] private Image portrait;       // Icon_Entity
    [SerializeField] private Image currentHpBar;   // CurrentHP_Bar (Image)

    private GameObject entity;
    private Entity_Info info;
    private Entity_StatistiqueCombat stats;

    // Réf fournie par Timeline_CombatUI
    private InfoEntityPanelUI infoPanel;

    /// <summary>Appelé par Timeline_CombatUI juste après l'Instantiate.</summary>
    public void Bind(GameObject boundEntity, InfoEntityPanelUI panel)
    {
        entity = boundEntity;
        infoPanel = panel;

        info = entity ? entity.GetComponent<Entity_Info>() : null;
        stats = entity ? entity.GetComponent<Entity_StatistiqueCombat>() : null;

        if (portrait) portrait.sprite = info ? info.entity_Icon : null;

        // IMPORTANT : s'assurer que l'Image est bien en Filled au runtime
        if (currentHpBar)
        {
            currentHpBar.type = Image.Type.Filled;
            currentHpBar.fillMethod = Image.FillMethod.Vertical;
            currentHpBar.fillOrigin = (int)Image.OriginVertical.Bottom; // (ou Top si tu veux que ça descende)
        }

        RefreshHP();
    }

    public void RefreshHP()
    {
        if (!stats || !currentHpBar) return;
        float ratio = (stats.baseHP > 0) ? Mathf.Clamp01(stats.currentHP / (float)stats.baseHP) : 0f;
        currentHpBar.fillAmount = ratio;

        // DEBUG (optionnel) : décommente si tu veux vérifier la valeur
        // Debug.Log($"[Timeline] {entity?.name} HP {stats.currentHP}/{stats.baseHP} => fill {ratio:0.00}");
    }

    // Survol = afficher/cacher le panneau d’info
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (infoPanel && entity) infoPanel.ShowFor(entity);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (infoPanel) infoPanel.Hide();
    }
    private void OnDisable()
    {
        if (infoPanel) infoPanel.Hide();
    }
}
