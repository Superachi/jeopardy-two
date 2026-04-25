# Netcode Library Overview

This file describes the current refactored netcode structure.
It follows the same general style as the older sequence doc:

- **Short legend**: Quick name lookup
- **Per-case ASCII diagrams**: One scenario at a time
- **Meaning / trigger / importance**: Plain-English explanation under each case
- **Notes**: Important design details
- **Simple term explanations**: Quick glossary at the bottom

## Legend

- **NH**: NetworkHandler
- **SS**: ServerSignals
- **CS**: ClientSignals
- **GNS**: GeneralNetworkSignals
- **SPM**: ServerPeerManager
- **SPL**: ServerPacketListener
- **CPM**: ClientPeerManager
- **CPL**: ClientPacketListener
- **PI**: PacketInfo
- **IDA**: IDAssignment
- **PW**: PacketWriter
- **PR**: PacketReader
- **ENet**: ENetConnection / ENetPacketPeer

## Case 1: Netcode Node Ownership

```text
Main / Scene Tree
      |
      v
+-------------------+
| NetworkHandler    |
|-------------------|
| owns ServerSignals|
| owns ClientSignals|
| owns GeneralNet...|
| owns ENet conn    |
+-------------------+
      |
      +----> ServerSignals
      |
      +----> ClientSignals
      |
      +----> GeneralNetworkSignals
```

