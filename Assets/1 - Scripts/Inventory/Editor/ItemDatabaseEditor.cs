
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ItemDatabase))]
public class ItemDatabaseEditor : Editor
{
    string newItemID = "item_id";
    Sprite newIcon;
    int newQuantity = 1;
    int newMaxStack = 99;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);
        GUILayout.Label("Ajouter un nouvel item", EditorStyles.boldLabel);

        newItemID = EditorGUILayout.TextField("ID de l'item", newItemID);
        newIcon = (Sprite)EditorGUILayout.ObjectField("Icône", newIcon, typeof(Sprite), false);
        newQuantity = EditorGUILayout.IntField("Quantité", newQuantity);
        newMaxStack = EditorGUILayout.IntField("Stack Max", newMaxStack);

        if (GUILayout.Button("Ajouter à la base de données"))
        {
            ItemDatabase db = (ItemDatabase)target;

            InventoryItem newItem = new InventoryItem
            {
                itemID = newItemID,
                icon = newIcon,
                quantity = newQuantity,
                maxStack = newMaxStack
            };

            db.items.Add(newItem);
            EditorUtility.SetDirty(db);

            Debug.Log($"Ajouté : {newItemID}");
        }
    }
}
