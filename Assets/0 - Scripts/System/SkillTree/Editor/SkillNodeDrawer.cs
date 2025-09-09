//using UnityEditor;
//using UnityEngine;

//[CustomPropertyDrawer(typeof(SkillNode))]
//public class SkillNodeDrawer : PropertyDrawer
//{
//    private readonly float lineHeight = EditorGUIUtility.singleLineHeight;
//    private readonly float spacing = 2f;

//    private static readonly System.Collections.Generic.Dictionary<string, bool> foldoutStates
//        = new System.Collections.Generic.Dictionary<string, bool>();

//    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
//    {
//        bool foldout = GetFoldoutState(prop);
//        // closed foldout = single line
//        if (!foldout)
//            return lineHeight;

//        float total = 0f;
//        // We'll accumulate height from each property using EditorGUI.GetPropertyHeight
//        SerializedProperty p;

//        // skillData
//        p = prop.FindPropertyRelative("skillData");
//        total += EditorGUI.GetPropertyHeight(p, true) + spacing;

//        // manualIcon
//        p = prop.FindPropertyRelative("manualIcon");
//        total += EditorGUI.GetPropertyHeight(p, true) + spacing;

//        // if icon assigned, manualName + manualDescription
//        if (p != null && p.objectReferenceValue != null)
//        {
//            var nameProp = prop.FindPropertyRelative("manualName");
//            total += EditorGUI.GetPropertyHeight(nameProp, true) + spacing;

//            var descProp = prop.FindPropertyRelative("manualDescription");
//            total += EditorGUI.GetPropertyHeight(descProp, true) + spacing;
//        }

//        // cost
//        p = prop.FindPropertyRelative("cost");
//        total += EditorGUI.GetPropertyHeight(p, true) + spacing;

//        // requiredLevel (optional)
//        var requiredProp = prop.FindPropertyRelative("requiredLevel");
//        if (requiredProp != null)
//            total += EditorGUI.GetPropertyHeight(requiredProp, true) + spacing;

//        // isUnlocked
//        p = prop.FindPropertyRelative("isUnlocked");
//        total += EditorGUI.GetPropertyHeight(p, true) + spacing;

//        // onUnlock (UnityEvent peut être multi-line)
//        p = prop.FindPropertyRelative("onUnlock");
//        total += EditorGUI.GetPropertyHeight(p, true) + spacing;

//        // small padding
//        total += spacing;

//        return total;
//    }

//    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
//    {
//        EditorGUI.BeginProperty(pos, label, prop);

//        float x = pos.x;
//        float y = pos.y;
//        float w = pos.width;

//        bool foldout = GetFoldoutState(prop);

//        // build display name like before
//        string displayName = "???";
//        var skillDataProp = prop.FindPropertyRelative("skillData");
//        var manualNameProp = prop.FindPropertyRelative("manualName");

//        if (skillDataProp != null && skillDataProp.objectReferenceValue != null)
//        {
//            var skillData = skillDataProp.objectReferenceValue as Object; // avoid direct type dependency
//            if (skillData != null)
//                displayName = skillData.name;
//        }
//        else if (manualNameProp != null && !string.IsNullOrEmpty(manualNameProp.stringValue))
//        {
//            displayName = manualNameProp.stringValue;
//        }

//        string foldoutLabel = "Skill Node : " + displayName;

//        Rect foldoutRect = new Rect(x, y, w, lineHeight);
//        foldout = EditorGUI.Foldout(foldoutRect, foldout, foldoutLabel, true);
//        SetFoldoutState(prop, foldout);
//        y += lineHeight + spacing;

//        if (!foldout)
//        {
//            EditorGUI.EndProperty();
//            return;
//        }

//        int prevIndent = EditorGUI.indentLevel;
//        EditorGUI.indentLevel = 0; // we draw full width fields; Unity will still draw labels

//        // Helper local to draw a property and advance y by its actual height + spacing
//        System.Action<string, GUIContent> DrawProp = (relativeName, guiContent) =>
//        {
//            var p = prop.FindPropertyRelative(relativeName);
//            if (p == null) return;
//            float h = EditorGUI.GetPropertyHeight(p, true);
//            Rect r = new Rect(x, y, w, h);
//            EditorGUI.PropertyField(r, p, guiContent ?? GUIContent.none, true);
//            y += h + spacing;
//        };

//        // Draw fields in order
//        DrawProp("skillData", new GUIContent("Skill Data"));
//        DrawProp("manualIcon", new GUIContent("Manual Icon"));

//        // If manual icon exists, show manualName + manualDescription
//        var iconProp = prop.FindPropertyRelative("manualIcon");
//        if (iconProp != null && iconProp.objectReferenceValue != null)
//        {
//            DrawProp("manualName", new GUIContent("Manual Name"));
//            DrawProp("manualDescription", new GUIContent("Manual Desc"));
//        }

//        DrawProp("cost", new GUIContent("Cost"));

//        // requiredLevel if present
//        var requiredProp = prop.FindPropertyRelative("requiredLevel");
//        if (requiredProp != null)
//            DrawProp("requiredLevel", new GUIContent("Required Level"));

//        DrawProp("isUnlocked", new GUIContent("Is Unlocked"));

//        DrawProp("onUnlock", new GUIContent("On Unlock"));

//        EditorGUI.indentLevel = prevIndent;
//        EditorGUI.EndProperty();
//    }

//    private bool GetFoldoutState(SerializedProperty prop)
//    {
//        string key = prop.propertyPath;
//        if (!foldoutStates.TryGetValue(key, out bool state))
//        {
//            foldoutStates[key] = true;
//            state = true;
//        }
//        return state;
//    }

//    private void SetFoldoutState(SerializedProperty prop, bool state)
//    {
//        foldoutStates[prop.propertyPath] = state;
//    }
//}
