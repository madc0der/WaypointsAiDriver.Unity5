using UnityEngine;

namespace waypoints.path
{
    public static class CircularPathHelper
    {
        public static float GetClampedDistance(float totalDistance, float distance)
        {
            return distance - totalDistance * Mathf.Floor(distance / totalDistance);            
        }

        public static float GetPathDelta(float totalDistance, float from, float to)
        {
            var clampedFrom = GetClampedDistance(totalDistance, from);
            var clampedTo = GetClampedDistance(totalDistance, to);
            if (clampedTo > clampedFrom)
            {
                return clampedTo - clampedFrom;
            }
            return clampedTo + totalDistance - clampedFrom;
        }
    }
}