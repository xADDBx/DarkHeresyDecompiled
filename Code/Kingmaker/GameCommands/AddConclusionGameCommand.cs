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
public sealed class AddConclusionGameCommand : GameCommand, IMemoryPackable<AddConclusionGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<AddConclusionGameCommand>
{
	[Preserve]
	private sealed class AddConclusionGameCommandFormatter : MemoryPackFormatter<AddConclusionGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref AddConclusionGameCommand value)
		{
			AddConclusionGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref AddConclusionGameCommand value)
		{
			AddConclusionGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref AddConclusionGameCommand value)
		{
			AddConclusionGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref AddConclusionGameCommand value)
		{
			AddConclusionGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private BlueprintConclusionReference _conclusion;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private AddConclusionGameCommand()
	{
	}

	[JsonConstructor]
	public AddConclusionGameCommand(BlueprintConclusionReference conclusion)
	{
		_conclusion = conclusion;
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.DetectiveSystem.AddConclusion(_conclusion);
	}

	static AddConclusionGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "AddConclusionGameCommand",
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
		if (!MemoryPackFormatterProvider.IsRegistered<AddConclusionGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new AddConclusionGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<AddConclusionGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<AddConclusionGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref AddConclusionGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
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
	public static void Deserialize(ref MemoryPackReader reader, ref AddConclusionGameCommand? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(AddConclusionGameCommand), 1, memberCount);
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
		value = new AddConclusionGameCommand
		{
			_conclusion = value2
		};
		return;
		IL_006a:
		value._conclusion = value2;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref AddConclusionGameCommand? value)
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
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref AddConclusionGameCommand? value)
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
			value = new AddConclusionGameCommand
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
		AddConclusionGameCommand source = new AddConclusionGameCommand();
		result = Unsafe.As<AddConclusionGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<AddConclusionGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "_conclusion", ref _conclusion, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<AddConclusionGameCommand>();
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
