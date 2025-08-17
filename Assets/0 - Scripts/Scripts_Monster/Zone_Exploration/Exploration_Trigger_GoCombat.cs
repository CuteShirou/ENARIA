using UnityEngine;
using System.Collections;

//----------------------------------------------------------
public class Exploration_Trigger_GoCombat : MonoBehaviour
{
    [Header("Save Settings (Offsets)")]
    [SerializeField] private float offsetX = 0f;  // Décalage appliqué sur l'axe X lors de la sauvegarde de la position
    [SerializeField] private float offsetZ = 0f;  // Décalage appliqué sur l'axe Y lors de la sauvegarde de la position

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

        //Sauvegarde les informations liée à l'exploration du joueur (position + caméra)
        SavePlayerExplorationContext(other.gameObject, info);

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
            StartCoroutine(DelayedLaunchCombat(phaseManager));
        }

        // Cas 2 : déjà engagé, mais pas encore en combat
        else if (info.IsState(MonsterState.Attacked))
        {
            Debug.Log("[Combat Join] Groupe déjà engagé. Ajout du joueur.");

            //phaseEnter.AddPlayerToTeamVerte(other.gameObject);
        }
        // Cas 3 : en combat actif
        else if (info.IsState(MonsterState.InFight))
        {
            Debug.LogWarning("[Combat Refusé] Groupe déjà en combat actif (InFight).");
        }
    }

    private void SavePlayerExplorationContext(GameObject playerGO, Exploration_InfoGroupMonster monsterInfo)
    {
        // Récupération du composant Entity_Info sur le joueur
        Entity_Info entityInfo = playerGO.GetComponent<Entity_Info>();
        if (entityInfo == null)
        {
            Debug.LogWarning("[SaveContext] Entity_Info introuvable sur le joueur : " + playerGO.name);
            return;
        }

        // Calcul de la position avec décalage (X,Z) en espace monde

        Vector3 originalPos = playerGO.transform.position;
        Vector3 savedPos = new Vector3(originalPos.x + offsetX, originalPos.y, originalPos.z + offsetZ);

        // Sauvegarde de la position dans Entity_Info
        entityInfo.savePosEntity = savedPos;

        // Sauvegarde de la caméra d'exploration affiliée au monstre (string id/nom)
        entityInfo.saveCamEntity = monsterInfo.cameraExplo;

        // Logs pour debug
        Debug.Log($"[SaveContext] Position sauvegardée (avec offset) pour '{playerGO.name}' = {savedPos}");
        if (!string.IsNullOrEmpty(entityInfo.saveCamEntity))
            Debug.Log($"[SaveContext] Caméra d'exploration associée = '{entityInfo.saveCamEntity}'");
        else
            Debug.LogWarning("[SaveContext] cameraExplo vide sur le groupe de monstres, aucun nom de caméra sauvegardé.");
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

    private IEnumerator DelayedLaunchCombat(Combat_PhaseManager manager)
    {
        yield return null; // attend une frame
        manager.LaunchCombat();
    }
}
