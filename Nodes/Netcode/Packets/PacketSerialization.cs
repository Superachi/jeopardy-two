using System;
using System.Collections.Generic;

namespace Nodes.Netcode.Packets;

public sealed class PacketWriter
{
	private readonly List<byte> _buffer = new();

	public void WriteByte(byte value)
	{
		_buffer.Add(value);
	}

	public void WriteBytes(IEnumerable<byte> values)
	{
		_buffer.AddRange(values);
	}

	public byte[] ToArray()
	{
		return _buffer.ToArray();
	}
}

public sealed class PacketReader
{
	private readonly byte[] _buffer;
	private int _offset;

	public PacketReader(byte[] buffer)
	{
		_buffer = buffer ?? Array.Empty<byte>();
	}

	public int Remaining => _buffer.Length - _offset;

	public bool TryReadByte(out byte value)
	{
		if (_offset >= _buffer.Length)
		{
			value = 0;
			return false;
		}

		value = _buffer[_offset];
		_offset++;
		return true;
	}

	public byte[] ReadRemainingBytes()
	{
		if (Remaining <= 0)
		{
			return Array.Empty<byte>();
		}

		var remaining = new byte[Remaining];
		Array.Copy(_buffer, _offset, remaining, 0, Remaining);
		_offset = _buffer.Length;
		return remaining;
	}

	public void Reset()
	{
		_offset = 0;
	}

	public static PacketReader FromPacket(byte[] buffer)
	{
		return new PacketReader(buffer);
	}
}