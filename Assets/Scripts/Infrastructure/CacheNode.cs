using UnityEngine;

namespace ArchitectureBlueprint.Infrastructure
{
   
    /// The Cache node sits between Servers and the Database.
    /// When a server sends a DB query, the cache has a chance of serving
    /// it directly (a "cache hit") — emitting a fast green packet instead
    /// of a slow amber one going all the way to the DB.

    public class CacheNode : InfrastructureNode
    {
        [Header("Cache Config")]
        [Range(0f, 1f)]
        public float hitRate = 0.6f;          // 60% of queries are served from cache
        public float decayRate = 0.2f;        // load decay per second

        [Header("Stats (read-only in Inspector)")]
        [SerializeField] private int totalRequests = 0;
        [SerializeField] private int cacheHits = 0;
        [SerializeField] private int cacheMisses = 0;

     
        /// Fired when a query is served from cache (fast path).
        /// PacketManager listens to this to spawn a green packet.
      
        public System.Action<CacheNode> onCacheHit;

   
        /// Fired when a query is NOT in cache and must go to the DB (slow path).
        /// PacketManager listens to this to spawn an amber packet toward the DB.
      
        public System.Action<CacheNode> onCacheMiss;

        // ── Public read-only stats ───────────────────────────────────────────
        public float HitRateActual =>
            totalRequests == 0 ? 0f : (float)cacheHits / totalRequests;

        protected override void Start()
        {
            base.Start();
            nodeType = NodeType.Cache;
            nodeName = "Cache";
        }

        private void Update()
        {
            if (!isPlaced) return;
            if (currentLoad > 0f)
                SetLoad(currentLoad - decayRate * Time.deltaTime);
        }


        /// Called by PacketManager when a server is about to query the database.
        /// Returns true = cache hit (serve it fast), false = cache miss (forward to DB).
  
        public bool TryServeQuery()
        {
            totalRequests++;
            SetLoad(currentLoad + 0.03f);

            bool hit = Random.value <= hitRate;
            if (hit)
            {
                cacheHits++;
                onCacheHit?.Invoke(this);
            }
            else
            {
                cacheMisses++;
                onCacheMiss?.Invoke(this);
            }
            return hit;
        }

        /// Reset stats counters — called when Clear button pressed.
        public void ResetStats()
        {
            totalRequests = 0;
            cacheHits = 0;
            cacheMisses = 0;
        }

        public override string GetDescription() =>
            $"<b>Cache</b>\n" +
            $"Hit rate: {hitRate * 100f:F0}% configured\n" +
            $"Actual hits: {HitRateActual * 100f:F0}%\n" +
            $"Requests served: {cacheHits} / {totalRequests}";

        public override string GetEducationalTip()
        {
            if (totalRequests == 0)
                return "The cache intercepts database queries. Once traffic flows, watch how many DB trips it prevents.";
            if (HitRateActual > 0.7f)
                return $"Excellent! {HitRateActual * 100f:F0}% of queries served from cache. The database load drops significantly.";
            if (HitRateActual > 0.3f)
                return "The cache is working but not all data is cached yet. In real systems, cache 'warms up' over time.";
            return "Low hit rate right now. Real Redis/Memcached caches warm up as repeated queries get stored.";
        }
    }
}