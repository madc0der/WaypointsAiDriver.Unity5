using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Vector3 = UnityEngine.Vector3;

#if UNITY_EDITOR
using System;
using UnityEditor;
#endif

namespace waypoints.path
{
    [ExecuteInEditMode]
    public class WaypointsPathProvider : MonoBehaviour
    {
        private List<Segment> segments;
        private PathPoint cachedPathPoint = new PathPoint();
        private float totalDistance;

        public float Length
        {
            get
            {
                return totalDistance;
            }
        }

        public List<Segment> Segments
        {
            get
            {
                return segments;
            }
        }

        //Do not store result, it'll be overwritten on next update
        public PathPoint GetClosestPoint(Vector3 p)
        {
            var minSqrDistanceToProjection = float.MaxValue;
            Segment foundSegment = null;
            
            for (var i = 0; i < segments.Count; i++)
            {
                var segment = segments[i];
                var fromToP = p - segment.from;
                if (Vector3.Dot(segment.direction, fromToP) < 0)
                {
                    continue;
                }

                var projectedVector = Vector3.Project(fromToP, segment.direction);
                var projectedSqrMagnitude = projectedVector.sqrMagnitude;
                
                var projectedPosition = segment.from + projectedVector;

                var pToProjected = p - projectedPosition;
                var sqrDistanceToProjection = pToProjected.sqrMagnitude;
                const float lengthThreshold = 1f;
                var segmentSqrLength = segment.length * segment.length + lengthThreshold;
                if (projectedSqrMagnitude < segmentSqrLength && sqrDistanceToProjection < minSqrDistanceToProjection)
                {
                    var pathOffset = sqrDistanceToProjection;
                    if (Vector3.Cross(pToProjected, segment.direction).y < 0f)
                    {
                        pathOffset = -pathOffset;
                    }
                    foundSegment = segment;
                    cachedPathPoint.Set(i, projectedPosition, -1f, pathOffset, 
                        segment.normal, segment.direction, segment.right, 
                        segment.endSpeedLimit,
                        -1f);
                    
                    minSqrDistanceToProjection = sqrDistanceToProjection;
                }
            }

            if (foundSegment != null)
            {
                var segmentDistance = (cachedPathPoint.position - foundSegment.from).magnitude;
                if (segmentDistance > foundSegment.length)
                {
                    segmentDistance = foundSegment.length;
                    cachedPathPoint.position = foundSegment.to;
                }
                cachedPathPoint.signedPathOffset = cachedPathPoint.signedPathOffset < 0
                    ? -Mathf.Sqrt(-cachedPathPoint.signedPathOffset)
                    : Mathf.Sqrt(cachedPathPoint.signedPathOffset);
                cachedPathPoint.pathDistance = foundSegment.initDistance + segmentDistance;
                cachedPathPoint.distanceToEndOfSegment = foundSegment.length - segmentDistance;
                return cachedPathPoint;
            }

            return FindPathPointByClosestWayPoint(p);
        }

        //Do not store result, it'll be overwritten on next update
        public PathPoint GetClosestPoint(float distance)
        {
            Assert.IsTrue(distance >= 0f, "Distance must be gte 0");
            var clampedDistance = CircularPathHelper.GetClampedDistance(Length, distance);
            
            var found = false;
            for (var i = 0; i < segments.Count; i++)
            {
                var segment = segments[i];
                var initDistance = segment.initDistance;
                if (clampedDistance >= initDistance && clampedDistance < initDistance + segment.length)
                {
                    var segmentDistance = clampedDistance - initDistance;
                    var p = segment.from + segment.direction * segmentDistance;
                    cachedPathPoint.Set(i, p, clampedDistance, 0f, segment.normal, segment.direction, segment.right,
                        segment.endSpeedLimit,
                        segment.length - segmentDistance);
                    found = true;
                }
            }

            return found ? cachedPathPoint : null;
        }

        private void OnEnable()
        {
            var waypoints = new List<Vector3>();
            var limiters = new Dictionary<int, WaypointSpeedLimiter>(50);
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                waypoints.Add(child.position);
                var limiter = child.GetComponent<WaypointSpeedLimiter>();
                limiters.Add(i, limiter);
            }
            BuildSegments(waypoints, limiters);
        }

        private void OnDrawGizmos()
        {
            for (var i = 0; i < segments.Count; i++)
            {
                Gizmos.color = Color.green;
                var segment = segments[i];
                Gizmos.DrawLine(segment.from, segment.to);
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(segment.to, 0.025f);
            
#if UNITY_EDITOR
                var msg = String.Format("{0} initDst={1:#.00}, len={2:#.00}", i + 1, segment.initDistance,
                    segment.length);
                Handles.Label(segment.from, msg);
#endif
            }
        }

