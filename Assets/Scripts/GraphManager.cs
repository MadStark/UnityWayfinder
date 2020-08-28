using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MadStark.Wayfinder
{
    public class GraphManager : MonoBehaviour
    {
        public List<Transform> nodes = new List<Transform>();

        public List<Link> links = new List<Link>();

        [SerializeField, HideInInspector] private int nextNodeId;

        private readonly Vector3 labelOffset = Vector3.up * 0.6f + Vector3.right * 0.5f;


        public Transform AddNode(Vector3 position)
        {
            Transform node = new GameObject($"Node {nextNodeId++}").transform;
            node.parent = transform;
            node.position = position;
            nodes.Add(node);
            return node;
        }

        public void RemoveNode(Transform node)
        {
            if (nodes.Contains(node))
                nodes.Remove(node);
        }

        public Link AddLink(Transform nodeA, Transform nodeB)
        {
            var link = new Link {
                nodeA = nodeA,
                nodeB = nodeB
            };
            links.Add(link);
            return link;
        }

        public List<Link> GetLinksToNode(Transform node)
        {
            return links.FindAll(x => x.nodeA == node || x.nodeB == node).ToList();
        }

        public bool HasLinkBetweenAnchors(Transform nodeA, Transform nodeB)
        {
            return links.FirstOrDefault(link => (link.nodeA == nodeA && link.nodeB == nodeB) || (link.nodeA == nodeB && link.nodeB == nodeA)) != null;
        }

        private void OnValidate()
        {
            nodes.RemoveAll(x => x == null);
            links.RemoveAll(x => x == null);
        }

        private void OnDrawGizmos()
        {
            var colorCache = UnityEditor.Handles.color;
            var yellow = Color.yellow;
            yellow.a = 0.5f;
            UnityEditor.Handles.color = yellow;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (!nodes[i].gameObject.activeInHierarchy)
                    continue;

                UnityEditor.Handles.SphereHandleCap(0, nodes[i].position, Quaternion.identity, 0.98f, Event.current.type);

                UnityEditor.Handles.Label(nodes[i].position + labelOffset, nodes[i].name.Substring(5), new GUIStyle { normal = new GUIStyleState { textColor = Color.magenta }, fontSize = 15});
            }
            UnityEditor.Handles.color = colorCache;
        }
    }
}
