using UnityEngine;
using waypoints.path;

namespace waypoints.driver
{
    public static class WaypointsAiHelper
    {
        private static readonly AnimationCurve scanDistance = new AnimationCurve();
        private static WaypointsPathProvider targetPathProvider;

        public static void Init(
            WaypointsPathProvider pathProvider, 
            float maxScanDistance,
            float minScanDistance,
            float maxAheadPathOffset)
        {
            targetPathProvider = pathProvider;
            BuildScanDistanceCurve(pathProvider, maxScanDistance, minScanDistance, maxAheadPathOffset);
        }

        public static float GetScanDistance(float pathDistance, float defaultScanDistance)
        {
            if (scanDistance.keys.Length == 0)
            {
                return defaultScanDistance;
            }
            var clamped = CircularPathHelper.GetClampedDistance(targetPathProvider.Length, pathDistance);
            return scanDistance.Evaluate(clamped);
        }

        private static void BuildScanDistanceCurve(
            WaypointsPathProvider pathProvider,
            float maxScanDistance,
            float minScanDistance,
            float maxAheadPathOffset)
        {
            scanDistance.keys = new Keyframe[0];

            const float scanStep = 1f;
            
            var fromPoint = new WaypointsPathProvider.PathPoint();
            
            fromPoint.CopyFrom(pathProvider.GetClosestPoint(0f));
            var distance = 0f;
            
            while (distance < pathProvider.Length)
            {
                distance += scanStep;

                var aheadScanDistance = FindAheadDistance(pathProvider, fromPoint, maxScanDistance, scanStep, maxAheadPathOffset);
                scanDistance.AddKey(distance, Mathf.Max(minScanDistance, aheadScanDistance));
                fromPoint.CopyFrom(pathProvider.GetClosestPoint(distance));
            }
        }

        private static float FindAheadDistance(
            WaypointsPathProvider pathProvider,
            WaypointsPathProvider.PathPoint fromPoint, 
            float maxScanDistance,
            float scanStep,
            float pathOffsetThreshold)
        {
            var distance = fromPoint.pathDistance;
            var targetPoint = new WaypointsPathProvider.PathPoint();
            
            while (distance < fromPoint.pathDistance + maxScanDistance)
            {
                distance += scanStep;

                targetPoint.CopyFrom(pathProvider.GetClosestPoint(distance));

                var deltaDistance =
                    CircularPathHelper.GetPathDelta(pathProvider.Length, fromPoint.pathDistance, distance);
                
                if (deltaDistance > maxScanDistance 
                    || IsTooFarFromCurrentPath(pathProvider, fromPoint, targetPoint, pathOffsetThreshold))
                {
                    return deltaDistance;
                }
            }

            return maxScanDistance;
        }

        private static bool IsTooFarFromCurrentPath(
            WaypointsPathProvider pathProvider,
            WaypointsPathProvider.PathPoint fromPoint,
            WaypointsPathProvider.PathPoint targetPoint,
            float pathOffsetThreshold)
        {
            var segmentStart = pathProvider.Segments[fromPoint.segmentIndex].from;
            
            var projected = segmentStart + Vector3.Project(targetPoint.position - segmentStart, fromPoint.direction);
            
            var targetToProjectedLength = (targetPoint.position - projected).magnitude;
            return targetToProjectedLength > pathOffsetThreshold;
        }
    }
}