using System.Collections.Generic;
using UnityEngine;

namespace MadStark.Wayfinder
{
    public class Path
    {
        public List<Link> links;
        public Transform lastNode;
        public Transform start;
        public float calculatedLength;

        public Path(Transform start)
        {
            links = new List<Link>();
            lastNode = start;
            this.start = start;
            calculatedLength = 0;
        }

        public Path(Path copy)
        {
            links = new List<Link>(copy.links);
            lastNode = copy.lastNode;
            start = copy.start;
            calculatedLength = copy.calculatedLength;
        }

        public float RecalculateLength()
        {
            calculatedLength = 0;
            for (int i = 0; i < links.Count; i++)
                calculatedLength += links[i].CalculateCost();
            return calculatedLength;
        }
    }
}
