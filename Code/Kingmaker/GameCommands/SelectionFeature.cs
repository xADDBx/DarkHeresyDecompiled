using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[OwlPackOldName("Kingmaker.GameCommands.SelectionEntry")]
[MemoryPackable(GenerateType.Object)]
public sealed class SelectionFeature : IMemoryPackable<SelectionFeature>, IMemoryPackFormatterRegister, IOwlPackable, IOwlPackable<SelectionFeature>
{
	[Preserve]
	private sealed class SelectionFeatureFormatter : MemoryPackFormatter<SelectionFeature>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SelectionFeature value)
		{
			SelectionFeature.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref SelectionFeature value)
		{
			SelectionFeature.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SelectionFeature value)
		{
			SelectionFeature.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref SelectionFeature value)
		{
			SelectionFeature.DeserializeJson(ref reader, ref value);
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
	public readonly BlueprintFeatureReference Feature;

	public static readonly TypeInfo OwlPackTypeInfo;

	[MemoryPackConstructor]
	public SelectionFeature([NotNull] BlueprintSelectionReference selection, int pathRank, [NotNull] BlueprintFeatureReference feature)
	{
		Selection = selection;
		PathRank = pathRank;
		Feature = feature;
	}

	public SelectionFeature(OwlPackConstructorParameter _)
	{
	}

	static SelectionFeature()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "SelectionFeature",
			OldNames = new string[1] { "Kingmaker.GameCommands.SelectionEntry" },
			Fields = new FieldInfo[0]
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SelectionFeature>())
		{
			MemoryPackFormatterProvider.Register(new SelectionFeatureFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SelectionFeature[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SelectionFeature>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SelectionFeature? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(3);
		writer.WritePackable(in value.Selection);
		writer.WriteUnmanaged(in value.PathRank);
		writer.WritePackable(in value.Feature);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SelectionFeature? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintSelectionReference value2;
		int value3;
		BlueprintFeatureReference value4;
		if (memberCount == 3)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<BlueprintSelectionReference>();
				reader.ReadUnmanaged<int>(out value3);
				value4 = reader.ReadPackable<BlueprintFeatureReference>();
			}
			else
			{
				value2 = value.Selection;
				value3 = value.PathRank;
				value4 = value.Feature;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<int>(out value3);
				reader.ReadPackable(ref value4);
			}
		}
		else
		{
			if (memberCount > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SelectionFeature), 3, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = 0;
				value4 = null;
			}
			else
			{
				value2 = value.Selection;
				value3 = value.PathRank;
				value4 = value.Feature;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<int>(out value3);
					if (memberCount != 2)
					{
						reader.ReadPackable(ref value4);
						_ = 3;
					}
				}
			}
			_ = value;
		}
		value = new SelectionFeature(value2, value3, value4);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref SelectionFeature? value)
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
		writer.WriteProperty("Feature");
		writer.WritePackable(value.Feature);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref SelectionFeature? value)
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
		BlueprintFeatureReference val2;
		if (value == null)
		{
			val = null;
			v = 0;
			val2 = null;
		}
		else
		{
			val = value.Selection;
			v = value.PathRank;
			val2 = value.Feature;
		}
		bool[] array = new bool[3];
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
				case "Feature":
					val2 = reader.ReadPackable<BlueprintFeatureReference>();
					array[2] = true;
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
				case "Feature":
					reader.ReadPackable(ref val2);
					break;
				}
			}
		}
		_ = value;
		value = new SelectionFeature(val, v, val2);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SelectionFeature source = new SelectionFeature(default(OwlPackConstructorParameter));
		result = Unsafe.As<SelectionFeature, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SelectionFeature>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SelectionFeature>();
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
