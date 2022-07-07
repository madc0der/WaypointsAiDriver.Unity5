using UnityEngine;
using UnityStandardAssets.Vehicles.Car;
using waypoints.common;
using waypoints.path;
#if UNITY_EDITOR
using System;
using UnityEditor;
#endif

namespace waypoints.driver
{
    [ExecuteInEditMode]
    public class WaypointsAiDriver : MonoBehaviour
    {
        public float maxAheadDistance = 100f;
        public float minAheadDistance = 15f;
        public float maxAheadPathOffset = 5f;
        public float steeringAngleLimit = 45f;
        public WaypointsPathProvider pathProvider;
        public float smoothSteeringSpeed = 15f;

        public bool useOutOfPathSteeringFix;
        public AnimationCurve pathOffsetSteeringFix = new AnimationCurve(new[]
        {
            new Keyframe(0, 0),
            new Keyframe(10f, 10f)
        });

        public AnimationCurve speedByVisiblity = new AnimationCurve(new[]
        {
            new Keyframe(0, 20f),
            new Keyframe(20f, 200f)
        });
        
        public KeyCode pauseKey = KeyCode.P;
        public KeyCode resetKey = KeyCode.R;
        private bool isPaused;

        private WaypointsPathProvider.PathPoint currentPathPoint = new WaypointsPathProvider.PathPoint();
        private WaypointsPathProvider.PathPoint farPathPoint = new WaypointsPathProvider.PathPoint();
        private Transform driverTransform;

        private float editorLastUpdateTime;

        private AnimationCurve steeringToAheadByDistance;
        
        private float currentSteeringAngle;
        private float previousSteeringAngle;
        private CarController carController;
        private float currentGas;
        private float currentBrake;

        private Vector3 initPosition;
        private Quaternion initRotation;

        private void OnEnable()
        {
            driverTransform = transform;
        }

        private void Start()
        {
            WaypointsAiHelper.Init(pathProvider, maxAheadDistance, minAheadDistance, maxAheadPathOffset);
            
            driverTransform = transform;
            UpdateCurrentPathPoints();

            initPosition = driverTransform.position = currentPathPoint.position;
            initRotation = driverTransform.rotation = Quaternion.LookRotation(currentPathPoint.direction, currentPathPoint.normal);

            carController = GetComponent<CarController>();
        }

        #if UNITY_EDITOR
        private void Update()
        {
            if (!IsInEditor())
            {
                return;
            }

            var currentTime = Time.time;
            const float editorUpdateDeltaTime = 0.3f;
            if (currentTime - editorLastUpdateTime > editorUpdateDeltaTime)
            {
                CalculateMovements(editorUpdateDeltaTime);
                editorLastUpdateTime = currentTime;
            }
        }
        #endif

        private bool IsInEditor()
        {
            return Application.isEditor && !Application.isPlaying;
        }

        private void FixedUpdate()
        {
            if (!isPaused)
            {
                ApplyControlsToCar();
            }

            if (DebounceInput.GetKey(pauseKey))
            {
                isPaused = !isPaused;
            }

            if (DebounceInput.GetKey(resetKey))
            {
                driverTransform.position = initPosition;
                driverTransform.rotation = initRotation;
                //It's also needed to reset rigidbody inertia, but don't want to expose std car rigidbody
            }

            CalculateMovements(Time.fixedDeltaTime);
        }

        //In real game it'll be moved by WheelColliders and other physics
        private void ApplyControlsToCar()
        {
            if (IsInEditor())
            {
                return;
            }
            
            var steeringNormalized = currentSteeringAngle / steeringAngleLimit;
            var handBrake = 0f;
            carController.Move(steeringNormalized, currentGas, -currentBrake, handBrake);
        }

        
        private void CalculateMovements(float deltaTime)
        {
            if (IsInEditor())
            {
                return;
            }
            
            var scanDistance = UpdateCurrentPathPoints();
            
            var moveDirection = (farPathPoint.position - driverTransform.position).normalized;
            var steeringAngle = SignedAngle(driverTransform.forward, moveDirection, Vector3.up);

            if (useOutOfPathSteeringFix)
            {
                var pathOffsetFix = pathOffsetSteeringFix.Evaluate(Mathf.Abs(currentPathPoint.signedPathOffset));
                if (currentPathPoint.signedPathOffset < 0f)
                {
                    pathOffsetFix = -pathOffsetFix;
                }
                steeringAngle += pathOffsetFix;
            }
            steeringAngle = Mathf.Clamp(steeringAngle, -steeringAngleLimit, steeringAngleLimit);

            var hardSteerFactor = Mathf.Clamp01(Mathf.Abs(steeringAngle / steeringAngleLimit));
            steeringAngle *= hardSteerFactor;

            currentSteeringAngle = Mathf.Lerp(previousSteeringAngle, steeringAngle, smoothSteeringSpeed * deltaTime);
            previousSteeringAngle = currentSteeringAngle;

            var recommendedSpeed = speedByVisiblity.Evaluate(scanDistance);
            var currentSpeed = carController.CurrentSpeed;
            var delta = recommendedSpeed - currentSpeed;
            //It's better to use smooth control for gas/brake to avoid extra slipping
            if (delta > 0f)
            {
                currentGas = 1f;
                currentBrake = 0f;
            } 
            else
            {
                currentGas = 0f;
                currentBrake = 1f;
            }
        }

        //Copied from latest Unity Vector3 impl
        private float SignedAngle(Vector3 from, Vector3 to, Vector3 axis)
        {
            float unsignedAngle = Vector3.Angle(from, to);

            float cross_x = from.y * to.z - from.z * to.y;
            float cross_y = from.z * to.x - from.x * to.z;
            float cross_z = from.x * to.y - from.y * to.x;
            float sign = Mathf.Sign(axis.x * cross_x + axis.y * cross_y + axis.z * cross_z);
            return unsignedAngle * sign;
        }

        private float UpdateCurrentPathPoints()
        {
            currentPathPoint.CopyFrom(pathProvider.GetClosestPoint(driverTransform.position));

            var scanDistance = WaypointsAiHelper.GetScanDistance(currentPathPoint.pathDistance, maxAheadDistance);
            
            farPathPoint.CopyFrom(pathProvider.GetClosestPoint(currentPathPoint.pathDistance + scanDistance));

            return scanDistance;
        }

        private void OnDrawGizmos()
        {
            if (!pathProvider)
            {
                return;
            }
            
            DrawLocalSpace(driverTransform.position, 
                driverTransform.forward,
                driverTransform.up,
                driverTransform.right);
            
            DrawPathPointGizmo(currentPathPoint);
            DrawPathPointGizmo(farPathPoint);
        }

        private void DrawPathPointGizmo(WaypointsPathProvider.PathPoint pathPoint)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(pathPoint.position, 0.5f);
            
            DrawLocalSpace(pathPoint.position, pathPoint.direction, pathPoint.normal, pathPoint.right);
            
#if UNITY_EDITOR
            var msg = String.Format("Seg={0}, pathDst={1:#.00}", pathPoint.segmentIndex + 1, pathPoint.pathDistance);
            Handles.Label(pathPoint.position, msg);
#endif
        }

        private void DrawLocalSpace(Vector3 position, Vector3 forward, Vector3 up, Vector3 right)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(position, position + right);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(position, position + up);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(position, position + forward);
        }
    }
}