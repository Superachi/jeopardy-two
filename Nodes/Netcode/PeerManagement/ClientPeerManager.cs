using System.Collections.Generic;
using Achi.Godot.Logging;
using Godot;
using Nodes.Signals;

namespace Nodes.Netcode.PeerManagement;

/// <summary>
/// Tracks and manages the local and remote peer IDs for the client.
/// Emits signals for local and remote ID assignments.
/// </summary>
public partial class ClientPeerManager : Node
{
    private GeneralNetworkSignals? _generalNetworkSignals;

    public byte? LocalPeerId { get; private set; }
    public List<byte> RemotePeerIds { get; private set; } = new();

    public override void _Ready()
    {
        var networkHandler = NetworkHandler.Singleton.Instance;
        if (networkHandler == null)
        {
            NetworkHandler.DebugLog("ClientPeerManager could not find NetworkHandler.");
            return;
        }

        _generalNetworkSignals = networkHandler.GeneralNetworkSignals;
        if (_generalNetworkSignals == null)
        {
            NetworkHandler.DebugLog("ClientPeerManager could not find GeneralNetworkSignals under NetworkHandler.");
            return;
        }
    }

    /// <summary>
    /// Applies ID assignment data and emits local or remote ID signals.
    /// </summary>
    /// <param name="idAssignment">Decoded ID assignment packet.</param>
    public void ManageIds(IDAssignment idAssignment)
    {
        if (_generalNetworkSignals == null)
        {
            NetworkHandler.DebugLog("ClientPeerManager cannot emit ID signals without GeneralNetworkSignals.");
            return;
        }

        if (LocalPeerId == null)
        {
            LocalPeerId = idAssignment.Id;
            _generalNetworkSignals.EmitSignal(GeneralNetworkSignals.SignalName.HandleLocalIdAssignment, (int)LocalPeerId.Value);

            RemotePeerIds = new List<byte>(idAssignment.RemoteIds);
            foreach (byte remotePeerId in RemotePeerIds)
            {
                if (remotePeerId == LocalPeerId.Value)
                {
                    continue;
                }

                _generalNetworkSignals.EmitSignal(GeneralNetworkSignals.SignalName.HandleRemoteIdAssignment, (int)remotePeerId);
            }
        }
        else
        {
            if (!RemotePeerIds.Contains(idAssignment.Id))
            {
                RemotePeerIds.Add(idAssignment.Id);
                _generalNetworkSignals.EmitSignal(GeneralNetworkSignals.SignalName.HandleRemoteIdAssignment, (int)idAssignment.Id);
            }
        }
    }
}
