//using UnityEngine;

//public class StatsUIManager : MonoBehaviour
//{
//    public CombatStats targetStats;

//    [Header("Références UI")]
//    public StatRowUI rowPV;
//    public StatRowUI rowPA;
//    public StatRowUI rowPM;
//    public StatRowUI rowPO;
//    public StatRowUI rowFOR;
//    public StatRowUI rowDEX;
//    public StatRowUI rowMAG;
//    public StatRowUI rowFOI;

//    void Start()
//    {
//        if (targetStats == null)
//        {
//            Debug.LogError("Aucune référence à CombatStats dans StatsUIManager !");
//            return;
//        }

//        rowPV.Initialize("PV", targetStats);
//        rowPA.Initialize("PA", targetStats);
//        rowPM.Initialize("PM", targetStats);
//        rowPO.Initialize("PO", targetStats);
//        rowFOR.Initialize("FOR", targetStats);
//        rowDEX.Initialize("DEX", targetStats);
//        rowMAG.Initialize("MAG", targetStats);
//        rowFOI.Initialize("FOI", targetStats);
//    }
//}
