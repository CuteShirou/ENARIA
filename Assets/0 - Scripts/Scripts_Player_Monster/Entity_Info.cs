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

    [Header("Position Sauvegardé de l'Entité")]
    [SerializeField] public Vector3 savePosEntity;

    [Header("Camera Sauvegardé de l'Entité")]
    [SerializeField] public string saveCamEntity;
}
