using UnityEngine;
using ArchitectureBlueprint.UI;

namespace ArchitectureBlueprint.Simulation
{
    public class LoadSimulator : MonoBehaviour
    {
        [Header("Current user count — also driven by slider")]
        [Range(1, 10000)] public int currentUserCount = 500;

        [Header("Thresholds")]
        public int warnThreshold = 2000;
        public int criticalThreshold = 5000;

        [Header("References")]
        public PlacementManager placementManager;
        public PacketManager packetManager;
        public ToastNotification toast;
        public StatsPanel statsPanel;

        private bool warnFired = false;
        private bool criticalFired = false;
        private int lastUserCount = -1;

        private void Start()
        {
            // Show initial values on the stats bar immediately
            UpdateStatsDisplay();
        }

        private void Update()
        {
            // Apply load every frame so it always reflects current state
            // This means even if slider wiring is broken, dragging in Inspector works
            if (currentUserCount != lastUserCount)
            {
                lastUserCount = currentUserCount;
                ApplyLoad();
            }

            UpdateStatsDisplay();
            CheckThresholds();
        }

        // ── Called by slider OnValueChanged ──────────────────────────────────
        public void OnSliderChanged(float value)
        {
            currentUserCount = Mathf.RoundToInt(value);
            warnFired = false;
            criticalFired = false;
            Debug.Log("[LoadSimulator] Slider → " + currentUserCount + " users");
        }

        // ── Apply load to all placed nodes ────────────────────────────────────
        private void ApplyLoad()
        {
            if (placementManager == null)
            {
                Debug.LogWarning("[LoadSimulator] placementManager not assigned!");
                return;
            }

            var nodes = placementManager.PlacedNodes;
            float norm = currentUserCount / 10000f;

            // --- Servers ---
            int serverCount = 0;
            if (nodes.ContainsKey(Infrastructure.NodeType.Server))
                serverCount = nodes[Infrastructure.NodeType.Server].Count;

            float perServer = serverCount > 0 ? norm / serverCount : norm;

            if (nodes.ContainsKey(Infrastructure.NodeType.Server))
            {
                foreach (var s in nodes[Infrastructure.NodeType.Server])
                {
                    if (s == null) continue;
                    s.SetLoad(perServer);
                    Debug.Log("[LoadSimulator] Server load set to: "
                        + (perServer * 100f).ToString("F0") + "%");
                }
            }

            // --- Load Balancer ---
            if (nodes.ContainsKey(Infrastructure.NodeType.LoadBalancer))
                foreach (var lb in nodes[Infrastructure.NodeType.LoadBalancer])
                    lb?.SetLoad(norm * 0.3f);

            // --- Database ---
            if (nodes.ContainsKey(Infrastructure.NodeType.Database))
                foreach (var db in nodes[Infrastructure.NodeType.Database])
                    db?.SetLoad(norm * 0.6f);

            // --- Cache ---
            if (nodes.ContainsKey(Infrastructure.NodeType.Cache))
                foreach (var c in nodes[Infrastructure.NodeType.Cache])
                    c?.SetLoad(norm * 0.2f);

            // --- Packet rate ---
            if (packetManager != null)
                packetManager.SetSpawnRate(norm);
        }

        // ── Update the stats bar display ──────────────────────────────────────
        private void UpdateStatsDisplay()
        {
            if (statsPanel == null) return;

            int serverCount = 0;
            float avgResponse = 0f;
            float pps = 0f;

            if (placementManager != null)
            {
                var nodes = placementManager.PlacedNodes;

                if (nodes.ContainsKey(Infrastructure.NodeType.Server))
                {
                    var servers = nodes[Infrastructure.NodeType.Server];
                    serverCount = servers.Count;
                    float total = 0f;
                    foreach (var s in servers)
                    {
                        var srv = s as Infrastructure.ServerNode;
                        if (srv != null) total += srv.CurrentResponseTimeMs;
                    }
                    if (serverCount > 0) avgResponse = total / serverCount;
                }
            }

            if (packetManager != null)
                pps = packetManager.CurrentSpawnRate * 60f;

            statsPanel.UpdateStats(currentUserCount, pps, avgResponse);
            statsPanel.UpdateServerCount(serverCount);
        }

        // ── Toast threshold checks ────────────────────────────────────────────
        private void CheckThresholds()
        {
            if (toast == null) return;

            if (!warnFired && currentUserCount >= warnThreshold)
            {
                toast.Show("Traffic growing — watch the server load!", ToastType.Warning);
                warnFired = true;
                Debug.Log("[LoadSimulator] Warning toast fired.");
            }

            if (!criticalFired && currentUserCount >= criticalThreshold)
            {
                toast.Show("System under stress! Add another server.", ToastType.Danger);
                criticalFired = true;
                Debug.Log("[LoadSimulator] Critical toast fired.");
            }
        }
    }
}