using System;
using System.Buffers;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

[Serializable]
[MemoryPackable(GenerateType.Object)]
public struct NodeIndexData : IMemoryPackable<NodeIndexData>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class NodeIndexDataFormatter : MemoryPackFormatter<NodeIndexData>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref NodeIndexData value)
		{
			NodeIndexData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref NodeIndexData value)
		{
			NodeIndexData.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref NodeIndexData value)
		{
			NodeIndexData.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref NodeIndexData value)
		{
			NodeIndexData.DeserializeJson(ref reader, ref value);
		}
	}

	private static readonly Vector2Int NoCoords;

	[MemoryPackInclude]
	public Vector2Int CoordinatesInGrid;

	[MemoryPackInclude]
	public Vector2Int EndCoordinatesInGrid;

	[MemoryPackInclude]
	public int PosX;

	[MemoryPackInclude]
	public int PosY;

	[MemoryPackInclude]
	public int PosZ;

	[MemoryPackIgnore]
	public bool IsGrid
	{
		get
		{
			if (!IsLink)
			{
				return CoordinatesInGrid != NoCoords;
			}
			return false;
		}
	}

	[MemoryPackIgnore]
	public bool IsLink
	{
		get
		{
			if (CoordinatesInGrid != NoCoords)
			{
				return EndCoordinatesInGrid != NoCoords;
			}
			return false;
		}
	}

	[MemoryPackIgnore]
	public Int3 Position => new Int3(PosX, PosY, PosZ);

	public static NodeIndexData Null()
	{
		NodeIndexData result = default(NodeIndexData);
		result.CoordinatesInGrid = NoCoords;
		result.EndCoordinatesInGrid = NoCoords;
		return result;
	}

	public static NodeIndexData GridNode(GridNode gridNode)
	{
		NodeIndexData nodeIndexData = default(NodeIndexData);
		nodeIndexData.CoordinatesInGrid = gridNode.CoordinatesInGrid;
		nodeIndexData.EndCoordinatesInGrid = NoCoords;
		return nodeIndexData.SetPosition(gridNode.position);
	}

	public static NodeIndexData LinkNode(LinkNode linkNode)
	{
		NodeIndexData nodeIndexData = default(NodeIndexData);
		nodeIndexData.CoordinatesInGrid = ((GridNode)linkNode.linkConcrete.startNodes[0]).CoordinatesInGrid;
		nodeIndexData.EndCoordinatesInGrid = ((GridNode)linkNode.linkConcrete.endNodes[0]).CoordinatesInGrid;
		return nodeIndexData.SetPosition(linkNode.position);
	}

	private NodeIndexData SetPosition(Int3 position)
	{
		PosX = position.x;
		PosY = position.y;
		PosZ = position.z;
		return this;
	}

	static NodeIndexData()
	{
		NoCoords = new Vector2Int(int.MinValue, int.MinValue);
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<NodeIndexData>())
		{
			MemoryPackFormatterProvider.Register(new NodeIndexDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<NodeIndexData[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<NodeIndexData>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref NodeIndexData value) where TBufferWriter : class, IBufferWriter<byte>
	{
		writer.WriteUnmanaged(in value);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref NodeIndexData value)
	{
		reader.ReadUnmanaged<NodeIndexData>(out value);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref NodeIndexData value)
	{
		writer.WriteObjectHeader();
		writer.WriteProperty("CoordinatesInGrid");
		writer.WriteUnmanaged(value.CoordinatesInGrid);
		writer.WriteProperty("EndCoordinatesInGrid");
		writer.WriteUnmanaged(value.EndCoordinatesInGrid);
		writer.WriteProperty("PosX");
		writer.WriteUnmanaged(value.PosX);
		writer.WriteProperty("PosY");
		writer.WriteUnmanaged(value.PosY);
		writer.WriteProperty("PosZ");
		writer.WriteUnmanaged(value.PosZ);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref NodeIndexData value)
	{
		if (!reader.CheckObjectStart())
		{
			value = default(NodeIndexData);
			reader.Advance();
			return;
		}
		reader.Advance();
		Vector2Int v = default(Vector2Int);
		Vector2Int v2 = default(Vector2Int);
		int v3 = 0;
		int v4 = 0;
		int v5 = 0;
		bool[] array = new bool[5];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			switch (text)
			{
			case "CoordinatesInGrid":
				reader.ReadUnmanaged<Vector2Int>(out v);
				array[0] = true;
				break;
			case "EndCoordinatesInGrid":
				reader.ReadUnmanaged<Vector2Int>(out v2);
				array[1] = true;
				break;
			case "PosX":
				reader.ReadUnmanaged<int>(out v3);
				array[2] = true;
				break;
			case "PosY":
				reader.ReadUnmanaged<int>(out v4);
				array[3] = true;
				break;
			case "PosZ":
				reader.ReadUnmanaged<int>(out v5);
				array[4] = true;
				break;
			}
		}
		value = new NodeIndexData
		{
			CoordinatesInGrid = v,
			EndCoordinatesInGrid = v2,
			PosX = v3,
			PosY = v4,
			PosZ = v5
		};
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}
}
