using UnityEngine;

public class Entity_Info : MonoBehaviour
{
    [Header("Information Complèmentaire:")]

    [Header("Pseudo de l'Entité")]
    [SerializeField] public string pseudoEntity;

    [Header("Position Sauvegardé de l'Entité")]
    [SerializeField] public Vector3 savePosEntity;

    [Header("Camera Sauvegardé de l'Entité")]
    [SerializeField] public string saveCamEntity;
}
