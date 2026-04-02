using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArchitectureBlueprint.Infrastructure;

namespace ArchitectureBlueprint.Simulation
{
    /// <summary>
    /// Spawns glowing orb packets that travel ALONG the connection lines
    /// between nodes — not through the air.
    ///
    /// Flow:
    ///   Client → Load Balancer  (blue HTTP packet)
    ///   LB     → Server         (blue, routed)
    ///   Server → Database       (amber DB query)
    ///   Cache  → Server         (green cache hit, linear — no arc)
    /// </summary>
    public class PacketManager : MonoBehaviour
    {
        [Header("Packet Prefab")]
        public GameObject packetPrefab;
        public int poolSize = 40;

        [Header("Spawn Rate — driven by LoadSimulator")]
        [Range(0f, 1f)] public float CurrentSpawnRate = 0.05f;

        [Header("Colours")]
        public Color httpColour = new Color(0.22f, 0.54f, 0.96f); // blue
        public Color dbColour = new Color(0.94f, 0.62f, 0.15f); // amber
        public Color cacheColour = new Color(0.36f, 0.79f, 0.64f); // green

        [Header("Speed (world units per second)")]
        public float packetSpeed = 1.2f;

        [Header("Line offset — packets travel slightly above the line")]
        public float lineHeightOffset = 0.04f;

        [Header("References")]
        public PlacementManager placementManager;
        public Transform clientTransform;

        private Queue<GameObject> pool = new Queue<GameObject>();
        private float spawnTimer = 0f;
        private bool poolReady = false;

        private void Start()
        {
            if (packetPrefab == null)
            {
                Debug.LogError("[PacketManager] packetPrefab not assigned!");
                return;
            }
            for (int i = 0; i < poolSize; i++)
            {
                var p = Instantiate(packetPrefab);
                p.SetActive(false);
                pool.Enqueue(p);
            }
            poolReady = true;
        }

        private void Update()
        {
            if (!poolReady || placementManager == null) return;

            var nodes = placementManager.PlacedNodes;

            // Need at least LB + 1 server to spawn
            bool hasLB = nodes.ContainsKey(NodeType.LoadBalancer) &&
                         nodes[NodeType.LoadBalancer].Count > 0;
            if (!hasLB) return;

            var lb = nodes[NodeType.LoadBalancer][0] as LoadBalancerNode;
            if (lb == null || lb.GetServerCount() == 0) return;

            float interval = Mathf.Lerp(3f, 0.12f, CurrentSpawnRate);
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= interval)
            {
                spawnTimer = 0f;
                SpawnTrafficCycle(lb, nodes);
            }
        }

        public void SetSpawnRate(float rate) =>
            CurrentSpawnRate = Mathf.Clamp01(rate);

        // ── Main traffic spawning ─────────────────────────────────────────────

        private void SpawnTrafficCycle(LoadBalancerNode lb,
            Dictionary<NodeType, List<InfrastructureNode>> nodes)
        {
            bool hasServer = nodes.ContainsKey(NodeType.Server) &&
                             nodes[NodeType.Server].Count > 0;
            bool hasDB = nodes.ContainsKey(NodeType.Database) &&
                             nodes[NodeType.Database].Count > 0;
            bool hasCache = nodes.ContainsKey(NodeType.Cache) &&
                             nodes[NodeType.Cache].Count > 0;

            // 1. Client → LB  (blue, slight arc so it is visible above the table)
            Vector3 clientPos = clientTransform != null
                ? clientTransform.position
                : lb.transform.position + new Vector3(-0.6f, 0f, 0f);

            LaunchAlongLine(clientPos, lb.transform.position,
                httpColour, packetSpeed, arcHeight: 0.08f);

            if (!hasServer) return;

            // 2. LB → random Server  (blue, linear along the line)
            int sIdx = Random.Range(0, nodes[NodeType.Server].Count);
            var srv = nodes[NodeType.Server][sIdx];
            if (srv != null)
                LaunchAlongLine(lb.transform.position, srv.transform.position,
                    httpColour, packetSpeed, arcHeight: 0f);

            if (!hasDB || srv == null) return;

            // 3. Server → DB  (amber, linear)
            var db = nodes[NodeType.Database][0];
            if (db != null)
                LaunchAlongLine(srv.transform.position, db.transform.position,
                    dbColour, packetSpeed * 0.7f, arcHeight: 0f);

            // 4. Cache → Server  (green, linear — represents cache hit)
            if (hasCache && Random.value < 0.5f)
            {
                var cache = nodes[NodeType.Cache][0];
                if (cache != null)
                    LaunchAlongLine(cache.transform.position, srv.transform.position,
                        cacheColour, packetSpeed * 1.4f, arcHeight: 0f);
            }
        }

        // ── Core launch — travels strictly along the line with tiny height offset ──

        private void LaunchAlongLine(Vector3 from, Vector3 to,
                                      Color colour, float speed,
                                      float arcHeight = 0f)
        {
            if (!poolReady) return;
            var p = GetFromPool();
            if (p == null) return;

            // Raise both endpoints slightly above the line renderer
            Vector3 start = from + Vector3.up * lineHeightOffset;
            Vector3 end = to + Vector3.up * lineHeightOffset;

            p.transform.position = start;
            p.transform.localScale = Vector3.one * 0.05f;
            p.SetActive(true);

            // Set colour via MaterialPropertyBlock — no new material instances
            var r = p.GetComponent<Renderer>();
            if (r != null)
            {
                var mpb = new MaterialPropertyBlock();
                mpb.SetColor("_BaseColor", colour);
                mpb.SetColor("_EmissionColor", colour * 2.5f);
                r.SetPropertyBlock(mpb);
            }

            var trail = p.GetComponent<TrailRenderer>();
            if (trail != null)
            {
                trail.enabled = true;
                trail.Clear();
                // Match trail colour to packet
                trail.startColor = colour;
                trail.endColor = new Color(colour.r, colour.g, colour.b, 0f);
            }

            StartCoroutine(MoveAlongLine(p, start, end, speed, arcHeight));
        }

        private IEnumerator MoveAlongLine(GameObject p, Vector3 from, Vector3 to,
                                           float speed, float arcHeight)
        {
            if (p == null) yield break;

            float dist = Vector3.Distance(from, to);
            float duration = Mathf.Max(dist / speed, 0.05f);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (p == null) yield break;

                float t = elapsed / duration;

                // Linear interpolation along the line
                Vector3 pos = Vector3.Lerp(from, to, t);

                // Optional gentle arc (used for Client→LB only)
                if (arcHeight > 0f)
                    pos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;

                p.transform.position = pos;

                // Face direction of travel
                Vector3 dir = (to - from).normalized;
                if (dir != Vector3.zero)
                    p.transform.rotation = Quaternion.LookRotation(dir);

                elapsed += Time.deltaTime;
                yield return null;
            }

            ReturnToPool(p);
        }

        // ── Object pool ───────────────────────────────────────────────────────

        private GameObject GetFromPool()
        {
            if (pool.Count > 0) return pool.Dequeue();
            return packetPrefab ? Instantiate(packetPrefab) : null;
        }

        private void ReturnToPool(GameObject p)
        {
            if (p == null) return;
            var trail = p.GetComponent<TrailRenderer>();
            if (trail != null) trail.enabled = false;
            p.SetActive(false);
            pool.Enqueue(p);
        }
    }
}