using System.Collections.Generic;
using UnityEngine;
using Mirror; // Pour la synchronisation réseau

//--------------------------------------------------
public class Phase_EnterSetupCombat : MonoBehaviour
{
    [Header("Carte utilisée pour ce combat")]
    public Data_FightMap combatMap;

    [Header("Équipes de combat")]
    public List<GameObject> greenTeam = new();   // Joueurs (équipe verte)
    public List<GameObject> redTeam = new();     // Monstres (équipe rouge)
    public List<GameObject> AllFighters = new(); // Ordre de tour (vert + rouge)

    [Header("Références hiérarchie")]
    public Transform teamRedParent;    // Objet parent pour l'équipe Rouge dans la hiérarchie
    public Transform teamGreenParent;  // Objet parent pour l'équipe Verte dans la hiérarchie

    // Groupe de monstres déclencheur du combat
    private Exploration_InfoGroupMonster currentGroup;

    // Référence vers le Combat_PhaseManager principal
    private Combat_PhaseManager manager;

    // ➕ Liste temporaire des joueurs arrivés avant que InitPhase() soit appelé
    private List<GameObject> pendingPlayers = new();

    //--------------------------------------------------
    public void InitPhase(Combat_PhaseManager phaseManager)
    {
        manager = phaseManager;

        Debug.Log($"[Phase_EnterSetupCombat][Arena {manager.arenaIndex}] Phase d'entrée activée.");
        Debug.Log($"[Phase_EnterSetupCombat] Joueurs en attente : {greenTeam.Count}");
        Debug.Log($"[Phase_EnterSetupCombat] Monstres dans le groupe : {redTeam.Count}");
        Debug.Log($"[Phase_EnterSetupCombat] Carte utilisée : {(combatMap != null ? combatMap.name : "aucune")}");

        // Spawn des monstres côté serveur
        SpawnMonstersInScene();

        // Ajoute les joueurs qui étaient en attente
        foreach (GameObject player in pendingPlayers)
        {
            AddPlayerToTeamVerte(player);
        }
        pendingPlayers.Clear();

        Debug.Log("[Phase_EnterSetupCombat] Phase terminée. Passage à la phase de préparation.");

        manager.NextPhase();
    }

    //--------------------------------------------------
    private void SpawnMonstersInScene()
    {
        if (!NetworkServer.active)
        {
            Debug.LogWarning("[Phase_EnterSetupCombat] Tentative de spawn de monstres alors que ce n'est pas le serveur.");
            return;
        }

        if (teamRedParent == null)
        {
            Debug.LogError("[Phase_EnterSetupCombat] Aucun parent 'TeamRed' défini pour accueillir les monstres.");
            return;
        }

        List<GameObject> monstersSpawned = new();

        foreach (GameObject prefabMonster in redTeam)
        {
            Vector3 spawnPosition = teamRedParent.position;
            Quaternion spawnRotation = Quaternion.identity;

            GameObject monster = Instantiate(prefabMonster, spawnPosition, spawnRotation);
            monster.transform.SetParent(teamRedParent); // Organisation hiérarchique serveur

            if (monster.TryGetComponent(out Setup_NetworkMonster setup))
            {
                NetworkIdentity redNet = teamRedParent.GetComponent<NetworkIdentity>();
                if (redNet != null)
                    setup.parentNetId = redNet.netId;
            }

            NetworkServer.Spawn(monster);
            monstersSpawned.Add(monster);
        }

        redTeam = monstersSpawned;

        AllFighters.Clear();
        AllFighters.AddRange(redTeam);
        AllFighters.AddRange(greenTeam);

        Debug.Log($"[Phase_EnterSetupCombat] {redTeam.Count} monstres instanciés et synchronisés.");
    }

    //--------------------------------------------------
    public void SetCombatData(List<GameObject> newMonsters, Data_FightMap newMap, Exploration_InfoGroupMonster group)
    {
        redTeam = newMonsters;
        combatMap = newMap;
        currentGroup = group;

        AllFighters.AddRange(redTeam);

        Debug.Log("[Phase_EnterSetupCombat] Données de combat reçues.");
        Debug.Log($" - Monstres : {redTeam.Count}");
        Debug.Log($" - Carte : {(combatMap != null ? combatMap.name : "null")}");
    }

    //--------------------------------------------------
    public void AddPlayerToTeamVerte(GameObject player)
    {
        if (manager == null)
        {
            Debug.LogWarning("[Phase_EnterSetupCombat] Manager encore null → joueur ajouté en file d'attente.");
            if (!pendingPlayers.Contains(player))
                pendingPlayers.Add(player);
            return;
        }

        if (!greenTeam.Contains(player))
        {
            greenTeam.Add(player);
            AllFighters.Add(player);
            Debug.Log("[Phase_EnterSetupCombat] Joueur ajouté à l'équipe verte : " + player.name);

            if (teamGreenParent == null)
            {
                Debug.LogError("[Phase_EnterSetupCombat] Aucun parent 'TeamVerte' défini !");
                return;
            }

            player.transform.SetParent(teamGreenParent); // Organisation hiérarchique serveur

            if (player.TryGetComponent(out Player_SetupNetworkCombat setup))
            {
                NetworkIdentity greenNet = teamGreenParent.GetComponent<NetworkIdentity>();
                if (greenNet != null)
                {
                    setup.parentNetId = greenNet.netId;
                    Debug.Log($"[DEBUG] ParentNetId assigné → {greenNet.netId}");
                }

                NetworkIdentity managerNetId = manager.GetComponent<NetworkIdentity>();
                if (managerNetId != null)
                {
                    setup.combatManagerIdentity = managerNetId;
                    Debug.Log($"[DEBUG] CombatManager assigné (NetID = {managerNetId.netId}) à {player.name}");
                }
                else
                {
                    Debug.LogError("[Phase_EnterSetupCombat] Le Combat_PhaseManager n'a pas de NetworkIdentity !");
                }
            }
            else
            {
                Debug.LogWarning("[Phase_EnterSetupCombat] Le joueur n'a pas de Player_SetupNetworkCombat !");
            }

            if (manager != null && manager.phasePrepa != null && manager.phasePrepa.isActiveAndEnabled)
            {
                manager.phasePrepa.PlaceEntity(player);
            }
            else
            {
                Debug.LogWarning("[Phase_EnterSetupCombat] Phase de préparation non active : placement différé.");
            }

            // ✅ Activation du mode combat dans le script stats
            if (player.TryGetComponent(out Entity_StatistiqueCombat stats))
            {
                stats.isFight = true;
            }
        }
        else
        {
            Debug.Log("[Phase_EnterSetupCombat] Joueur déjà dans l'équipe verte : " + player.name);
        }
    }

    //--------------------------------------------------
    public void SetMonsterState(MonsterState newState)
    {
        if (currentGroup != null)
        {
            currentGroup.SetState(newState);
            Debug.Log("[Phase_EnterSetupCombat] État du groupe mis à jour : " + newState);
        }
        else
        {
            Debug.LogWarning("[Phase_EnterSetupCombat] Aucun groupe de monstres référencé.");
        }
    }
}
