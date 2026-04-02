using UnityEngine;

namespace ArchitectureBlueprint.Data
{
    
    /// ScriptableObject that stores all the display data for one node type.
    ///
    /// How to create an instance in Unity:
    ///   Right-click in Project → Create → NetStack → Node Data
    ///   Create one asset per node type (5 total) in Assets/Data/Nodes/
    ///
    /// Each NodeButton prefab holds a reference to its NodeData asset.
    /// InfoOverlayController reads from this to populate the popup panel.
    ///
    /// This keeps all text and icons OUT of the code — easy to edit without
    /// touching scripts, and easy to localise later.
    
    [CreateAssetMenu(
        fileName = "NodeData_New",
        menuName = "NetStack/Node Data",
        order = 1
    )]
    public class NodeData : ScriptableObject
    {
        [Header("Identity")]
        public Infrastructure.NodeType nodeType;
        public string displayName = "Node";
        public string shortLabel = "NODE";    // 4-6 chars, used on the 3D label above the prefab

        [Header("UI")]
        public Sprite icon;                         // assigned in Inspector — PNG from Assets/Art/Icons/
        public Color themeColor = Color.white;
        public Color darkTint = new Color(0.1f, 0.1f, 0.1f, 1f);

        [Header("Toolbar Button")]
        public string toolbarSubLabel = "Short role description";  // appears under name in toolbar
        public string lockedTooltip = "Place a Load Balancer first.";

        [Header("Info Overlay — Educational Content")]
        [TextArea(2, 4)]
        public string roleDescription =
            "Describe what this component does in plain language.";

        [TextArea(2, 4)]
        public string educationalTip =
            "A helpful teaching insight that appears in the info overlay.";

        [TextArea(2, 4)]
        public string overloadTip =
            "What students should do when this node is overloaded.";

        [Header("Placement Rules")]
        public bool requiresLoadBalancer = false;
        public bool requiresServer = false;
        public int maxInstances = 4;      // how many of this node type can be placed

        [Header("Simulation")]
        public float maxCapacity = 100f;
        public float baseResponseTimeMs = 100f;
    }
}

// ─── FILL IN THESE VALUES IN THE INSPECTOR FOR EACH NODE TYPE ───────────────
//
//  LoadBalancer:
//    displayName     = "Load Balancer"
//    shortLabel      = "LB"
//    themeColor      = #1D9E75
//    toolbarSubLabel = "Routes traffic"
//    roleDescription = "The Load Balancer distributes incoming requests across
//                       multiple servers. It uses round-robin routing — each
//                       new request goes to the next server in the list."
//    educationalTip  = "With only one server, all traffic hits the same machine.
//                       Add a second server and watch the load halve instantly."
//    overloadTip     = "The Load Balancer itself is stressed — this usually means
//                       too many servers are connected and routing is slow. Rare."
//
//  Server:
//    displayName     = "Server"
//    shortLabel      = "SRV"
//    themeColor      = #378ADD
//    requiresLoadBalancer = true
//    toolbarSubLabel = "Handles logic"
//    roleDescription = "Application servers run your backend code — Node.js,
//                       Python, Java. They receive requests from the load
//                       balancer, process them, and query the database."
//    educationalTip  = "Servers are stateless — any server can handle any request.
//                       That's why the load balancer can send requests to any of them."
//    overloadTip     = "This server is at capacity! Add another server — the load
//                       balancer will immediately start splitting traffic."
//
//  Database:
//    displayName     = "Database"
//    shortLabel      = "DB"
//    themeColor      = #7F77DD
//    requiresLoadBalancer = true
//    toolbarSubLabel = "Stores data"
//    roleDescription = "The database stores all persistent data — user accounts,
//                       posts, orders. Every server queries it. Because it's a
//                       single resource, it often becomes the bottleneck."
//    educationalTip  = "Notice how the database load grows even when you add more
//                       servers? More servers = more DB queries. That's why caching exists."
//    overloadTip     = "DB is overwhelmed! Add a Cache node to intercept repeated
//                       queries before they reach the database."
//
//  Cache:
//    displayName     = "Cache"
//    shortLabel      = "CACHE"
//    themeColor      = #BA7517
//    requiresServer  = true
//    toolbarSubLabel = "Speed layer"
//    roleDescription = "A cache (like Redis) stores frequently-requested data in
//                       fast memory. When a server needs data, it checks the cache
//                       first. A cache hit means no database query needed."
//    educationalTip  = "Watch the green cache-hit packets vs amber DB-query packets.
//                       The more green packets, the less load on your database."
//    overloadTip     = "Cache is under load — this is unusual. Consider increasing
//                       the cache size or distributing across multiple cache nodes."