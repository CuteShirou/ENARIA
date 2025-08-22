using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[AddComponentMenu("Combat/UI/Timeline Profil UI")]
public class Timeline_ProfilUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Réfs UI (Drag & Drop depuis le prefab)")]
    [SerializeField] private Image portrait;
    [SerializeField] private Image currentHpBar;

    private GameObject entity;
    private Entity_Info info;
    private Entity_StatistiqueCombat stats;

    private InfoEntityPanelUI infoPanel;

    public void Bind(GameObject boundEntity, InfoEntityPanelUI panel)
    {
        entity = boundEntity;
        infoPanel = panel;

        info = entity ? entity.GetComponent<Entity_Info>() : null;
        stats = entity ? entity.GetComponent<Entity_StatistiqueCombat>() : null;

        if (portrait) portrait.sprite = info ? info.entity_Icon : null;

        RefreshHP();
    }

    public void RefreshHP()
    {
        if (!stats || !currentHpBar) return;
        float ratio = (stats.baseHP > 0) ? Mathf.Clamp01(stats.currentHP / (float)stats.baseHP) : 0f;
        currentHpBar.fillAmount = ratio;
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
