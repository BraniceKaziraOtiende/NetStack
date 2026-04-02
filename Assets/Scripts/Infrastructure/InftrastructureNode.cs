using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace ArchitectureBlueprint.Infrastructure
{
    public enum NodeType { Client, LoadBalancer, Server, Database, Cache }

    public class InfrastructureNode : MonoBehaviour
    {
        [Header("Identity")]
        public NodeType nodeType;
        public string nodeName = "Node";

        [Header("Load — watch this in Play mode to confirm SetLoad is working")]
        [Range(0f, 1f)] public float currentLoad = 0f;

        [Header("State")]
        public bool isOnline = true;
        public bool isPlaced = false;

        [Header("Visual — drag Body child Mesh Renderer here")]
        public Renderer nodeRenderer;

        [Header("Materials — drag your 3 materials here if you have them")]
        public Material healthyMaterial;
        public Material warningMaterial;
        public Material overloadMaterial;

        [Header("Optional")]
        public TextMeshPro labelText;
        public GameObject selectionRing;

        public UnityEvent onNodePlaced = new UnityEvent();
        public UnityEvent<float> onLoadChanged = new UnityEvent<float>();
        public UnityEvent onOverload = new UnityEvent();
        public UnityEvent onRecovered = new UnityEvent();

        protected bool wasOverloaded = false;
        private float lastLoad = -1f;

        // Teal → Amber → Red in HDR values for emission
        private static readonly Color COL_HEALTHY = new Color(0.11f, 0.62f, 0.46f);
        private static readonly Color COL_WARNING = new Color(0.94f, 0.62f, 0.15f);
        private static readonly Color COL_OVERLOAD = new Color(0.88f, 0.29f, 0.29f);

        protected virtual void Start()
        {
            if (labelText != null) labelText.text = nodeName;
            if (selectionRing != null) selectionRing.SetActive(false);

            // Auto-find renderer if not assigned in Inspector
            if (nodeRenderer == null)
                nodeRenderer = GetComponentInChildren<Renderer>();

            if (nodeRenderer == null)
                Debug.LogWarning("[Node] " + gameObject.name
                    + " has no Renderer — colours will not work. "
                    + "Assign the Body child Mesh Renderer to nodeRenderer in Inspector.");

            // Enable emission so colour changes are visible in URP
            if (nodeRenderer != null)
                foreach (var mat in nodeRenderer.materials)
                    mat.EnableKeyword("_EMISSION");

            UpdateVisuals();
        }

        // Update runs every frame — guarantees colour is always correct
        // even if SetLoad() was called before the renderer was ready
        protected virtual void Update()
        {
            if (Mathf.Abs(currentLoad - lastLoad) > 0.001f)
            {
                lastLoad = currentLoad;
                UpdateVisuals();
            }
        }

        public virtual void Place(Vector3 worldPosition)
        {
            transform.position = worldPosition;
            isPlaced = true;
            onNodePlaced.Invoke();
            UpdateVisuals();
        }

        public virtual void SetLoad(float load)
        {
            currentLoad = Mathf.Clamp01(load);
            onLoadChanged.Invoke(currentLoad);

            bool over = currentLoad >= 1f;
            if (over && !wasOverloaded) { onOverload.Invoke(); wasOverloaded = true; }
            if (!over && wasOverloaded) { onRecovered.Invoke(); wasOverloaded = false; }

            UpdateVisuals();
        }

        public virtual void Select()
        {
            if (selectionRing != null) selectionRing.SetActive(true);
        }

        public virtual void Deselect()
        {
            if (selectionRing != null) selectionRing.SetActive(false);
        }

        protected virtual void UpdateVisuals()
        {
            // Find renderer if still not assigned
            if (nodeRenderer == null)
                nodeRenderer = GetComponentInChildren<Renderer>();
            if (nodeRenderer == null) return;

            // --- Method 1: swap whole material if all 3 are assigned ---
            if (healthyMaterial != null &&
                warningMaterial != null &&
                overloadMaterial != null)
            {
                Material target = currentLoad < 0.6f ? healthyMaterial
                                : currentLoad < 0.85f ? warningMaterial
                                : overloadMaterial;

                // Only swap if different to avoid material thrashing
                if (nodeRenderer.sharedMaterial != target)
                    nodeRenderer.material = target;
                return;
            }

            // --- Method 2: change emission colour on existing material ---
            // Works even with NO separate materials assigned
            Color col = currentLoad < 0.6f ? COL_HEALTHY
                      : currentLoad < 0.85f ? COL_WARNING
                      : COL_OVERLOAD;

            // Enable emission keyword — required in URP for emission to show
            nodeRenderer.material.EnableKeyword("_EMISSION");

            // Use MaterialPropertyBlock — does NOT create new material instances
            var mpb = new MaterialPropertyBlock();
            nodeRenderer.GetPropertyBlock(mpb);
            mpb.SetColor("_EmissionColor", col * 2.5f);
            mpb.SetColor("_BaseColor", col * 0.55f);
            nodeRenderer.SetPropertyBlock(mpb);
        }

        public virtual string GetDescription() =>
            nodeName + "\nLoad: " + (currentLoad * 100f).ToString("F0") + "%";

        public virtual string GetEducationalTip() =>
            "Tap any component to learn its role in a real web architecture.";
    }
}