using UnityEngine;
using System.Collections.Generic;
public enum MonsterAIType
{
    Passive,
    Aggressive,
    Fleeing,
    Focused
}

[CreateAssetMenu(fileName = "New Monster", menuName = "Game Creation Tool/Monster")]
public class MonsterData : ScriptableObject
{
    [Header("Identité")]
    public int ID;
    public string monsterName;

    [TextArea(3, 6)]
    public string description;

    public int level;

    [Header("Apparence / Animation")]
    public GameObject monsterPrefab;

    [Header("Compétences")]
    public List<SkillData> skills = new List<SkillData>();

    [Header("Stats")]
    public int PV;
    public int PA;
    public int PM;
    public int PO;
    public int Initiative;
    public int Force;
    public int Dexterite;
    public int magie;
    public int foi;

    [Range(0, 100)]
    public float critChance;

    [Range(0, 100)]
    public float ResForce;

    [Range(0, 100)]
    public float ResDexterite;

    [Range(0, 100)]
    public float ResMagie;

    [Range(0, 100)]
    public float ResFoi;

    [Header("Comportement IA")]
    public MonsterAIType aiType;

    public Sprite icon;
}
