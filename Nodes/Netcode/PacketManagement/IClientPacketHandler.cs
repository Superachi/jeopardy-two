namespace Nodes.Netcode.PacketManagement;

/// <summary>
/// Handles a specific packet type received by the client packet listener.
/// </summary>
public interface IClientPacketHandler
{
    /// <summary>
    /// The packet type this handler is responsible for.
    /// </summary>
    byte PacketType { get; }

    /// <summary>
    /// Handles packet bytes received from the network.
    /// </summary>
    /// <param name="data">Raw packet bytes.</param>
    void HandlePacket(byte[] data);
}
