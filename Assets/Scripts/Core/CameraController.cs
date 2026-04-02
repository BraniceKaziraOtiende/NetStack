using UnityEngine;

namespace ArchitectureBlueprint
{
    public class CameraController : MonoBehaviour
    {
        [Header("Target — drag VirtualTable here")]
        public Transform tableCenter;

        [Header("Orbit")]
        public float orbitSensitivity = 0.4f;
        public float minPolarAngle = 15f;
        public float maxPolarAngle = 75f;

        [Header("Zoom")]
        public float minZoomDistance = 0.8f;
        public float maxZoomDistance = 5.0f;
        public float startDistance = 2.5f;

        private float azimuth = 30f;
        private float polar = 45f;
        private float distance;
        private float lastPinchDist;
        private bool wasPinching = false;

        private void Start()
        {
            if (tableCenter == null)
            {
                var t = GameObject.Find("VirtualTable");
                if (t != null) tableCenter = t.transform;
            }
            distance = startDistance;
            UpdateCamera();
        }

        private void LateUpdate()
        {
            HandleInput();
            polar = Mathf.Clamp(polar, minPolarAngle, maxPolarAngle);
            distance = Mathf.Clamp(distance, minZoomDistance, maxZoomDistance);
            UpdateCamera();
        }

        private void HandleInput()
        {
#if UNITY_EDITOR
            if (Input.GetMouseButton(0))
            {
                azimuth -= Input.GetAxis("Mouse X") * orbitSensitivity * 3f;
                polar -= Input.GetAxis("Mouse Y") * orbitSensitivity * 2f;
            }
            distance -= Input.GetAxis("Mouse ScrollWheel") * 1.5f;
#else
            if (Input.touchCount == 1 && !wasPinching)
            {
                Touch t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Moved)
                {
                    azimuth -= t.deltaPosition.x * orbitSensitivity * 0.3f;
                    polar   -= t.deltaPosition.y * orbitSensitivity * 0.2f;
                }
            }
            else if (Input.touchCount == 2)
            {
                wasPinching = true;
                float cur = Vector2.Distance(
                    Input.GetTouch(0).position, Input.GetTouch(1).position);
                if (Input.GetTouch(0).phase == TouchPhase.Began ||
                    Input.GetTouch(1).phase == TouchPhase.Began)
                    lastPinchDist = cur;
                else
                    distance -= (cur - lastPinchDist) * 0.005f;
                lastPinchDist = cur;
            }
            else
            {
                wasPinching = false;
            }
#endif
        }

        private void UpdateCamera()
        {
            if (tableCenter == null) return;
            float az = azimuth * Mathf.Deg2Rad;
            float po = polar * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(
                distance * Mathf.Sin(po) * Mathf.Sin(az),
                distance * Mathf.Cos(po),
                distance * Mathf.Sin(po) * Mathf.Cos(az));
            transform.position = tableCenter.position + offset;
            transform.LookAt(tableCenter.position);
        }
    }
}