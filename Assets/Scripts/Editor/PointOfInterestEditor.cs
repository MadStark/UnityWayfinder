using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MadStark.Wayfinder
{
    [CustomEditor(typeof(PointOfInterest))]
    public class PointOfInterestEditor : Editor
    {
        private SerializedProperty accessPointsProp;

        private int searchForAnchor;


        private void OnEnable()
        {
            accessPointsProp = serializedObject.FindProperty(nameof(PointOfInterest.accessPoints));
        }

        public override void OnInspectorGUI()
        {
            if (accessPointsProp.isArray)
            {
                EditorGUILayout.LabelField($"Access Points ({accessPointsProp.arraySize})");
                for (int i = 0; i < accessPointsProp.arraySize; i++)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    EditorGUILayout.PropertyField(accessPointsProp.GetArrayElementAtIndex(i));

                    if (accessPointsProp.GetArrayElementAtIndex(i).objectReferenceValue == null)
                    {
                        EditorGUILayout.BeginHorizontal();

                        searchForAnchor = EditorGUILayout.IntField("Search For Anchor", searchForAnchor);

                        if (GUILayout.Button("Search"))
                        {
                            var nav = FindObjectOfType<GraphManager>();
                            if (nav != null && nav.nodes.Count > searchForAnchor && searchForAnchor >= 0)
                            {
                                accessPointsProp.GetArrayElementAtIndex(i).objectReferenceValue = nav.nodes.FirstOrDefault(x => x.name.EndsWith(searchForAnchor.ToString()));
                            }
                        }

                        EditorGUILayout.EndHorizontal();
                    }

                    if (GUILayout.Button("Delete"))
                    {
                        accessPointsProp.DeleteArrayElementAtIndex(i);
                        break;
                    }

                    EditorGUILayout.EndVertical();
                }

                if (GUILayout.Button("Add"))
                    accessPointsProp.InsertArrayElementAtIndex(accessPointsProp.arraySize);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
