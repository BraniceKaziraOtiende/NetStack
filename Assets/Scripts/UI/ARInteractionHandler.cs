using UnityEngine;
using ArchitectureBlueprint.Infrastructure;

namespace ArchitectureBlueprint.UI
{
    
    /// Detects taps on placed 3D node prefabs and shows the InfoOverlay for that node.
    /// Works in both 3D mode (Physics.Raycast against colliders) and AR mode (same).
    ///
    /// Prerequisites:
    ///   All node prefabs must have a Collider component (Box or Mesh Collider).
    ///   Attach to the Manager GameObject in Main.unity.
    ///   Assign references in the Inspector.
    
    public class ARInteractionHandler : MonoBehaviour
    {
        [Header("References")]
        public InfoOverlayController infoOverlay;
        public Camera orbitCamera;      // the 3D sandbox camera
        public SceneModeManager sceneModeManager;

        private InfrastructureNode selectedNode;

        private void Update()
        {
            if (Input.touchCount != 1) return;
            Touch touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Began) return;
            if (IsOverUI(touch.fingerId)) return;

            HandleTap(touch.position);
        }

        private void HandleTap(Vector2 screenPos)
        {
            Camera cam = GetActiveCamera();
            if (cam == null) return;

            Ray ray = cam.ScreenPointToRay(screenPos);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                InfrastructureNode node = hit.collider.GetComponentInParent<InfrastructureNode>();

                if (node != null)
                {
                    // Tap same node again = close overlay
                    if (node == selectedNode)
                    {
                        DeselectCurrent();
                        return;
                    }

                    DeselectCurrent();
                    selectedNode = node;
                    selectedNode.Select();
                    infoOverlay?.ShowForNode(selectedNode);
                    return;
                }
            }

            // Tapped empty space — close overlay
            DeselectCurrent();
        }

        private void DeselectCurrent()
        {
            selectedNode?.Deselect();
            selectedNode = null;
            infoOverlay?.Hide();
        }

        private Camera GetActiveCamera()
        {
            bool inAR = sceneModeManager != null &&
                        sceneModeManager.CurrentMode == SceneModeManager.SceneMode.AR;
            return inAR ? Camera.main : orbitCamera;
        }

        private bool IsOverUI(int fingerId)
        {
            return UnityEngine.EventSystems.EventSystem.current != null &&
                   UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(fingerId);
        }
    }
}