using UnityEditor;
using UnityEngine;


[CustomPropertyDrawer(typeof(SkillNode))]
public class SkillNodeDrawer : UnityEditor.PropertyDrawer
{
    private readonly float lineHeight = EditorGUIUtility.singleLineHeight;
    private readonly float spacing = 2f;

    private static readonly System.Collections.Generic.Dictionary<string, bool> foldoutStates
        = new System.Collections.Generic.Dictionary<string, bool>();

    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    {
        bool foldout = GetFoldoutState(prop);

        if (!foldout)
        {
            return lineHeight;
        }

        var iconProp = prop.FindPropertyRelative("manualIcon");
        bool hasIcon = iconProp.objectReferenceValue != null;

        var onUnlockProp = prop.FindPropertyRelative("onUnlock");

        int lines = 4; 
        if (hasIcon) lines += 2;

        float height = lines * lineHeight + (lines - 1) * spacing;

        if (hasIcon)
        {
            height += lineHeight + spacing;
        }

        height += spacing + EditorGUI.GetPropertyHeight(onUnlockProp, true);
        height += spacing;

        return height;
    }

    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
    {
        float y = pos.y;
        float w = pos.width;

        bool foldout = GetFoldoutState(prop);

        string foldoutLabel = "Skill Node";

        var skillDataProp = prop.FindPropertyRelative("skillData");
        var manualNameProp = prop.FindPropertyRelative("manualName");
        string displayName = "???";

        if (skillDataProp.objectReferenceValue != null)
        {
            var skillData = skillDataProp.objectReferenceValue as SkillData;
            if (skillData != null)
                displayName = skillData.skillName;
        }
        else if (!string.IsNullOrEmpty(manualNameProp.stringValue))
        {
            displayName = manualNameProp.stringValue;
        }

        foldoutLabel += " : " + displayName;

        Rect foldoutRect = new Rect(pos.x, y, w, lineHeight);
        foldout = EditorGUI.Foldout(foldoutRect, foldout, foldoutLabel, true);
        SetFoldoutState(prop, foldout);
        y += lineHeight + spacing;

        if (!foldout)
        {
            return;
        }

        EditorGUI.PropertyField(
            new Rect(pos.x, y, w, lineHeight),
            skillDataProp,
            new GUIContent("Skill Data")
        );
        y += lineHeight + spacing;

        var iconProp = prop.FindPropertyRelative("manualIcon");
        EditorGUI.PropertyField(
            new Rect(pos.x, y, w, lineHeight),
            iconProp,
            new GUIContent("Manual Icon")
        );
        y += lineHeight + spacing;

        if (iconProp.objectReferenceValue != null)
        {
            var nameProp = prop.FindPropertyRelative("manualName");
            EditorGUI.PropertyField(
                new Rect(pos.x, y, w, lineHeight),
                nameProp,
                new GUIContent("Manual Name")
            );
            y += lineHeight + spacing;

            var descProp = prop.FindPropertyRelative("manualDescription");
            float descHeight = lineHeight * 2 + spacing;
            EditorGUI.PropertyField(
                new Rect(pos.x, y, w, descHeight),
                descProp,
                new GUIContent("Manual Desc")
            );
            y += descHeight;
        }

        var costProp = prop.FindPropertyRelative("cost");
        EditorGUI.PropertyField(
            new Rect(pos.x, y, w, lineHeight),
            costProp
        );
        y += lineHeight + spacing;

        var unlockedProp = prop.FindPropertyRelative("isUnlocked");
        EditorGUI.PropertyField(
            new Rect(pos.x, y, w, lineHeight),
            unlockedProp,
            new GUIContent("Is Unlocked")
        );
        y += lineHeight + spacing;

        var onUnlockProp = prop.FindPropertyRelative("onUnlock");
        float onUnlockHeight = EditorGUI.GetPropertyHeight(onUnlockProp, true);
        EditorGUI.PropertyField(
            new Rect(pos.x, y, w, onUnlockHeight),
            onUnlockProp,
            new GUIContent("On Unlock"),
            true
        );
    }

    private bool GetFoldoutState(SerializedProperty prop)
    {
        string key = prop.propertyPath;
        if (!foldoutStates.TryGetValue(key, out bool state))
        {
            foldoutStates[key] = true;
            state = true;
        }
        return state;
    }

    private void SetFoldoutState(SerializedProperty prop, bool state)
    {
        foldoutStates[prop.propertyPath] = state;
    }
}
