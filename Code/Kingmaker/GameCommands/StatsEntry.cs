using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Stats.Base;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class StatsEntry : IMemoryPackable<StatsEntry>, IMemoryPackFormatterRegister, IOwlPackable, IOwlPackable<StatsEntry>
{
	[Preserve]
	private sealed class StatsEntryFormatter : MemoryPackFormatter<StatsEntry>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref StatsEntry value)
		{
			StatsEntry.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref StatsEntry value)
		{
			StatsEntry.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref StatsEntry value)
		{
			StatsEntry.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref StatsEntry value)
		{
			StatsEntry.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	public readonly BlueprintSelectionReference Selection;

	[JsonProperty]
	[MemoryPackInclude]
	public readonly int PathRank;

	[JsonProperty]
	[MemoryPackInclude]
	public readonly StatType StatType;

	[JsonProperty]
	[MemoryPackInclude]
	public readonly int Points;

	public static readonly TypeInfo OwlPackTypeInfo;

	[MemoryPackConstructor]
	public StatsEntry([NotNull] BlueprintSelectionReference selection, int pathRank, StatType statType, int points)
	{
		Selection = selection;
		PathRank = pathRank;
		StatType = statType;
		Points = points;
	}

	public StatsEntry(OwlPackConstructorParameter _)
	{
	}

	static StatsEntry()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "StatsEntry",
			OldNames = null,
			Fields = new FieldInfo[0]
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<StatsEntry>())
		{
			MemoryPackFormatterProvider.Register(new StatsEntryFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<StatsEntry[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<StatsEntry>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<StatType>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<StatType>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref StatsEntry? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(4);
		writer.WritePackable(in value.Selection);
		writer.WriteUnmanaged(in value.PathRank, in value.StatType, in value.Points);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref StatsEntry? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintSelectionReference value2;
		int value3;
		StatType value4;
		int value5;
		if (memberCount == 4)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<BlueprintSelectionReference>();
				reader.ReadUnmanaged<int, StatType, int>(out value3, out value4, out value5);
			}
			else
			{
				value2 = value.Selection;
				value3 = value.PathRank;
				value4 = value.StatType;
				value5 = value.Points;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<int>(out value3);
				reader.ReadUnmanaged<StatType>(out value4);
				reader.ReadUnmanaged<int>(out value5);
			}
		}
		else
		{
			if (memberCount > 4)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(StatsEntry), 4, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = 0;
				value4 = StatType.Unknown;
				value5 = 0;
			}
			else
			{
				value2 = value.Selection;
				value3 = value.PathRank;
				value4 = value.StatType;
				value5 = value.Points;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<int>(out value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<StatType>(out value4);
						if (memberCount != 3)
						{
							reader.ReadUnmanaged<int>(out value5);
							_ = 4;
						}
					}
				}
			}
			_ = value;
		}
		value = new StatsEntry(value2, value3, value4, value5);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref StatsEntry? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("Selection");
		writer.WritePackable(value.Selection);
		writer.WriteProperty("PathRank");
		writer.WriteUnmanaged(value.PathRank);
		writer.WriteProperty("StatType");
		writer.WriteUnmanaged(value.StatType);
		writer.WriteProperty("Points");
		writer.WriteUnmanaged(value.Points);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref StatsEntry? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		BlueprintSelectionReference val;
		int v;
		StatType v2;
		int v3;
		if (value == null)
		{
			val = null;
			v = 0;
			v2 = StatType.Unknown;
			v3 = 0;
		}
		else
		{
			val = value.Selection;
			v = value.PathRank;
			v2 = value.StatType;
			v3 = value.Points;
		}
		bool[] array = new bool[4];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				switch (text)
				{
				case "Selection":
					val = reader.ReadPackable<BlueprintSelectionReference>();
					array[0] = true;
					break;
				case "PathRank":
					reader.ReadUnmanaged<int>(out v);
					array[1] = true;
					break;
				case "StatType":
					reader.ReadUnmanaged<StatType>(out v2);
					array[2] = true;
					break;
				case "Points":
					reader.ReadUnmanaged<int>(out v3);
					array[3] = true;
					break;
				}
			}
			else
			{
				switch (text)
				{
				case "Selection":
					reader.ReadPackable(ref val);
					break;
				case "PathRank":
					reader.ReadUnmanaged<int>(out v);
					break;
				case "StatType":
					reader.ReadUnmanaged<StatType>(out v2);
					break;
				case "Points":
					reader.ReadUnmanaged<int>(out v3);
					break;
				}
			}
		}
		_ = value;
		value = new StatsEntry(val, v, v2, v3);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		StatsEntry source = new StatsEntry(default(OwlPackConstructorParameter));
		result = Unsafe.As<StatsEntry, TPossiblyBase>(ref source);
	}

	public void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<StatsEntry>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<StatsEntry>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			if (mappingForType[fieldID] == byte.MaxValue)
			{
				formatter.SkipField(size);
			}
		}
		formatter.LeaveObject();
	}
}
