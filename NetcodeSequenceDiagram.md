# Netcode Sequence Diagram

This file uses small ASCII diagrams instead of Mermaid so it stays readable in the normal editor view.

## Legend

- **NH**: NetworkHandler
- **SS**: ServerSignals
- **CS**: ClientSignals
- **SNG**: ServerNetworkGlobals
- **CNG**: ClientNetworkGlobals
- **GNS**: GeneralNetworkSignals
- **IDA**: IDAssignment
- **ENet**: ENetConnection / ENetPacketPeer

## Case 1: Server Peer Connect

```text
Server ENet event        NH                  SS                  SNG                 IDA               ENet
      |                  |                   |                   |                   |                  |
      |  connect event   |                   |                   |                   |                  |
      |----------------->|                   |                   |                   |                  |
      |                  | PeerConnected()   |                   |                   |                  |
      |                  | emit PeerJoined   |                   |                   |                  |
      |                  |------------------>|                   |                   |                  |
      |                  |                   | OnPeerConnected   |                   |                  |
      |                  |                   |------------------>|                   |                  |
      |                  |                   |                   | add peer to list   |                  |
      |                  |                   |                   | Create(...)        |                  |
      |                  |                   |                   |------------------->|                  |
      |                  |                   |                   | Broadcast()        |                  |
      |                  |                   |                   |--------------------------------------->|
```

