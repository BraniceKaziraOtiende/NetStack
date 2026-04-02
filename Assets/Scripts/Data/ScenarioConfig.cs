using UnityEngine;
using ArchitectureBlueprint.Infrastructure;

namespace ArchitectureBlueprint.Data
{
    
    /// ScriptableObject that defines one playable scenario.
    ///
    /// How to create an instance in Unity:
    ///   Right-click in Project → Create → NetStack → Scenario Config
    ///   Create in Assets/Data/Scenarios/
    ///
    /// Create two instances to start:
    ///   ScenarioConfig_FreeBuild      (open sandbox, no constraints)
    ///   ScenarioConfig_OverloadChallenge (start with 5000 users, fix it)
    ///
    /// LoadSimulator reads the active ScenarioConfig on Start().
   
    [CreateAssetMenu(
        fileName = "ScenarioConfig_New",
        menuName = "NetStack/Scenario Config",
        order = 2
    )]
    public class ScenarioConfig : ScriptableObject
    {
        [Header("Identity")]
        public string scenarioName = "Free Build";
        public string scenarioDescription = "Place any components and explore the architecture freely.";

        [Header("Starting State")]
        [Range(100, 10000)]
        public int startingUserCount = 500;

        [Range(0f, 1f)]
        public float userRampSpeed = 0f;    // 0 = no auto-ramp; 0.1 = slow ramp
        public bool autoRampEnabled = false;

        [Header("Pre-placed Nodes")]
        [Tooltip("These node types are already in the scene when the scenario starts")]
        public NodeType[] prePlacedNodes = new NodeType[0];

        [Header("Win Condition (optional)")]
        public bool hasWinCondition = false;
        public int targetUserCount = 5000;   // student must handle this many users
        public float maxAllowedResponseMs = 500f;   // without exceeding this response time
        [TextArea(2, 3)]
        public string winMessage = "Great work! Your architecture handles the load.";

        [Header("Guidance Toasts")]
        [Tooltip("Messages shown in sequence as the student builds. Leave empty for no guidance.")]
        public string[] guidanceMessages = new string[]
        {
            "Welcome to NetStack! Start by placing a Load Balancer.",
            "Now add at least two Servers — watch the traffic split.",
            "Add a Database to persist data.",
            "Try increasing users to 5,000. What happens?",
            "Add a Cache to reduce Database load."
        };
    }
}

// ─── SCENARIO CONFIGS TO CREATE ─────────────────────────────────────────────
//
//  ScenarioConfig_FreeBuild:
//    scenarioName        = "Free Build"
//    scenarioDescription = "Build any architecture you like. No constraints."
//    startingUserCount   = 100
//    autoRampEnabled     = false
//    prePlacedNodes      = []          (empty — student places everything)
//    hasWinCondition     = false
//    guidanceMessages    = []          (empty — no hand-holding)
//
//  ScenarioConfig_OverloadChallenge:
//    scenarioName        = "Overload Challenge"
//    scenarioDescription = "The system is under stress. Fix the architecture
//                           to handle 5,000 users with under 500ms response time."
//    startingUserCount   = 5000
//    autoRampEnabled     = false
//    prePlacedNodes      = [LoadBalancer, Server]   (one server, already overloaded)
//    hasWinCondition     = true
//    targetUserCount     = 5000
//    maxAllowedResponseMs = 500
//    winMessage          = "Excellent! The architecture is stable under load."
//    guidanceMessages    = ["One server can't handle 5,000 users. What would you add?"]