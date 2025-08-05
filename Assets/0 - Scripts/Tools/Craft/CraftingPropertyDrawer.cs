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

        SerializedProperty typeProp = property.FindPropertyRelative("ingredientType")
                                 ?? property.FindPropertyRelative("resultType");

        SerializedProperty resourceProp = property.FindPropertyRelative("resource");
        SerializedProperty equipmentProp = property.FindPropertyRelative("equipment");
        SerializedProperty quantityProp = property.FindPropertyRelative("quantity");

        float h = EditorGUIUtility.singleLineHeight;
        float spacing = 2f;

        Rect rType = new Rect(position.x, position.y, position.width, h);
        Rect rItem = new Rect(position.x, position.y + h + spacing, position.width, h);
        Rect rQuantity = new Rect(position.x, position.y + 2 * (h + spacing), position.width, h);

        EditorGUI.PropertyField(rType, typeProp);

        if (typeProp.enumValueIndex == 0)
        {
            equipmentProp.objectReferenceValue = null;
            EditorGUI.PropertyField(rItem, resourceProp, new GUIContent("Resource"));
        }
        else
        {
            resourceProp.objectReferenceValue = null;
            EditorGUI.PropertyField(rItem, equipmentProp, new GUIContent("Equipment"));
        }

        EditorGUI.PropertyField(rQuantity, quantityProp);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 3 * EditorGUIUtility.singleLineHeight + 2 * 2f;
    }
}
#endif
