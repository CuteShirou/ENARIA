using UnityEngine;

[CreateAssetMenu(fileName = "New Resource", menuName = "Game Creation Tool/Resource")]
public class ResourceData : ScriptableObject
{
    public int ID;
    public string resourceName;

    [TextArea(2, 5)]
    public string description;

    public Sprite icon;
}
