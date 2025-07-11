using UnityEngine;

[RequireComponent(typeof(CombatStats))]
[RequireComponent(typeof(CombatController))]
public class EntityInfo : MonoBehaviour
{
    public TeamType team;
    public int id { get; private set; }

    private CombatStats stats;
    private CombatController controller;

    private void Start()
    {
        stats = GetComponent<CombatStats>();
        controller = GetComponent<CombatController>();

        if (AnalyseDataFighting.Instance == null)
        {
            Debug.LogError("Aucun AnalyseDataFighting dans la scène !");
            return;
        }

        Vector2Int startPos = controller.GetCurrentCoord();
        id = AnalyseDataFighting.Instance.RegisterEntity(
            gameObject,
            team,
            controller.GetCurrentCoord(),
            stats.currentHP,
            stats
         );

    }

    private void Update()
    {
        // Met à jour la position et les PV à chaque frame
        if (AnalyseDataFighting.Instance != null)
        {
            Vector2Int newPos = controller.GetCurrentCoord();
            AnalyseDataFighting.Instance.UpdateEntity(id, newPos, stats.currentHP);
        }
    }
}
