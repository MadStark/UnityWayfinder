using System;
using System.Collections.Generic;
using System.Linq;
using MadStark.Wayfinder;
using UnityEngine;

public class Wayfinder : MonoBehaviour
{
    public GraphManager graph;

    public PointOfInterest start;
    public PointOfInterest end;

    public GameObject startIndicator;
    public GameObject endIndicator;

    public PathDisplay pathDisplay;

    private JulesStarBrain brain;

    private Path bestPath;


    private void Update()
    {
        DoPathFinding();
    }

    private void DoPathFinding()
    {
        if (brain == null)
        {
            Dictionary<Transform, List<Link>> linkMap = graph.nodes.ToDictionary(node => node, graph.GetLinksToNode);
            brain = new JulesStarBrain(linkMap);
        }

        bestPath = brain.GetBestPath(start.accessPoints, new HashSet<Transform>(end.accessPoints));
        if (bestPath != null)
        {
            pathDisplay.Display(bestPath);
            startIndicator.transform.position = bestPath.start.position + (1f * -Vector3.forward);
            endIndicator.transform.position = bestPath.lastNode.position + (1f * -Vector3.forward);
        }
    }
}