- **Meaning**: One Godot node owns the shared network objects and signal nodes.
- **Who triggers this**: [Nodes/Netcode/NetworkHandler.cs](Nodes/Netcode/NetworkHandler.cs#L1) during `_Ready()`.
- **Why this matters**: This is more library-like than spreading signal ownership across multiple consumer nodes.

## Case 2: Packet Serialization Flow

```text
Packet object         PW                  byte[]               PR                Packet object
     |                |                     |                  |                     |
     | Encode()       |                     |                  |                     |
     |--------------->| write fields        |                  |                     |
     |                |-------------------->| raw packet bytes |                     |
     |                |                     |                  |                     |
     | Decode(data)   |                     |                  |                     |
     |<--------------------------------------------------------------------------->|
     |                |                     |----------------->| read fields         |
     |                |                     |                  |-------------------->|
```

- **Meaning**: Packets no longer manually manage byte indexes in every class. They write through `PacketWriter` and read through `PacketReader`.
- **Who triggers this**: [Nodes/Netcode/PacketInfo.cs](Nodes/Netcode/PacketInfo.cs#L1) and packet subclasses like [Nodes/Netcode/IDAssignment.cs](Nodes/Netcode/IDAssignment.cs#L1).
- **Why this matters**: This makes packet code easier to reuse, extend, and maintain as more packet types are added.

## Case 3: Server Peer Connect -> Broadcast ID Assignment

```text
Server ENet event        NH                  SS                  SPR                 IDA               ENet
      |                  |                   |                   |                   |                  |
      | connect event    |                   |                   |                   |                  |
      |----------------->|                   |                   |                   |                  |
      |                  | PeerConnected()   |                   |                   |                  |
      |                  | emit PeerJoined   |                   |                   |                  |
      |                  |------------------>|                   |                   |                  |
      |                  |                   | OnPeerConnected   |                   |                  |
      |                  |                   |------------------>|                   |                  |
      |                  |                   |                   | update PeerIds     |                  |
      |                  |                   |                   | create IDA         |                  |
      |                  |                   |                   |------------------->|                  |
      |                  |                   |                   | broadcast packet   |                  |
      |                  |                   |                   |--------------------------------------->|
```

- **Meaning**: When a peer joins, the server updates its list of connected peers and sends out a fresh ID-assignment packet.
- **Who triggers this**: `PeerJoined` emitted by [Nodes/Netcode/NetworkHandler.cs](Nodes/Netcode/NetworkHandler.cs#L1) and consumed by [Nodes/Netcode/PeerRegistry/ServerPeerManager.cs](Nodes/Netcode/PeerRegistry/ServerPeerManager.cs#L1).
- **Why this matters**: This keeps all clients aware of who exists in the current session.

## Case 4: Client Receives ID Assignment

```text
ENet                 NH                  CS                  CPR                 IDA
 |                   |                   |                   |                   |
 | packet bytes      |                   |                   |                   |
 |------------------>|                   |                   |                   |
 |                   | emit PacketReceived(data)             |                   |
 |                   |------------------>|                   |                   |
 |                   |                   | OnClientPacket    |                   |
 |                   |                   |------------------>|                   |
 |                   |                   |                   | decode IDA        |
 |                   |                   |                   |------------------>|
 |                   |                   |                   | decoded data      |
 |                   |                   |                   |<------------------|
```

- **Meaning**: The client receives network bytes, forwards them through `ClientSignals`, and decodes them into a packet object.
- **Who triggers this**: A receive event inside [Nodes/Netcode/NetworkHandler.cs](Nodes/Netcode/NetworkHandler.cs#L1).
- **Why this matters**: This is the bridge from low-level transport data to usable gameplay data.

## Case 5: Client Publishes Simpler Gameplay Signals

```text
CPR                                    GNS
 |                                      |
 | first packet?                        |
 | set LocalPeerId                      |
 | set RemotePeerIds                    |
 | emit HandleLocalIdAssignment         |
 |------------------------------------->|
 | emit HandleRemoteIdAssignment        |
 |------------------------------------->|
```

- **Meaning**: `ClientPeerManager` turns packet data into higher-level Godot signals.
- **Who triggers this**: [Nodes/Netcode/PeerRegistry/ClientPeerManager.cs](Nodes/Netcode/PeerRegistry/ClientPeerManager.cs#L1) after decoding an `IDAssignment` packet.
- **Why this matters**: The rest of the game can listen to simpler signals without caring about packet bytes.

## Case 6: Signal Subscription Lifecycle

```text
Node enters tree
    -> _Ready()
    -> subscribe to shared signal node

Node exits tree
    -> _ExitTree()
    -> unsubscribe from shared signal node
```

- **Meaning**: The globals now subscribe and unsubscribe cleanly instead of only attaching handlers once.
- **Who triggers this**: [Nodes/Netcode/PeerRegistry/ClientPeerManager.cs](Nodes/Netcode/PeerRegistry/ClientPeerManager.cs#L1) and [Nodes/Netcode/PeerRegistry/ServerPeerManager.cs](Nodes/Netcode/PeerRegistry/ServerPeerManager.cs#L1).
- **Why this matters**: This is safer for scene reloads, testing, and future library reuse.

## Case 7: Current Intended Polling Flow

```text
Game loop / frame tick
        |
        v
NetworkHandler._Process(delta)
        |
        +--> HandlePackets()   [planned to be used later]
                |
                +--> ENet events
                +--> emit Godot signal nodes
                +--> globals react
```

- **Meaning**: `HandlePackets()` is still the intended polling point, but it is intentionally not wired in yet.
- **Who triggers this**: Future frame-based or manually-controlled polling.
- **Why this matters**: Keeping this deferred is fine, but this method is still the main event pump entry point.

## Notes

- **Important**: [Nodes/Netcode/NetworkHandler.cs](Nodes/Netcode/NetworkHandler.cs#L1) now owns all shared network signal nodes.
- **Important**: [Nodes/Netcode/PacketSerialization.cs](Nodes/Netcode/PacketSerialization.cs#L1) is the new serializer helper layer.
- **Important**: [Nodes/Netcode/PacketInfo.cs](Nodes/Netcode/PacketInfo.cs#L1) now provides reusable `WritePayload` and `ReadPayload` hooks for packet subclasses.
- **Important**: [Nodes/Netcode/IDAssignment.cs](Nodes/Netcode/IDAssignment.cs#L1) is the example packet currently using the serializer layer.
- **Important**: [Nodes/Netcode/PeerRegistry/ClientPeerManager.cs](Nodes/Netcode/PeerRegistry/ClientPeerManager.cs#L1) now uses `LocalPeerId` and `RemotePeerIds` instead of the older mixed-type ID model.
- **Important**: [Nodes/Netcode/PeerRegistry/ServerPeerManager.cs](Nodes/Netcode/PeerRegistry/ServerPeerManager.cs#L1) and [Nodes/Netcode/PeerRegistry/ServerPacketListener.cs](Nodes/Netcode/PeerRegistry/ServerPacketListener.cs#L1) now unsubscribe from shared signals in `_ExitTree()`.
- **Important**: [Nodes/Netcode/NetworkHandler.cs](Nodes/Netcode/NetworkHandler.cs#L58) still does not call `HandlePackets()` from `_Process`, by design for now.

## Simple Term Explanations

- **Serializer layer**: Helper code that turns packet objects into bytes and bytes back into packet objects.
- **PacketWriter**: A helper that builds packet bytes in order.
- **PacketReader**: A helper that reads packet bytes in order.
- **Payload**: The packet data after the packet type byte.
- **Shared signal node**: A Godot node that exists mainly to emit signals other classes can subscribe to.
- **Signal ownership**: Which node is responsible for creating and exposing the signal nodes.
- **Lifecycle-safe subscription**: Connecting to signals in `_Ready()` and disconnecting in `_ExitTree()`.
- **LocalPeerId**: The ID of the current client on this machine.
- **RemotePeerIds**: The IDs of all other peers known to this client.
- **Broadcast**: Send one packet to every connected peer.
- **Polling**: Repeatedly checking the network connection for pending events.
- **Godot-specific library**: A reusable C# library designed around Godot nodes, signals, and engine APIs instead of trying to be engine-agnostic.
