using System.Collections.Generic;
using System.Linq;
using Achi.Godot.Logging;
using Godot;
using Nodes.Signals;

namespace Nodes.Netcode;

public partial class ClientNetworkGlobals : Node
{
	private readonly GeneralNetworkSignals _generalNetworkSignals = new();

	public int Id { get; private set; } = -1;
	public List<int> RemotedIds { get; private set; } = new();

	/// <summary>
	/// Sets up signal nodes and starts listening for incoming client packets.
	/// </summary>
	public override void _Ready()
	{
		AddChild(_generalNetworkSignals);

		var networkHandler = NetworkHandler.Singleton.Instance;
		if (networkHandler == null)
		{
			LogNode.Log("ClientNetworkGlobals could not find NetworkHandler.");
			return;
		}

		var clientSignals = networkHandler.ClientSignals;
		if (clientSignals == null)
		{
			LogNode.Log("ClientNetworkGlobals could not find ClientSignals under NetworkHandler.");
			return;
		}

		clientSignals.PacketReceived += OnClientPacket;
	}

	/// <summary>
	/// Reads incoming packet bytes and routes each packet to the right handler.
	/// </summary>
	/// <param name="data">Raw packet bytes received from the network.</param>
	private void OnClientPacket(byte[] data)
	{
		if (data == null || data.Length == 0)
		{
			LogNode.Log("Received empty client packet.");
			return;
		}

		var packetType = (PacketInfo.PacketType)data[0];

		switch (packetType)
		{
			case PacketInfo.PacketType.IdAssignment:
				ManageIds(IDAssignment.CreateFromData(data));
				break;

			default:
				LogNode.Log($"Unhandled packet type index {data[0]}.");
				break;
		}
	}

	/// <summary>
	/// Applies ID assignment data and emits local or remote ID signals.
	/// </summary>
	/// <param name="idAssignment">Decoded ID assignment packet.</param>
	private void ManageIds(IDAssignment idAssignment)
	{
		if (Id == -1)
		{
			Id = idAssignment.Id;
			_generalNetworkSignals.EmitSignal(GeneralNetworkSignals.SignalName.HandleLocalIdAssignment, Id);

			RemotedIds = idAssignment.RemotedIds.Select(remoteId => (int)remoteId).ToList();
			foreach (int remoteId in RemotedIds)
			{
				if (remoteId == Id)
				{
					continue;
				}

				_generalNetworkSignals.EmitSignal(GeneralNetworkSignals.SignalName.HandleRemoteIdAssignment, remoteId);
			}
		}
		else
		{
			RemotedIds.Add(idAssignment.Id);
			_generalNetworkSignals.EmitSignal(GeneralNetworkSignals.SignalName.HandleRemoteIdAssignment, idAssignment.Id);
		}
	}
}
