using UnityEngine;

public enum TeamType
{
    Green,
    Red
}

[System.Serializable]
public class EntityData
{
    public int id;
    public TeamType team;
    public GameObject reference;
    public Vector2Int position;
    public int currentHP;
    public int level;

    public float resistanceForce;
    public float resistanceDex;
    public float resistanceMagie;
    public float resistanceFoi;
}