- **Meaning**: A new peer connects to the server, gets an ID, and that updated ID list is sent out.
- **Who triggers this**: A low-level ENet connect event reaching [Nodes/Netcode/NetworkHandler.cs](Nodes/Netcode/NetworkHandler.cs#L1).
- **Why this matters**: This is how every connected client learns who just joined and what IDs currently exist.

## Case 2: Network Delivery To Client

```text
ENet                 NH                  CS                  CNG                 IDA
 |                   |                   |                   |                   |
 | packet bytes      |                   |                   |                   |
 |------------------>|                   |                   |                   |
 |                   | emit PacketReceived(data)             |                   |
 |                   |------------------>|                   |                   |
 |                   |                   | OnClientPacket    |                   |
 |                   |                   |------------------>|                   |
 |                   |                   |                   | CreateFromData    |
 |                   |                   |                   |------------------>|
 |                   |                   |                   | decoded IDs       |
 |                   |                   |                   |<------------------|
```

- **Meaning**: Raw network bytes reach the client and get turned back into an `IDAssignment` object.
- **Who triggers this**: A receive event handled by [Nodes/Netcode/NetworkHandler.cs](Nodes/Netcode/NetworkHandler.cs#L1).
- **Why this matters**: Without this step, the client only has raw bytes and cannot update gameplay state.

## Case 3: First ID Assignment On Client

```text
CNG                                    GNS
 |                                      |
 | Id == -1                             |
 | set Id                               |
 | set RemotedIds                       |
 | emit HandleLocalIdAssignment(Id)     |
 |------------------------------------->|
 | for each remote peer != local peer   |
 | emit HandleRemoteIdAssignment(id)    |
 |------------------------------------->|
```

- **Meaning**: The client learns which player it is and who else is already in the match.
- **Who triggers this**: The first decoded `IDAssignment` packet processed by [Nodes/Netcode/ClientNetworkGlobals.cs](Nodes/Netcode/ClientNetworkGlobals.cs#L1).
- **Why this matters**: This establishes the local player's identity and publishes easier-to-use signals for the rest of the game.

## Case 4: Later Remote Peer Update On Client

```text
CNG                                    GNS
 |                                      |
 | Id already set                       |
 | add new remote peer id               |
 | emit HandleRemoteIdAssignment(id)    |
 |------------------------------------->|
```

- **Meaning**: The client already has its own ID, so a later ID assignment is treated as another player joining.
- **Who triggers this**: A later `IDAssignment` packet received after `Id` has already been set.
- **Why this matters**: This keeps each client's remote player list in sync as new peers appear.

## Case 5: Server Peer Disconnect

```text
Server ENet event        NH                  SS                  SNG
      |                  |                   |                   |
      | disconnect event |                   |                   |
      |----------------->|                   |                   |
      |                  | PeerDisconnected()|                   |
      |                  | emit PeerLeft     |                   |
      |                  |------------------>|                   |
      |                  |                   | OnPeerDisconnected|
      |                  |                   |------------------>|
      |                  |                   |                   | remove peer from list
      |                  |                   |                   | TODO: unassignment broadcast
```

- **Meaning**: A peer leaves, and the server removes that peer from its tracked ID list.
- **Who triggers this**: A low-level ENet disconnect event reaching [Nodes/Netcode/NetworkHandler.cs](Nodes/Netcode/NetworkHandler.cs#L1).
- **Why this matters**: The server stops treating that peer as active, though the follow-up removal packet is still a TODO.

## Case 6: Server Receives A Packet It Does Not Handle Yet

```text
ENet                 NH                  SS                  SNG
 |                   |                   |                   |
 | server packet     |                   |                   |
 |------------------>|                   |                   |
 |                   | emit PacketReceived(peerId, data)     |
 |                   |------------------>|                   |
 |                   |                   | OnServerPacket    |
 |                   |                   |------------------>|
 |                   |                   |                   | log unhandled packet type
```

- **Meaning**: The server receives packet data but does not yet have custom handling for that packet type.
- **Who triggers this**: Any server-side receive event forwarded through [Nodes/Signals/ServerSignals.cs](Nodes/Signals/ServerSignals.cs#L1).
- **Why this matters**: It shows where future server-side packet handling logic should be added.

## Notes

- **Important**: [Nodes/Netcode/NetworkHandler.cs](Nodes/Netcode/NetworkHandler.cs#L1) is the hub between ENet events and Godot signal nodes.
- **Important**: [Nodes/Netcode/ServerNetworkGlobals.cs](Nodes/Netcode/ServerNetworkGlobals.cs#L1) listens to server-side signals and creates [Nodes/Netcode/IDAssignment.cs](Nodes/Netcode/IDAssignment.cs#L1) packets.
- **Important**: [Nodes/Netcode/ClientNetworkGlobals.cs](Nodes/Netcode/ClientNetworkGlobals.cs#L1) decodes [Nodes/Netcode/IDAssignment.cs](Nodes/Netcode/IDAssignment.cs#L1) and re-emits simpler gameplay-facing signals through [Nodes/Signals/GeneralNetworkSignals.cs](Nodes/Signals/GeneralNetworkSignals.cs#L1).
- **Important**: [Nodes/Netcode/NetworkHandler.cs](Nodes/Netcode/NetworkHandler.cs#L52) still does not call `HandlePackets()` from `_Process`, so this diagram reflects the current intended flow more than a fully active polling loop.

## Simple Term Explanations

- **ID assignment**: The server gives a player a number that uniquely identifies them in the match.
- **Local ID**: The current player's own ID on their machine.
- **Remote ID**: Another player's ID from the point of view of the current machine.
- **Peer**: One connected player or client in the network session.
- **Packet**: A small bundle of bytes sent over the network.
- **Packet bytes**: The raw data form of a packet before code turns it back into useful values.
- **Decode**: Reading raw packet bytes and turning them back into structured data.
- **Encode**: Turning structured data into raw packet bytes that can be sent over the network.
- **Broadcast**: Send the same packet to every connected peer.
- **Signal**: Godot's event system. One object emits a signal, and other objects can react to it.
- **ENet**: The low-level networking layer Godot is using here to send and receive data.
- **PacketReceived**: A signal that says new network data has arrived.
- **PeerJoined**: A signal that says a new peer connected.
- **PeerLeft**: A signal that says a peer disconnected.
- **Polling loop**: Code that repeatedly checks the network connection for new events every frame.
