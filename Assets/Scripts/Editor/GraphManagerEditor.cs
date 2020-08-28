using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MadStark.Wayfinder
{
    [CustomEditor(typeof(GraphManager))]
    public class GraphManagerEditor : Editor
    {
        private bool addMode;
        private bool moveMode;

        private SelectionInfo selectionInfo;
        private bool needsHandlesRepaint;

        private GraphManager graphManager;


        private void OnEnable()
        {
            addMode = false;
            moveMode = false;
            graphManager = (GraphManager)target;
            selectionInfo = new SelectionInfo();

            graphManager.nodes.RemoveAll(x => x == null);
            graphManager.links.RemoveAll(x => x == null);
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginHorizontal();
            addMode = GUILayout.Toggle(addMode, "Additive Mode", "Button", GUILayout.Width(100f));
            moveMode = GUILayout.Toggle(moveMode, "Move Mode", "Button", GUILayout.Width(100f));
            EditorGUILayout.EndHorizontal();

            if (selectionInfo.linkSelection != null)
            {
                EditorGUILayout.BeginVertical("Box");

                Undo.RecordObject(graphManager, "Change Link Type");
                selectionInfo.linkSelection.costMode = (LinkCostMode)EditorGUILayout.EnumPopup("Selected Link Cost Mode", selectionInfo.linkSelection.costMode);

                if (selectionInfo.linkSelection.costMode == LinkCostMode.Value)
                    selectionInfo.linkSelection.cost = EditorGUILayout.FloatField("Fixed Cost", selectionInfo.linkSelection.cost);
                else if (selectionInfo.linkSelection.costMode == LinkCostMode.Weighted)
                    selectionInfo.linkSelection.cost = EditorGUILayout.FloatField("Distance Factor", selectionInfo.linkSelection.cost);

                EditorGUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            var handlesColorCache = Handles.color;

            Event guiEvent = Event.current;

            bool anchorsSelected = selectionInfo.anchorSelection.Count > 0;
            Tools.hidden = moveMode && (anchorsSelected || selectionInfo.linkSelection != null);
            if (anchorsSelected && moveMode)
            {
                var handleCenter = selectionInfo.anchorSelection[0].position;

                var newPos = Handles.PositionHandle(handleCenter, Quaternion.identity);
                //newPos = Handles.D(handleCenter, Quaternion.identity, 1f, Vector3.zero, Handles.CircleCap);

                var deltaPos = newPos - handleCenter;
                Undo.RecordObjects(selectionInfo.anchorSelection.ToArray(), "Move Anchor(s)");
                for (int i = 0; i < selectionInfo.anchorSelection.Count; i++)
                {
                    selectionInfo.anchorSelection[i].position += deltaPos;
                }
            }

            if (guiEvent.type == EventType.Repaint)
            {
                Draw();
            }
            else if (guiEvent.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            }
            else
            {
                HandleInput(guiEvent);

                if (needsHandlesRepaint)
                    HandleUtility.Repaint();
            }

            Handles.color = handlesColorCache;
        }

        private void HandleInput(Event guiEvent)
        {
            UpdateMouseOver(guiEvent.mousePosition);

            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0)
            {
                HandleLeftMouseDown(guiEvent);
            }

            if (guiEvent.type == EventType.MouseUp && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
            {
                HandleLeftMouseUp(guiEvent);
            }

            if (guiEvent.type == EventType.MouseDrag && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
            {
                HandleLeftMouseDrag(guiEvent);
            }

            if (guiEvent.type == EventType.KeyDown)
            {
                HandleKeyDown(guiEvent);
            }
        }

        private void HandleLeftMouseDown(Event guiEvent)
        {
            if (selectionInfo.mouseOverAnchor != null)
            {
                if (guiEvent.modifiers == EventModifiers.Shift)
                {
                    selectionInfo.ToggleSelection(selectionInfo.mouseOverAnchor, true);
                    needsHandlesRepaint = true;
                    Repaint();
                }
                else if (guiEvent.modifiers == EventModifiers.None)
                {
                    selectionInfo.ToggleSelection(selectionInfo.mouseOverAnchor, false);
                    needsHandlesRepaint = true;
                    Repaint();
                }
            }
            else if (selectionInfo.mouseOverLink != null)
            {
                if (guiEvent.modifiers == EventModifiers.None || guiEvent.modifiers == EventModifiers.Shift)
                {
                    selectionInfo.ToggleSelection(selectionInfo.mouseOverLink);
                    needsHandlesRepaint = true;
                    Repaint();
                }
            }
            else if (addMode && (guiEvent.modifiers == EventModifiers.None || guiEvent.modifiers == EventModifiers.Shift))
            {
                Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
                Vector3 clickPosWS = mouseRay.origin;
                clickPosWS.z = 0;

                Undo.RecordObject(graphManager, "Add point");
                var newNode = graphManager.AddNode(clickPosWS);
                needsHandlesRepaint = true;

                if (guiEvent.modifiers == EventModifiers.Shift && selectionInfo.anchorSelection.Count == 1)
                {
                    if (!graphManager.HasLinkBetweenAnchors(selectionInfo.anchorSelection[0], newNode))
                    {
                        Undo.RecordObject(graphManager, "Add link");
                        graphManager.AddLink(selectionInfo.anchorSelection[0], newNode);
                        needsHandlesRepaint = true;
                    }
                }

                selectionInfo.ToggleSelection(newNode, false);
            }
        }

        private void HandleKeyDown(Event guiEvent)
        {
            if (guiEvent.keyCode == KeyCode.Delete)
            {
                if (selectionInfo.anchorSelection.Count > 0)
                {
                    for (int i = selectionInfo.anchorSelection.Count - 1; i >= 0; i--)
                    {
                        Undo.RecordObject(graphManager, "Delete Anchor(s)");
                        graphManager.links.RemoveAll(x => x.nodeA == selectionInfo.anchorSelection[i] || x.nodeB == selectionInfo.anchorSelection[i]);
                        var go = selectionInfo.anchorSelection[i].gameObject;
                        graphManager.RemoveNode(selectionInfo.anchorSelection[i]);
                        Undo.DestroyObjectImmediate(go);
                    }

                    selectionInfo.ClearSelection();
                    needsHandlesRepaint = true;
                }
                else if (selectionInfo.linkSelection != null)
                {
                    Undo.RecordObject(graphManager, "Delete Link(s)");
                    graphManager.links.Remove(selectionInfo.linkSelection);
                    selectionInfo.ClearSelection();
                    needsHandlesRepaint = true;
                }
                guiEvent.Use();
            }

            if (guiEvent.keyCode == KeyCode.L && selectionInfo.anchorSelection.Count == 2)
            {
                if (!graphManager.HasLinkBetweenAnchors(selectionInfo.anchorSelection[0], selectionInfo.anchorSelection[1]))
                {
                    Undo.RecordObject(graphManager, "Add link");
                    graphManager.AddLink(selectionInfo.anchorSelection[0], selectionInfo.anchorSelection[1]);
                    needsHandlesRepaint = true;
                }
                guiEvent.Use();
            }
        }

        private void HandleLeftMouseUp(Event guiEvent) { }

        private void HandleLeftMouseDrag(Event guiEvent) { }

        private void Draw()
        {
            for (int i = 0; i < graphManager.links.Count; i++)
            {
                if (!graphManager.links[i].nodeA.gameObject.activeInHierarchy || !graphManager.links[i].nodeB.gameObject.activeInHierarchy)
                    continue;

                switch (graphManager.links[i].costMode)
                {
                    default:
                        if (selectionInfo.linkSelection == graphManager.links[i])
                            Handles.color = Color.cyan;
                        else if (selectionInfo.mouseOverLink == graphManager.links[i])
                            Handles.color = Color.Lerp(Color.blue, Color.cyan, 0.5f);
                        else
                            Handles.color = Color.blue;
                        Handles.DrawLine(graphManager.links[i].nodeA.position, graphManager.links[i].nodeB.position);
                        break;
                }
            }

            for (int i = 0; i < graphManager.nodes.Count; i++)
            {
                if (!graphManager.nodes[i].gameObject.activeInHierarchy)
                    continue;

                if (selectionInfo.anchorSelection.Contains(graphManager.nodes[i]))
                    Handles.color = Color.cyan;
                else if (selectionInfo.mouseOverAnchor == graphManager.nodes[i])
                    Handles.color = Color.Lerp(Color.yellow, Color.cyan, 0.3f);
                else
                    Handles.color = Color.yellow;
                Handles.SphereHandleCap(0, graphManager.nodes[i].position, Quaternion.identity, 1, Event.current.type);

                //Handles.Label(navigationManager.anchorPoints[i].position, $"{i}", new GUIStyle { normal = new GUIStyleState { textColor = Color.magenta } });
            }
        }

        private void UpdateMouseOver(Vector2 mousePosition)
        {
            var camera = SceneView.currentDrawingSceneView.camera;

            var windowSize = SceneView.currentDrawingSceneView.position.size;
            mousePosition.y = windowSize.y - mousePosition.y;
            var viewportMousePosition = mousePosition / windowSize;

            Transform point = null;
            if (camera != null)
            {
                for (int i = 0; i < graphManager.nodes.Count; i++)
                {
                    var a = camera.WorldToViewportPoint(graphManager.nodes[i].position);
                    a.z = 0;

                    if (Vector2.Distance(a, viewportMousePosition) < 0.02f)
                    {
                        point = graphManager.nodes[i];
                        break;
                    }
                }
            }

            if (point != selectionInfo.mouseOverAnchor)
            {
                selectionInfo.mouseOverAnchor = point;
                needsHandlesRepaint = true;
            }

            Link link = null;
            if (point == null && camera != null)
            {
                for (int i = 0; i < graphManager.links.Count; i++)
                {
                    var a = camera.WorldToViewportPoint(graphManager.links[i].nodeA.position);
                    a.z = 0;
                    var b = camera.WorldToViewportPoint(graphManager.links[i].nodeB.position);
                    b.z = 0;
                    float distance = DistancePointToSegment(viewportMousePosition, a, b);

                    if (distance < 0.008f)
                    {
                        link = graphManager.links[i];
                        break;
                    }
                }
            }

            if (link != selectionInfo.mouseOverLink)
            {
                selectionInfo.mouseOverLink = link;
                needsHandlesRepaint = true;
            }
        }

        public static float DistancePointToSegment(Vector3 point, Vector3 a, Vector3 b)
        {
            //Subject 1.02: How do I find the distance from a point to a line? link : http://www.faqs.org/faqs/graphics/algorithms-faq/
            var ab = b - a;
            var r = Vector3.Dot(point - a, ab) / ab.sqrMagnitude;
            if (r <= 0)
                return Vector3.Distance(point, a);
            if (r >= 1)
                return Vector3.Distance(point, b);
            var intersect = a + r * ab;
            return Vector3.Distance(point, intersect);
        }

        public class SelectionInfo
        {
            public List<Transform> anchorSelection = new List<Transform>();
            public Transform mouseOverAnchor;

            public Link linkSelection;
            public Link mouseOverLink;

            public void ToggleSelection(Transform anchor, bool additive)
            {
                bool currentlySelected = IsSelected(anchor);
                if (!currentlySelected)
                {
                    if (!additive && anchorSelection.Count > 0)
                        anchorSelection.Clear();

                    ClearLinkSelection();
                    anchorSelection.Add(anchor);
                }
                else
                {
                    if (additive)
                        anchorSelection.Remove(anchor);
                    else
                    {
                        if (anchorSelection.Count > 1)
                        {
                            ClearLinkSelection();
                            anchorSelection.Clear();
                            anchorSelection.Add(anchor);
                        }
                        else
                            anchorSelection.Remove(anchor);
                    }
                }
            }

            public void ToggleSelection(Link link)
            {
                bool currentlySelected = IsSelected(link);

                if (!currentlySelected)
                {
                    ClearAnchorSelection();
                    linkSelection = link;
                }
                else
                {
                    linkSelection = null;
                }
            }

            public void ClearAnchorSelection()
            {
                anchorSelection.Clear();
            }

            public void ClearLinkSelection()
            {
                linkSelection = null;
            }

            public void ClearSelection()
            {
                ClearAnchorSelection();
                ClearLinkSelection();
            }

            public bool IsSelected(Transform anchor)
            {
                return anchorSelection.Contains(anchor);
            }

            public bool IsSelected(Link link)
            {
                return linkSelection == link;
            }
        }
    }
}
