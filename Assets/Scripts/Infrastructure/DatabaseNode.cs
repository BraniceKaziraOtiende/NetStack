using UnityEngine;

namespace ArchitectureBlueprint.Infrastructure
{
    
    /// Simulates a database. All servers eventually query the DB,
    /// so it becomes the bottleneck at high load — a key teaching insight.
    /// Packets travelling to the DB are visually larger and slower.

    public class DatabaseNode : InfrastructureNode
    {
        [Header("Database Config")]
        public float baseQueryTimeMs = 120f;
        public float maxQueryTimeMs = 5000f;
        public float queryLoadIncrement = 0.04f;
        public float queryDecayRate = 0.3f;

        public float CurrentQueryTimeMs =>
            Mathf.Lerp(baseQueryTimeMs, maxQueryTimeMs, currentLoad);

        public System.Action<DatabaseNode> onQueryReceived;

        protected override void Start()
        {
            base.Start();
            nodeType = NodeType.Database;
        }

        private void Update()
        {
            if (!isPlaced) return;
            if (currentLoad > 0f)
                SetLoad(currentLoad - queryDecayRate * Time.deltaTime);
        }

        public void ReceiveQuery()
        {
            SetLoad(currentLoad + queryLoadIncrement);
            onQueryReceived?.Invoke(this);
        }

        public override string GetDescription() =>
            $"<b>Database</b>\nLoad: {currentLoad * 100f:F0}%\n" +
            $"Query time: {CurrentQueryTimeMs:F0}ms\n" +
            $"Status: {(isOnline ? "Online" : "Offline")}";

        public override string GetEducationalTip() =>
            currentLoad > 0.7f
                ? "The database is becoming a bottleneck. In real systems, a cache (Redis) sits in front of the DB to reduce this load."
                : "Every server sends queries here. The DB is often the slowest part of a web system — that's why caching exists.";
    }
}