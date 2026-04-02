using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ArchitectureBlueprint.UI
{
    /// <summary>
    /// Orchestrates the entire bottom toolbar:
    ///   - Manages which NodeButton is currently selected
    ///   - Enforces prerequisite rules (e.g. Cache requires a Server)
    ///   - Drives the user-count slider and forwards value to LoadSimulator
    ///   - Handles the "View in AR" toggle button
    ///   - Communicates selected node type to PlacementManager
    ///
    /// Attach to the ToolbarRoot GameObject in your Canvas.
    /// </summary>
    public class ToolbarManager : MonoBehaviour
    {
        [Header("Node Buttons — assign in Inspector (order matters)")]
        public List<NodeButtonController> nodeButtons;

        [Header("User Count Slider")]
        public Slider userSlider;
        public TextMeshProUGUI userCountLabel;
        public Simulation.LoadSimulator loadSimulator;

        [Header("AR Toggle Button")]
        public Button arToggleButton;
        public TextMeshProUGUI arButtonLabel;
        public SceneModeManager sceneModeManager;

        [Header("Clear Button — separate from node buttons")]
        public Button clearButton;

        [Header("Placement")]
        public PlacementManager placementManager;

        // ── Prerequisite rules ───────────────────────────────────────────────
        // Key = NodeType that is LOCKED, Value = NodeType that must exist first
        private Dictionary<Infrastructure.NodeType, Infrastructure.NodeType> prerequisites
            = new Dictionary<Infrastructure.NodeType, Infrastructure.NodeType>
        {
            { Infrastructure.NodeType.Cache,    Infrastructure.NodeType.Server      },
            { Infrastructure.NodeType.Database, Infrastructure.NodeType.LoadBalancer },
        };

        // Track which node types have been placed so we can unlock buttons
        private HashSet<Infrastructure.NodeType> placedTypes = new HashSet<Infrastructure.NodeType>();

        private NodeButtonController activeButton;

        // ────────────────────────────────────────────────────────────────────
        private void Start()
        {
            // Wire up each button
            foreach (var btn in nodeButtons)
            {
                btn.onClicked += OnNodeButtonClicked;
                ApplyLockState(btn);
            }

            // Slider
            if (userSlider != null)
                userSlider.onValueChanged.AddListener(OnSliderChanged);

            // AR toggle
            if (arToggleButton != null)
                arToggleButton.onClick.AddListener(ToggleARMode);

            // Clear button
            if (clearButton != null)
                clearButton.onClick.AddListener(OnClearClicked);

            // Default: select first button that isn't locked
            SelectFirstAvailable();
        }

        // ── Button selection ─────────────────────────────────────────────────

        private void OnNodeButtonClicked(NodeButtonController clicked)
        {
            // Deselect previous
            if (activeButton != null && activeButton != clicked)
                activeButton.SetActive(false);

            activeButton = clicked;
            clicked.SetActive(true);

            // Tell placement manager which node to spawn next
            placementManager?.SetSelectedNodeType(clicked.nodeType);
        }

        private void SelectFirstAvailable()
        {
            foreach (var btn in nodeButtons)
            {
                if (!btn.IsLocked)
                {
                    OnNodeButtonClicked(btn);
                    return;
                }
            }
        }

        // ── Prerequisite enforcement ─────────────────────────────────────────

        /// <summary>
        /// Called by PlacementManager after a node is successfully placed.
        /// Unlocks any buttons whose prerequisite has now been satisfied.
        /// </summary>
        public void NotifyNodePlaced(Infrastructure.NodeType type)
        {
            placedTypes.Add(type);
            RefreshAllLockStates();
        }

        /// <summary>Called when a node is removed from the scene.</summary>
        public void NotifyNodeRemoved(Infrastructure.NodeType type)
        {
            // Only remove from set if no remaining nodes of that type exist
            // (PlacementManager tracks the count)
            if (placementManager != null && placementManager.GetNodeCount(type) == 0)
            {
                placedTypes.Remove(type);
                RefreshAllLockStates();
            }
        }

        private void RefreshAllLockStates()
        {
            foreach (var btn in nodeButtons)
                ApplyLockState(btn);
        }

        private void ApplyLockState(NodeButtonController btn)
        {
            if (prerequisites.TryGetValue(btn.nodeType, out var required))
            {
                bool prerequisiteMet = placedTypes.Contains(required);
                btn.SetLocked(!prerequisiteMet);

                // Set a clear tooltip explaining what's needed
                if (!prerequisiteMet)
                    btn.lockedTooltip = $"Place a {required} first to unlock {btn.displayName}.";
            }
        }

        // ── Slider ───────────────────────────────────────────────────────────

        public void OnSliderChanged(float value)
        {
            int userCount = Mathf.RoundToInt(value);
            if (userCountLabel != null)
                userCountLabel.text = userCount.ToString("N0");

            loadSimulator?.OnSliderChanged(value);
        }

        // ── AR toggle ────────────────────────────────────────────────────────

        private bool arModeActive = false;

        private void ToggleARMode()
        {
            arModeActive = !arModeActive;
            sceneModeManager?.SetMode(
                arModeActive ? SceneModeManager.SceneMode.AR : SceneModeManager.SceneMode.Mode3D
            );
            if (arButtonLabel != null)
                arButtonLabel.text = arModeActive ? "Exit AR" : "View in AR";
        }

        // ── Clear ────────────────────────────────────────────────────────────

        public void OnClearClicked()
        {
            placementManager?.ClearAll();
            placedTypes.Clear();
            RefreshAllLockStates();
            SelectFirstAvailable();
        }
    }
}