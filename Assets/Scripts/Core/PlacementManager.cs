using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using ArchitectureBlueprint.Infrastructure;

namespace ArchitectureBlueprint
{
    [RequireComponent(typeof(ARRaycastManager))]
    public class PlacementManager : MonoBehaviour
    {
        [Header("Node Prefabs — 0=Client 1=LB 2=Server 3=DB 4=Cache")]
        public GameObject[] nodePrefabs;

        [Header("References")]
        public SceneModeManager sceneModeManager;
        public UI.ToolbarManager toolbarManager;
        public Transform sceneRoot;
        public ConnectionRenderer connectionRenderer;
        public Transform clientTransform;

        // Grid spread clearly away from client at (-0.5, 0.05, 0.35)
        // Client is near-left-front, nodes spread right and back
        private static readonly Vector3[] GRID = new Vector3[]
        {
            new Vector3( 0.2f,  0.2f,  0.3f),   // slot 0 — right of client
            new Vector3( 0.5f,  0.2f,  0.3f),   // slot 1
            new Vector3( 0.2f,  0.2f,  0.0f),   // slot 2 — middle row
            new Vector3( 0.5f,  0.2f,  0.0f),   // slot 3
            new Vector3(-0.1f,  0.2f,  0.0f),   // slot 4
            new Vector3( 0.2f,  0.2f, -0.3f),   // slot 5 — back row
            new Vector3( 0.5f,  0.2f, -0.3f),   // slot 6
            new Vector3(-0.1f,  0.2f, -0.3f),   // slot 7
            new Vector3( 0.8f,  0.2f,  0.0f),   // slot 8
        };

        private NodeType selectedType = NodeType.LoadBalancer;
        private int nextSlot = 0;

        private ARRaycastManager rayMgr;
        private ARAnchorManager anchorMgr;
        private List<ARRaycastHit> arHits = new List<ARRaycastHit>();

        private Dictionary<NodeType, List<InfrastructureNode>> placed
            = new Dictionary<NodeType, List<InfrastructureNode>>();

        public Dictionary<NodeType, List<InfrastructureNode>> PlacedNodes => placed;
        public System.Action<InfrastructureNode> onNodePlaced;

        private void Awake()
        {
            rayMgr = GetComponent<ARRaycastManager>();
            anchorMgr = GetComponent<ARAnchorManager>();
            foreach (NodeType t in System.Enum.GetValues(typeof(NodeType)))
                placed[t] = new List<InfrastructureNode>();
        }

        private void Update()
        {
            bool inAR = sceneModeManager != null &&
                        sceneModeManager.CurrentMode == SceneModeManager.SceneMode.AR;
            if (inAR) HandleARInput();
        }

        public void PlaceSelectedNode()
        {
            int idx = (int)selectedType;
            if (nodePrefabs == null || nodePrefabs.Length == 0)
            { Debug.LogError("[PM] nodePrefabs empty!"); return; }
            if (idx < 0 || idx >= nodePrefabs.Length || nodePrefabs[idx] == null)
            { Debug.LogError("[PM] No prefab at slot " + idx + " for " + selectedType); return; }
            if (nextSlot >= GRID.Length)
            { Debug.Log("[PM] All slots full."); return; }
            SpawnNode(GRID[nextSlot++], Quaternion.identity);
        }

        private void HandleARInput()
        {
            bool inputDown = false; Vector2 ip = Vector2.zero;
#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0)) { inputDown = true; ip = Input.mousePosition; }
#else
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            { inputDown = true; ip = Input.GetTouch(0).position; }
#endif
            if (!inputDown || IsPointerOverUI(ip)) return;
            if (rayMgr == null) return;
            if (!rayMgr.Raycast(ip, arHits, TrackableType.PlaneWithinPolygon)) return;
            Pose p = arHits[0].pose;
            var anchor = anchorMgr?.AttachAnchor((ARPlane)arHits[0].trackable, p);
            SpawnNode(p.position + Vector3.up * 0.05f, p.rotation, anchor);
        }

