using UnityEngine;

[CreateAssetMenu(fileName = "NewSpell", menuName = "Combat/Spell")]
public class SpellData : ScriptableObject
{
    public string spellName;
    public int costPA = 3;
    public int range = 4;
    public int damage = 20;
}
