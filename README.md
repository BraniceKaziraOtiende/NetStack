# NetStack — Build. Scale. Learn.

> An interactive mobile 3D/AR educational sandbox for learning web infrastructure.  
> Built with Unity 2022.3 LTS · AR Foundation · C#

---

## What is NetStack?

NetStack lets students **physically build web infrastructure** on a virtual table and watch it respond to live traffic. Place servers, load balancers, databases, and caches — then drag a slider from 100 to 10,000 users and watch your architecture under pressure.

When a server overloads it turns red and pulses. When a cache intercepts a database query, a green packet flies across the table. The infrastructure teaches itself.

---

## Features

| Feature | Description |
|---|---|
| 5 node types | Load Balancer, Server, Database, Cache, Client Device |
| Live simulation | User count slider drives real-time load across all nodes |
| Colour feedback | Teal = healthy · Amber = warning · Red = overloaded |
| Tap-to-learn | Tap any node to see its role, load bar, and teaching tip |
| Toast guidance | Contextual messages guide students through each decision |
| AR mode | One button projects the architecture onto a real surface |
| Packet animation | Colour-coded orbs fly between nodes (blue/amber/green) |
| Prerequisite locking | Can't place Cache without a Server — enforces correct order |

---

## Architecture

```
Assets/Scripts/
  ├── Core/           SceneModeManager, PlacementManager, CameraController, ConnectionRenderer
  ├── Infrastructure/ InfrastructureNode (base), LoadBalancerNode, ServerNode, DatabaseNode, CacheNode
  ├── Simulation/     PacketManager, LoadSimulator
  ├── UI/             NodeButtonController, ToolbarManager, ToastNotification,
  │                   InfoOverlayController, StatsPanel, ARInteractionHandler, SplashController
  └── Data/           NodeData (ScriptableObject), ScenarioConfig (ScriptableObject)
```

### Design patterns used

- **Inheritance** — `InfrastructureNode` base class with 4 subclasses
- **Observer pattern** — `UnityEvent` callbacks on node state changes
- **Object pooling** — `PacketManager` recycles 40 packet GameObjects, zero GC allocation
- **ScriptableObjects** — `NodeData` and `ScenarioConfig` separate data from logic
- **SOLID principles** — single responsibility per script, open/closed via virtual methods

---

## Performance optimisations

| Technique | Implementation | Benefit |
|---|---|---|
| Object pooling | PacketManager pre-instantiates 40 DataPacket objects | Eliminates per-frame GC allocation |
| GPU Instancing | Enabled on all node and packet materials | Batches identical draw calls |
| Frustum culling | Unity default — all nodes kept within camera FOV | Zero overdraw from off-screen objects |
| Target FPS | 60 FPS on Android (ARCore minimum spec) | Profiled via Unity Profiler |

---

## How to build

1. Clone this repository
2. Open in **Unity 2022.3 LTS**
3. Install packages via Package Manager: AR Foundation 5.x · ARCore XR Plugin · TextMeshPro
4. Open `Assets/Scenes/Main.unity`
5. Press **Play** for 3D sandbox mode in the editor
6. **File → Build Settings → Android → Build And Run** for device testing

### Device requirements

- Android 8.0+ with ARCore support
- iOS 13+ with ARKit support
- Camera permission required for AR mode

---

## Module context

Built for **Introduction to Software Engineering — Web Infrastructure module**.  
Demonstrates: load balancing, horizontal scaling, database bottlenecks, caching strategies.

---
