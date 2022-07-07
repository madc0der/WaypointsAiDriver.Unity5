using UnityEngine;

namespace camera
{
    public class FollowCameraController : MonoBehaviour
    {
        public Transform follower;
        public float dampSpeed = 1f;
        public Vector3 offset = new Vector3(0f, 5f, -20f);

        private Transform cameraTransform;

        private void Start()
        {
            cameraTransform = transform;
        }

        private void LateUpdate()
        {
            var followerPosition = follower.position;
            var cameraPosition = followerPosition
                           + follower.forward * offset.z
                           + follower.up * offset.y
                           + follower.right * offset.x;
            var cameraRotation = Quaternion.LookRotation((followerPosition - cameraPosition).normalized, Vector3.up);

            var deltaTime = Time.fixedDeltaTime;
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, cameraPosition, dampSpeed * deltaTime);
            cameraTransform.rotation =
                Quaternion.Slerp(cameraTransform.rotation, cameraRotation, dampSpeed * deltaTime);
        }
    }
}