using UnityEngine;

//----------------------------------------------------------
public class Exploration_Trigger_GoCombat : MonoBehaviour
{
    // Détection de la collision avec un autre collider
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("[Trigger] Collision détectée avec : " + other.name);

        // Vérifie que l'objet entrant est bien un joueur
        if (!other.CompareTag("Player")) return;

        Debug.Log("[Trigger] Collision avec un joueur validée.");

        // Récupère les infos du groupe de monstres déclenché
        Exploration_InfoGroupMonster info = GetComponentInParent<Exploration_InfoGroupMonster>();

        if (info == null)
        {
            Debug.LogWarning("[Combat Error] Aucun Exploration_InfoGroupMonster trouvé sur le parent.");
            return;
        }

        // Recherche du manager d'arène via l'index donné
        Combat_PhaseManager phaseManager = FindArenaByIndex(info.arenaIndex);

        if (phaseManager == null)
        {
            Debug.LogWarning("[ArenaFinder] Aucune arène trouvée avec l'index : " + info.arenaIndex);
            return;
        }

        // Accès à la phaseEnter via le manager
        var phaseEnter = phaseManager.phaseEnter;

        // Cas 1 : monstre libre
        // Cas 1 : monstre libre
        if (info.IsState(MonsterState.InNature))
        {
            Debug.Log("[Combat Init] Monstre dans l'état InNature : lancement du combat.");

            // Passe l'état à Attacked
            info.SetState(MonsterState.Attacked);

            // Envoie les données du groupe à la phase d'entrée
            phaseEnter.SetCombatData(info.monstersInGroup, info.combatMap, info);
            Debug.Log("[Combat Init] Données de combat envoyées à la phase d'entrée.");

            // Ajoute le joueur à l'équipe verte
            phaseEnter.AddPlayerToTeamVerte(other.gameObject);
            Debug.Log("[Combat Init] Premier joueur ajouté à l'équipe verte.");

            // ⚠️ Lancement du système de phase uniquement maintenant
            phaseManager.LaunchCombat();
        }

        // Cas 2 : déjà engagé, mais pas encore en combat
        else if (info.IsState(MonsterState.Attacked))
        {
            Debug.Log("[Combat Join] Groupe déjà engagé. Ajout du joueur.");

            phaseEnter.AddPlayerToTeamVerte(other.gameObject);
        }
        // Cas 3 : en combat actif
        else if (info.IsState(MonsterState.InFight))
        {
            Debug.LogWarning("[Combat Refusé] Groupe déjà en combat actif (InFight).");
        }
    }

    // Recherche le Combat_PhaseManager de l'arène correspondante
    private Combat_PhaseManager FindArenaByIndex(int index)
    {
        Debug.Log("[ArenaFinder] Recherche du Combat_PhaseManager avec index : " + index);

        Combat_PhaseManager[] allManagers = Object.FindObjectsByType<Combat_PhaseManager>(FindObjectsSortMode.None);

        foreach (Combat_PhaseManager manager in allManagers)
        {
            if (manager.arenaIndex == index)
            {
                Debug.Log("[ArenaFinder] Combat_PhaseManager trouvé : " + manager.name);
                return manager;
            }
        }

        return null;
    }
}
