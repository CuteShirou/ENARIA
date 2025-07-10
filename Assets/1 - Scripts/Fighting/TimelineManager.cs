using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimelineManager : MonoBehaviour
{
    public GameObject timelineSlotPrefab;       // Le prefab à instancier pour chaque entité
    public Transform timelineParent;            // Référence au TimelinePanel
    public Sprite defaultPortrait;              // Icône par défaut pour une entité
    public InfoBubbleUI infoBubbleUI;           // Référence à la bulle d'information (InfoBubble)

    private List<GameObject> currentSlots = new();
    private bool testDone = false;

    private void Update()
    {
        // On attend que le CombatManager soit prêt avant d'initialiser la timeline
        if (!testDone)
        {
            CombatManager cm = FindAnyObjectByType<CombatManager>();
            if (cm != null && cm.fighters.Count > 0)
            {
                CreateTimeline();
                testDone = true;
            }
        }
    }

    public void CreateTimeline()
    {
        // Supprimer les anciens slots s'ils existent
        foreach (GameObject slot in currentSlots)
        {
            Destroy(slot);
        }
        currentSlots.Clear();

        // Récupérer les entités en combat
        CombatManager cm = FindAnyObjectByType<CombatManager>();
        if (cm == null)
        {
            Debug.LogError("Aucun CombatManager trouvé dans la scène !");
            return;
        }

        foreach (GameObject entity in cm.fighters)
        {
            GameObject newSlot = Instantiate(timelineSlotPrefab, timelineParent);
            currentSlots.Add(newSlot);

            // On récupère les composants internes
            Transform portrait = newSlot.transform.Find("Portrait");
            Transform bar = newSlot.transform.Find("HealthBarFill");
            CombatStats stats = entity.GetComponent<CombatStats>();

            // Affecter l'icône
            if (portrait.TryGetComponent<Image>(out Image portraitImg))
            {
                portraitImg.sprite = defaultPortrait;
            }

            //// Remplir la barre de vie en fonction du ratio HP
            //if (bar.TryGetComponent<Image>(out Image barImg))
            //{
            //    float ratio = (float)stats.currentHP / stats.baseHP;
            //    barImg.fillAmount = ratio;
            //    barImg.type = Image.Type.Filled;
            //    barImg.fillMethod = Image.FillMethod.Vertical;
            //    barImg.fillOrigin = (int)Image.OriginVertical.Bottom;
            //}

            // Ajouter le comportement de survol
            TimelineSlotHover hover = newSlot.AddComponent<TimelineSlotHover>();
            hover.targetEntity = entity;
            hover.infoBubble = infoBubbleUI;
            hover.portraitSprite = defaultPortrait;
        }
    }
}
