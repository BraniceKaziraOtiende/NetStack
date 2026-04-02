using System.Collections.Generic;
using UnityEngine;

namespace ArchitectureBlueprint.Infrastructure
{
    public class LoadBalancerNode : InfrastructureNode
    {
        private List<ServerNode> connectedServers = new List<ServerNode>();
        private int roundRobinIndex = 0;

        public System.Action<ServerNode> onPacketRouted;

        protected override void Start()
        {
            base.Start();
            nodeType = NodeType.LoadBalancer;
        }

        // No Update override — load is set directly by LoadSimulator
        // and stays until changed

        public void RegisterServer(ServerNode server)
        {
            if (server != null && !connectedServers.Contains(server))
            {
                connectedServers.Add(server);
                Debug.Log("[LoadBalancer] Server registered. Total: " + connectedServers.Count);
            }
        }

        public void UnregisterServer(ServerNode server)
        {
            connectedServers.Remove(server);
        }

        public ServerNode RouteNextPacket()
        {
            if (connectedServers.Count == 0) return null;
            roundRobinIndex = (roundRobinIndex + 1) % connectedServers.Count;
            var server = connectedServers[roundRobinIndex];
            onPacketRouted?.Invoke(server);
            return server;
        }

        public int GetServerCount() => connectedServers.Count;

        public override string GetDescription() =>
            nodeName + "\nServers: " + connectedServers.Count +
            "\nLoad: " + (currentLoad * 100f).ToString("F0") + "%";

        public override string GetEducationalTip() =>
            "The load balancer distributes requests across servers using round-robin routing.";
    }
}