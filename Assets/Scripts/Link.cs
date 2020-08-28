using System;
using UnityEngine;

namespace MadStark.Wayfinder
{
    [Serializable]
    public class Link
    {
        public Transform nodeA;

        public Transform nodeB;

        public LinkCostMode costMode;

        public float cost;


        public Transform GetOtherNode(Transform node)
        {
            if (node == nodeA)
                return nodeB;
            if (node == nodeB)
                return nodeA;

            throw new Exception($"Cannot find opposite node of {node.name} in the link {this}.");
        }

        public float CalculateCost()
        {
            switch (costMode)
            {
                case LinkCostMode.Distance:
                    return Vector3.Distance(nodeA.position, nodeB.position);
                case LinkCostMode.Value:
                    return cost;
                case LinkCostMode.Weighted:
                    return Vector3.Distance(nodeA.position, nodeB.position) * cost;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override string ToString()
        {
            return $"({nodeA.name}, {nodeB.name})";
        }
    }
}
