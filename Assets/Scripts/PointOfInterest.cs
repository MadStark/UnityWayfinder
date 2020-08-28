using System.Collections.Generic;
using UnityEngine;

namespace MadStark.Wayfinder
{
    public class PointOfInterest : MonoBehaviour
    {
        public List<Transform> accessPoints;

#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            var gizmosColorCache = Gizmos.color;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, 0.35f);


            if (accessPoints == null || accessPoints.Count < 1)
                return;

            var handlesColorCache = UnityEditor.Handles.color;
            UnityEditor.Handles.color = Color.red;

            for (int i = 0; i < accessPoints.Count; i++)
            {
                if (accessPoints[i] == null)
                    continue;
                UnityEditor.Handles.DrawDottedLine(transform.position, accessPoints[i].position, 2f);
            }

            Gizmos.color = gizmosColorCache;
            UnityEditor.Handles.color = handlesColorCache;
        }

#endif

    }
}
