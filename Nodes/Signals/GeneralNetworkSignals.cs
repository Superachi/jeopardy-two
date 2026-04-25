using Godot;

namespace Nodes.Signals;

public partial class GeneralNetworkSignals : Node
{
	[Signal]
	public delegate void HandleLocalIdAssignmentEventHandler(int localId);

	[Signal]
	public delegate void HandleRemoteIdAssignmentEventHandler(int remoteId);
}