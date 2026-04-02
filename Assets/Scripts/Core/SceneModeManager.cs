using UnityEngine;
using UnityEngine.XR.ARFoundation;
using ArchitectureBlueprint.UI;

namespace ArchitectureBlueprint
{
    public class SceneModeManager : MonoBehaviour
    {
        public enum SceneMode { Mode3D, AR }

        [Header("Cameras")]
        public Camera orbitCamera;
        public Camera arCamera;

        [Header("AR")]
        public ARSession arSession;
        public ARSessionOrigin arSessionOrigin;

        [Header("UI")]
        public ToastNotification toast;
        public GameObject trackingLostBanner;

        // The AR toggle button label — so we can update its text
        [Header("AR Button label (TMP on the View in AR button)")]
        public TMPro.TextMeshProUGUI arButtonLabel;

        [Header("Scene")]
        public Transform sceneRoot;
        public Transform tableTransform;

        private SceneMode currentMode = SceneMode.Mode3D;

        private void Start()
        {
            if (orbitCamera == null) orbitCamera = Camera.main;
            ActivateMode3D();
        }

        private void Update()
        {
            if (currentMode == SceneMode.AR) MonitorTracking();
        }

        public SceneMode CurrentMode => currentMode;

        public void SetMode(SceneMode mode)
        {
            currentMode = mode;
            if (mode == SceneMode.Mode3D) ActivateMode3D();
            else ActivateModeAR();
        }

        // Called directly by the View in AR button OnClick
        public void ToggleARMode()
        {
            if (currentMode == SceneMode.Mode3D)
            {
                SetMode(SceneMode.AR);
                if (arButtonLabel != null) arButtonLabel.text = "Exit AR";
            }
            else
            {
                SetMode(SceneMode.Mode3D);
                if (arButtonLabel != null) arButtonLabel.text = "View in AR";
            }
        }

        private void ActivateMode3D()
        {
            if (orbitCamera == null) orbitCamera = Camera.main;
            if (orbitCamera != null) orbitCamera.gameObject.SetActive(true);
            if (arCamera != null) arCamera.gameObject.SetActive(false);
            if (arSession != null) arSession.enabled = false;
            if (trackingLostBanner != null) trackingLostBanner.SetActive(false);

            if (sceneRoot != null && tableTransform != null)
            {
                sceneRoot.position = tableTransform.position;
                sceneRoot.rotation = tableTransform.rotation;
            }

            Debug.Log("[SceneModeManager] 3D mode active.");
        }

        private void ActivateModeAR()
        {
            if (orbitCamera != null) orbitCamera.gameObject.SetActive(false);
            if (arCamera != null) arCamera.gameObject.SetActive(true);
            if (arSession != null) arSession.enabled = true;

            Debug.Log("[SceneModeManager] AR mode active.");
            toast?.Show("Point camera at a flat surface.", ToastType.Info);
        }

        private void MonitorTracking()
        {
            bool good = ARSession.state == ARSessionState.SessionTracking;
            if (trackingLostBanner != null) trackingLostBanner.SetActive(!good);
        }
    }
}