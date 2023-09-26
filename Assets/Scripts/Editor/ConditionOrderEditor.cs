using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ConditionOrder))]
public class ConditionOrderEditor : Editor
{
    /*private SerializedProperty _conditions;
    private SerializedProperty _defaultCondition;
    private SerializedProperty _conditionOrderList;
    private SerializedProperty _participantsPerCondition;
    private SerializedProperty _currParticipantNumber;

    private void OnEnable()
    {
        _conditions = serializedObject.FindProperty("conditions");
        _defaultCondition = serializedObject.FindProperty("defaultCondition");
        _conditionOrderList = serializedObject.FindProperty("conditionOrderList");
        _participantsPerCondition = serializedObject.FindProperty("participantsPerCondition");
        _currParticipantNumber = serializedObject.FindProperty("currParticipantNumber");
    }*/

    bool showButtons = false;
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ConditionOrder script = (ConditionOrder)target;
        var conditionText = script.GetCurrentCondition() ?? "empty";
        ReadOnlyTextField("Current Condition: ", conditionText);
        
        showButtons = EditorGUILayout.Toggle("Open List Buttons", showButtons);

        if (!showButtons) return;
        
        if(GUILayout.Button("Generate List", GUILayout.Height(30)))
        {
            script.GenerateList();
        }
            
        if(GUILayout.Button("Randomize List", GUILayout.Height(30)))
        {
            script.RandomizeList();
        }
    }
    
    private void ReadOnlyTextField(string label, string text)
    {
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
            EditorGUILayout.SelectableLabel(text, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
        }
        EditorGUILayout.EndHorizontal();
    }
}