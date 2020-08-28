using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

namespace MadStark.Wayfinder
{
    public class JulesStarBrain
    {
        readonly Dictionary<Transform, List<Link>> linksLuT;
        readonly List<Path> openPaths;
        readonly HashSet<Link> alreadyExploredLinks;

        private JulesStarBrain() { }

        public JulesStarBrain(Dictionary<Transform, List<Link>> anchorsAndLinks)
        {
            linksLuT = anchorsAndLinks;
            openPaths = new List<Path>();
            alreadyExploredLinks = new HashSet<Link>();
        }

        public Path GetBestPath(IEnumerable<Transform> startNodes, HashSet<Transform> ends)
        {
            Profiler.BeginSample("JulesStar.GetBestPath");

            openPaths.Clear();
            foreach (Transform node in startNodes)
                openPaths.Add(new Path(node));

            alreadyExploredLinks.Clear();

            Path r = JulesStarRecursive(openPaths, ends);

            Profiler.EndSample();

            return r;
        }

        private Path JulesStarRecursive(List<Path> paths, HashSet<Transform> targetNodes)
        {
            if (paths.Count == 0)
                return null; // Start and end are not connected

            paths = paths.OrderBy(x => x.calculatedLength).ToList();
            var pathToExplore = paths.First();
            paths.Remove(pathToExplore);

            if (targetNodes.Contains(pathToExplore.lastNode))
                return pathToExplore; // Done, return best path

            List<Link> connections = linksLuT[pathToExplore.lastNode];
            for (int i = 0; i < connections.Count; i++)
            {
                Link connection = connections[i];
                if (alreadyExploredLinks.Contains(connection))
                    continue; // Node already explored: skip

                alreadyExploredLinks.Add(connection);

                Transform previousAnchor = pathToExplore.lastNode;
                Transform nextAnchor = connection.GetOtherNode(previousAnchor);

                Path pathBranch = new Path(pathToExplore);
                do
                {
                    pathBranch.links.Add(connection);
                    pathBranch.calculatedLength += connection.CalculateCost();

                    if (targetNodes.Contains(nextAnchor))
                        break;

                    var connectionsToNextAnchor = linksLuT[nextAnchor];
                    if (connectionsToNextAnchor.Count != 2)
                        break;

                    connection = connectionsToNextAnchor[0];
                    if (previousAnchor == connection.GetOtherNode(nextAnchor))
                        connection = connectionsToNextAnchor[1];

                    Assert.IsFalse(alreadyExploredLinks.Contains(connection), "Path should not have been explored already.");

                    previousAnchor = nextAnchor;
                    nextAnchor = connection.GetOtherNode(nextAnchor);

                } while (true);

                alreadyExploredLinks.Add(connection);
                pathBranch.lastNode = nextAnchor;
                paths.Add(pathBranch);
            }

            // Continue exploring this path
            return JulesStarRecursive(paths, targetNodes);
        }
    }
}
