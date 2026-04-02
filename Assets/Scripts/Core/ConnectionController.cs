using System.Collections.Generic;
using UnityEngine;
using ArchitectureBlueprint.Infrastructure;

namespace ArchitectureBlueprint
{
    /// <summary>
    /// Draws glowing LineRenderer connections between placed nodes.
    /// Class name MUST be ConnectionRenderer to match PlacementManager reference.
    /// File MUST be named ConnectionRenderer.cs
    /// </summary>
    public class ConnectionRenderer : MonoBehaviour
    {
        [Header("Prefab — empty GO with LineRenderer component")]
        public GameObject connectionLinePrefab;

        [Header("Line appearance")]
        public float baseLineWidth = 0.008f;
        public float activeLineWidth = 0.016f;
        public float pulseSpeed = 3f;

        [Header("Colours")]
        public Color clientToLBColor = new Color(0.22f, 0.54f, 0.96f, 0.6f);
        public Color lbToServerColor = new Color(0.22f, 0.54f, 0.96f, 0.5f);
        public Color serverToDBColor = new Color(0.94f, 0.62f, 0.15f, 0.5f);
        public Color serverToCacheColor = new Color(0.36f, 0.79f, 0.64f, 0.5f);
        public Color cacheToDBColor = new Color(0.94f, 0.62f, 0.15f, 0.3f);

        private class Connection
        {
            public Transform from;
            public Transform to;
            public LineRenderer line;
            public bool isActive;
            public float pulseTimer;
        }

        private List<Connection> connections = new List<Connection>();

        private void Update()
        {
            foreach (var c in connections) UpdateLine(c);
        }

        private void UpdateLine(Connection c)
        {
            if (c.from == null || c.to == null || c.line == null) return;

            c.line.SetPosition(0, c.from.position + Vector3.up * 0.05f);
            c.line.SetPosition(1, c.to.position + Vector3.up * 0.05f);

            if (c.isActive)
            {
                c.pulseTimer += Time.deltaTime * pulseSpeed;
                float t = (Mathf.Sin(c.pulseTimer) + 1f) * 0.5f;
                float w = Mathf.Lerp(baseLineWidth, activeLineWidth, t);
                c.line.startWidth = w;
                c.line.endWidth = w * 0.5f;
            }
            else
            {
                c.line.startWidth = baseLineWidth;
                c.line.endWidth = baseLineWidth * 0.5f;
            }
        }

        public int RegisterConnection(Transform from, Transform to, Color color)
        {
            if (connectionLinePrefab == null)
            {
                Debug.LogWarning("[ConnectionRenderer] connectionLinePrefab not assigned in Inspector.");
                return -1;
            }

            GameObject lineGO = Instantiate(connectionLinePrefab);
            LineRenderer lr = lineGO.GetComponent<LineRenderer>();

            if (lr == null)
            {
                Debug.LogWarning("[ConnectionRenderer] Prefab has no LineRenderer component.");
                Destroy(lineGO);
                return -1;
            }

            lr.positionCount = 2;
            lr.startWidth = baseLineWidth;
            lr.endWidth = baseLineWidth * 0.5f;
            lr.startColor = color;
            lr.endColor = new Color(color.r, color.g, color.b, color.a * 0.3f);
            lr.useWorldSpace = true;

            connections.Add(new Connection { from = from, to = to, line = lr });
            return connections.Count - 1;
        }

        public int ConnectClientToLB(Transform client, Transform lb) =>
            RegisterConnection(client, lb, clientToLBColor);
        public int ConnectLBToServer(Transform lb, Transform server) =>
            RegisterConnection(lb, server, lbToServerColor);
        public int ConnectServerToDB(Transform server, Transform db) =>
            RegisterConnection(server, db, serverToDBColor);
        public int ConnectServerToCache(Transform server, Transform cache) =>
            RegisterConnection(server, cache, serverToCacheColor);
        public int ConnectCacheToDB(Transform cache, Transform db) =>
            RegisterConnection(cache, db, cacheToDBColor);

        public void SetActive(int id, bool active)
        {
            if (id >= 0 && id < connections.Count)
                connections[id].isActive = active;
        }

        public void RemoveConnectionsFor(Transform node)
        {
            for (int i = connections.Count - 1; i >= 0; i--)
            {
                if (connections[i].from == node || connections[i].to == node)
                {
                    if (connections[i].line != null) Destroy(connections[i].line.gameObject);
                    connections.RemoveAt(i);
                }
            }
        }

        public void ClearAll()
        {
            foreach (var c in connections)
                if (c.line != null) Destroy(c.line.gameObject);
            connections.Clear();
        }
    }
}