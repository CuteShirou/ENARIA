#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(CraftIngredient))]
[CustomPropertyDrawer(typeof(CraftResult))]
public class CraftingPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty itemProp = property.FindPropertyRelative("item");
        SerializedProperty quantityProp = property.FindPropertyRelative("quantity");

        float h = EditorGUIUtility.singleLineHeight;
        float spacing = 2f;

        Rect rItem = new Rect(position.x, position.y, position.width, h);
        Rect rQuantity = new Rect(position.x, position.y + h + spacing, position.width, h);

        EditorGUI.PropertyField(rItem, itemProp, new GUIContent("Objet"));
        EditorGUI.PropertyField(rQuantity, quantityProp, new GUIContent("Quantité"));

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float h = EditorGUIUtility.singleLineHeight;
        float spacing = 2f;
        return 2 * h + spacing;
    }
}
#endif
