//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using UnityEditor;
using UnityEngine;

namespace AdvancedInputFieldPlugin.Editor
{
	[CustomEditor(typeof(SettingsData), true)]
	public class SettingsDataEditor: UnityEditor.Editor
	{
		private SerializedProperty localizationsProperty;
		private SerializedProperty mobileKeyboardBehaviourProperty;
		private SerializedProperty androidActionBarPrefabProperty;
		private SerializedProperty iOSActionBarPrefabProperty;
		private SerializedProperty uwpActionBarPrefabProperty;
		private SerializedProperty mobileSelectionCursorsPrefabProperty;
		private SerializedProperty mobileSelectionCursorsScaleProperty;
		private SerializedProperty simulateMobileBehaviourInEditorProperty;
		private SerializedProperty portraitKeyboardCanvasPrefabProperty;
		private SerializedProperty landscapeKeyboardCanvasPrefabProperty;

		private void OnEnable()
		{
			localizationsProperty = serializedObject.FindProperty("localizations");
			mobileKeyboardBehaviourProperty = serializedObject.FindProperty("mobileKeyboardBehaviour");
			androidActionBarPrefabProperty = serializedObject.FindProperty("androidActionBarPrefab");
			iOSActionBarPrefabProperty = serializedObject.FindProperty("iOSActionBarPrefab");
			uwpActionBarPrefabProperty = serializedObject.FindProperty("uwpActionBarPrefab");
			mobileSelectionCursorsPrefabProperty = serializedObject.FindProperty("mobileSelectionCursorsPrefab");
			mobileSelectionCursorsScaleProperty = serializedObject.FindProperty("mobileSelectionCursorsScale");
			simulateMobileBehaviourInEditorProperty = serializedObject.FindProperty("simulateMobileBehaviourInEditor");
			portraitKeyboardCanvasPrefabProperty = serializedObject.FindProperty("portraitKeyboardCanvasPrefab");
			landscapeKeyboardCanvasPrefabProperty = serializedObject.FindProperty("landscapeKeyboardCanvasPrefab");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			SettingsData settingsData = (SettingsData)target;

			EditorGUILayout.LabelField("General:", EditorStyles.boldLabel);
			DrawLocalizationsProperty();
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Mobile:", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(mobileKeyboardBehaviourProperty);
			EditorGUILayout.PropertyField(androidActionBarPrefabProperty);
			EditorGUILayout.PropertyField(iOSActionBarPrefabProperty);
			EditorGUILayout.PropertyField(uwpActionBarPrefabProperty);
			EditorGUILayout.PropertyField(mobileSelectionCursorsPrefabProperty);
			EditorGUILayout.PropertyField(mobileSelectionCursorsScaleProperty);
			EditorGUILayout.PropertyField(simulateMobileBehaviourInEditorProperty);
			EditorGUILayout.PropertyField(portraitKeyboardCanvasPrefabProperty);
			EditorGUILayout.PropertyField(landscapeKeyboardCanvasPrefabProperty);

			serializedObject.ApplyModifiedProperties();
		}

		public void DrawLocalizationsProperty()
		{
			localizationsProperty.isExpanded = EditorGUILayout.Foldout(localizationsProperty.isExpanded, "Localizations");
			if(localizationsProperty.isExpanded)
			{
				EditorGUI.indentLevel = 1;
				int length = localizationsProperty.arraySize;
				length = EditorGUILayout.IntField("Size", length);

				if(length != localizationsProperty.arraySize)
				{
					while(length > localizationsProperty.arraySize)
					{
						localizationsProperty.InsertArrayElementAtIndex(localizationsProperty.arraySize);
					}
					while(length < localizationsProperty.arraySize)
					{
						localizationsProperty.DeleteArrayElementAtIndex(localizationsProperty.arraySize - 1);
					}
					serializedObject.ApplyModifiedProperties();
				}

				for(int i = 0; i < length; i++)
				{
					SerializedProperty localizationProperty = localizationsProperty.GetArrayElementAtIndex(i);
					EditorGUILayout.ObjectField(localizationProperty, new GUIContent("Element " + i));
				}
				EditorGUI.indentLevel = 0;
			}
		}
	}
}