        private void BuildSegments(List<Vector3> waypoints, Dictionary<int, WaypointSpeedLimiter> limiters)
        {
            segments = new List<Segment>();
            totalDistance = 0f;
            for (var i = 0; i < waypoints.Count; i++)
            {
                var p0 = i == 0 ? waypoints[waypoints.Count - 1] : waypoints[i - 1];
                var p1 = waypoints[i];

                var direction = (p1 - p0).normalized;
                var right = Vector3.Cross(Vector3.up, direction).normalized;
                var normal = Vector3.Cross(direction, right).normalized;

                var limiter = limiters.ContainsKey(i)
                    ? limiters[i]
                    : null;
                
                segments.Add(new Segment(
                    p0, p1,
                    totalDistance,
                    (p1 - p0).magnitude, 
                    direction, 
                    normal, 
                    right,
                    limiter
                ));
                totalDistance += (p1 - p0).magnitude;
            }
        }

        private float GetSignedPathOffset(Vector3 positionToPathPoint, Vector3 pathDirection, float sqrDistanceToProjection)
        {
            var pathOffset = sqrDistanceToProjection;
            if (Vector3.Cross(positionToPathPoint, pathDirection).y < 0f)
            {
                return -pathOffset;
            }
            return pathOffset;
        }

        private PathPoint FindPathPointByClosestWayPoint(Vector3 p)
        {
            int foundSegmentIndex = -1;
            var minSqrDistance = float.MaxValue;
            for (var i = 0; i < segments.Count; i++)
            {
                var segment = segments[i];
                var positionToPathPoint = p - segment.from;
                var sqrDistance = positionToPathPoint.sqrMagnitude;
                if (sqrDistance < minSqrDistance)
                {
                    var signedPathOffset = GetSignedPathOffset(positionToPathPoint, segment.direction, sqrDistance);
                    cachedPathPoint.signedPathOffset = signedPathOffset;
                    
                    minSqrDistance = sqrDistance;
                    foundSegmentIndex = i;
                }
            }
            
            Assert.IsTrue(foundSegmentIndex >= 0, "Can't find closest segment event by waypoints");

            var foundSegment = segments[foundSegmentIndex];
            cachedPathPoint.Set(foundSegmentIndex, foundSegment.from, foundSegment.initDistance, 
                cachedPathPoint.signedPathOffset, foundSegment.normal, foundSegment.direction, foundSegment.right,
                foundSegment.endSpeedLimit,
                foundSegment.length);
            
            return cachedPathPoint;
        }
        
        public class Segment
        {
            public Vector3 from;
            public Vector3 to;
            public float initDistance;
            public float length;
            public Vector3 direction;
            public Vector3 normal;
            public Vector3 right;
            public WaypointSpeedLimiter endSpeedLimit;

            public Segment(
                Vector3 from, 
                Vector3 to,
                float initDistance,
                float length,
                Vector3 direction, 
                Vector3 normal, 
                Vector3 right,
                WaypointSpeedLimiter endSpeedLimit)
            {
                this.from = from;
                this.to = to;
                this.initDistance = initDistance;
                this.length = length;
                this.direction = direction;
                this.normal = normal;
                this.right = right;
                this.endSpeedLimit = endSpeedLimit;
            }
        }

        public class PathPoint
        {
            public int segmentIndex;
            public Vector3 position;
            public float pathDistance;
            public float signedPathOffset;
            public Vector3 normal;
            public Vector3 direction;
            public Vector3 right;
            public WaypointSpeedLimiter endSpeedLimit;
            public float distanceToEndOfSegment;

            public void Set(int segmentIndex, 
                Vector3 position,
                float pathDistance,
                float signedPathOffset,
                Vector3 normal, 
                Vector3 direction, 
                Vector3 right,
                WaypointSpeedLimiter endSpeedLimit,
                float distanceToEndOfSegment)
            {
                this.segmentIndex = segmentIndex;
                this.position = position;
                this.pathDistance = pathDistance;
                this.signedPathOffset = signedPathOffset;
                this.normal = normal;
                this.direction = direction;
                this.right = right;
                this.endSpeedLimit = endSpeedLimit;
                this.distanceToEndOfSegment = distanceToEndOfSegment;
            }

            public void CopyFrom(PathPoint src)
            {
                Set(src.segmentIndex, src.position, src.pathDistance, src.signedPathOffset, 
                    src.normal, src.direction, src.right, src.endSpeedLimit, src.distanceToEndOfSegment);
            }
        }
    }
}
