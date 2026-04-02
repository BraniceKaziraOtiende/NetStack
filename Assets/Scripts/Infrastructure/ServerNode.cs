using UnityEngine;

namespace ArchitectureBlueprint.Infrastructure
{
    public class ServerNode : InfrastructureNode
    {
        [Header("Server Config")]
        public float requestDecayRate = 0.05f;  // very slow decay so colour stays visible
        public float requestLoadIncrement = 0.08f;

        [Header("Response Time")]
        public float baseResponseTimeMs = 50f;
        public float maxResponseTimeMs = 2000f;

        public float CurrentResponseTimeMs =>
            Mathf.Lerp(baseResponseTimeMs, maxResponseTimeMs, currentLoad);

        public System.Action<ServerNode> onRequestHandled;

        // Target load set by LoadSimulator — server smoothly moves toward it
        private float targetLoad = 0f;

        protected override void Start()
        {
            base.Start();
            nodeType = NodeType.Server;
        }

        protected override void Update()
        {
            if (!isPlaced) return;

            // Smoothly move current load toward the target set by LoadSimulator
            // This gives a nice animated transition instead of instant jump
            float smoothed = Mathf.MoveTowards(
                currentLoad, targetLoad, requestDecayRate * Time.deltaTime);

            if (Mathf.Abs(smoothed - currentLoad) > 0.001f)
            {
                currentLoad = smoothed;
                UpdateVisuals();
            }
        }

        // Called by LoadSimulator — sets the TARGET, not current load directly
        public override void SetLoad(float load)
        {
            targetLoad = Mathf.Clamp01(load);

            // Also fire the events immediately
            onLoadChanged.Invoke(targetLoad);

            bool over = targetLoad >= 1f;
            if (over && !wasOverloaded) { onOverload.Invoke(); wasOverloaded = true; }
            if (!over && wasOverloaded) { onRecovered.Invoke(); wasOverloaded = false; }
        }

        public void ReceiveRequest()
        {
            targetLoad = Mathf.Clamp01(targetLoad + requestLoadIncrement);
            onRequestHandled?.Invoke(this);
        }

        public override string GetDescription() =>
            nodeName + "\nLoad: " + (currentLoad * 100f).ToString("F0") + "%" +
            "\nResponse: " + CurrentResponseTimeMs.ToString("F0") + "ms";

        public override string GetEducationalTip() =>
            currentLoad > 0.85f
                ? "Near capacity! Add another server to share the load."
                : "Servers handle application logic. Your code runs here.";
    }
}