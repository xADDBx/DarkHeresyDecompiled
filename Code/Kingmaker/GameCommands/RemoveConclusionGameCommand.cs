using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class RemoveConclusionGameCommand : GameCommand, IMemoryPackable<RemoveConclusionGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<RemoveConclusionGameCommand>
{
	[Preserve]
	private sealed class RemoveConclusionGameCommandFormatter : MemoryPackFormatter<RemoveConclusionGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref RemoveConclusionGameCommand value)
		{
			RemoveConclusionGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref RemoveConclusionGameCommand value)
		{
			RemoveConclusionGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RemoveConclusionGameCommand value)
		{
			RemoveConclusionGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref RemoveConclusionGameCommand value)
		{
			RemoveConclusionGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private BlueprintConclusionReference _conclusion;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private RemoveConclusionGameCommand()
	{
	}

	[JsonConstructor]
	public RemoveConclusionGameCommand(BlueprintConclusionReference conclusion)
	{
		_conclusion = conclusion;
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.DetectiveSystem.RemoveConclusion(_conclusion);
	}

	static RemoveConclusionGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "RemoveConclusionGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("_conclusion", typeof(BlueprintConclusionReference))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RemoveConclusionGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new RemoveConclusionGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RemoveConclusionGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<RemoveConclusionGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref RemoveConclusionGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value._conclusion);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref RemoveConclusionGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintConclusionReference value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value._conclusion;
				reader.ReadPackable(ref value2);
				goto IL_006a;
			}
			value2 = reader.ReadPackable<BlueprintConclusionReference>();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(RemoveConclusionGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value._conclusion : null);
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_006a;
			}
		}
		value = new RemoveConclusionGameCommand
		{
			_conclusion = value2
		};
		return;
		IL_006a:
		value._conclusion = value2;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref RemoveConclusionGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("_conclusion");
		writer.WritePackable(value._conclusion);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref RemoveConclusionGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		BlueprintConclusionReference val = ((value != null) ? value._conclusion : null);
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "_conclusion")
				{
					val = reader.ReadPackable<BlueprintConclusionReference>();
					array[0] = true;
				}
			}
			else if (text == "_conclusion")
			{
				reader.ReadPackable(ref val);
			}
		}
		if (value != null)
		{
			value._conclusion = val;
		}
		else
		{
			value = new RemoveConclusionGameCommand
			{
				_conclusion = val
			};
		}
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		RemoveConclusionGameCommand source = new RemoveConclusionGameCommand();
		result = Unsafe.As<RemoveConclusionGameCommand, TPossiblyBase>(ref source);
	}

	public override void Serialize<TFormatter>(TFormatter formatter, SerializerState state)
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<RemoveConclusionGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "_conclusion", ref _conclusion, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<RemoveConclusionGameCommand>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			switch (mappingForType[fieldID])
			{
			case byte.MaxValue:
				formatter.SkipField(size);
				break;
			case 0:
				_conclusion = formatter.ReadPackable<BlueprintConclusionReference>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
