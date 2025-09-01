using System.Collections.Generic;
using UnityEngine;

public class Entity_Info : MonoBehaviour
{
    [Header("Information Complèmentaire:")]

    [Header("Pseudo de l'Entité")]
    [SerializeField] public string entity_Name;

    [Header("Level de l'Entité")]
    [SerializeField] public int entity_Level;

    [Header("Icon (Sprite) de l'Entité")]
    [SerializeField] public Sprite entity_Icon;

    [Header("Liste des Ressources Dropables sur lui")]
    public List<DropRessource> listDropRessources = new();

    [Header("Gain d'Xp si vaincu")]
    [SerializeField] public float gainXp;

    [Header("Position Sauvegardé de l'Entité")]
    [SerializeField] public Vector3 savePosEntity;

    [Header("Camera Sauvegardé de l'Entité")]
    [SerializeField] public string saveCamEntity;
}

[System.Serializable]
public class DropRessource
{
    [Tooltip("Prefab de la ressource à drop")]
    public GameObject ressourcePrefab;

    [Tooltip("Pourcentage de chance de drop (0-100)")]
    [Range(0f, 100f)] public float dropChance;
}