        private void SpawnNode(Vector3 pos, Quaternion rot, ARAnchor anchor = null)
        {
            int idx = (int)selectedType;
            GameObject go = Instantiate(nodePrefabs[idx], pos, rot);

            // Reset then apply scale so prefab internal scale doesn't interfere
            go.transform.localScale = Vector3.one;
            go.transform.localScale = ScaleFor(selectedType);

            if (anchor != null) go.transform.SetParent(anchor.transform, true);
            else if (sceneRoot != null) go.transform.SetParent(sceneRoot, true);

            // Force all renderers enabled
            foreach (var r in go.GetComponentsInChildren<Renderer>(true))
                r.enabled = true;

            InfrastructureNode node = go.GetComponent<InfrastructureNode>();
            if (node == null) node = go.GetComponentInChildren<InfrastructureNode>();
            if (node == null)
            {
                Debug.LogWarning("[PM] Adding InfrastructureNode to " + go.name);
                node = go.AddComponent<InfrastructureNode>();
            }

            node.Place(pos);
            placed[selectedType].Add(node);
            onNodePlaced?.Invoke(node);
            toolbarManager?.NotifyNodePlaced(selectedType);
            if (selectedType == NodeType.Server) AutoWire(node as ServerNode);
            ConnectLine(node);
            Debug.Log("[PM] Placed " + selectedType + " at " + pos);
        }

        private Vector3 ScaleFor(NodeType t)
        {
            switch (t)
            {
                case NodeType.LoadBalancer: return Vector3.one * 0.35f;
                case NodeType.Server: return new Vector3(0.25f, 0.40f, 0.25f);
                case NodeType.Database: return Vector3.one * 0.32f;
                case NodeType.Cache: return Vector3.one * 0.22f;
                default: return Vector3.one * 0.28f;
            }
        }

        private void ConnectLine(InfrastructureNode newNode)
        {
            if (connectionRenderer == null) return;
            switch (selectedType)
            {
                case NodeType.LoadBalancer:
                    if (clientTransform != null)
                        connectionRenderer.ConnectClientToLB(clientTransform, newNode.transform);
                    break;
                case NodeType.Server:
                    foreach (var lb in placed[NodeType.LoadBalancer])
                        if (lb != null) connectionRenderer.ConnectLBToServer(lb.transform, newNode.transform);
                    break;
                case NodeType.Database:
                    foreach (var srv in placed[NodeType.Server])
                        if (srv != null) connectionRenderer.ConnectServerToDB(srv.transform, newNode.transform);
                    break;
                case NodeType.Cache:
                    foreach (var db in placed[NodeType.Database])
                        if (db != null) connectionRenderer.ConnectCacheToDB(newNode.transform, db.transform);
                    break;
            }
        }

        private void AutoWire(ServerNode server)
        {
            if (server == null) return;
            foreach (var lb in placed[NodeType.LoadBalancer])
            {
                var lbn = lb as LoadBalancerNode;
                if (lbn != null) { lbn.RegisterServer(server); break; }
            }
        }

        public void SetSelectedNodeType(NodeType t) => selectedType = t;
        public int GetNodeCount(NodeType t) => placed.TryGetValue(t, out var l) ? l.Count : 0;
        public T GetFirstNode<T>(NodeType type) where T : InfrastructureNode
        {
            if (!placed.ContainsKey(type) || placed[type].Count == 0) return null;
            return placed[type][0] as T;
        }

        public void ClearAll()
        {
            foreach (var l in placed.Values)
            { foreach (var n in l) if (n) Destroy(n.gameObject); l.Clear(); }
            connectionRenderer?.ClearAll();
            nextSlot = 0;
        }

        private bool IsPointerOverUI(Vector2 p)
        {
            if (UnityEngine.EventSystems.EventSystem.current == null) return false;
            var pd = new UnityEngine.EventSystems.PointerEventData(
                UnityEngine.EventSystems.EventSystem.current)
            { position = p };
            var r = new List<UnityEngine.EventSystems.RaycastResult>();
            UnityEngine.EventSystems.EventSystem.current.RaycastAll(pd, r);
            return r.Count > 0;
        }
    }
}