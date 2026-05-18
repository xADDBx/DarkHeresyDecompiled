using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class PartyFormationIndexGameCommand : GameCommand, IMemoryPackable<PartyFormationIndexGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<PartyFormationIndexGameCommand>
{
	[Preserve]
	private sealed class PartyFormationIndexGameCommandFormatter : MemoryPackFormatter<PartyFormationIndexGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref PartyFormationIndexGameCommand value)
		{
			PartyFormationIndexGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref PartyFormationIndexGameCommand value)
		{
			PartyFormationIndexGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref PartyFormationIndexGameCommand value)
		{
			PartyFormationIndexGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref PartyFormationIndexGameCommand value)
		{
			PartyFormationIndexGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly int m_FormationIndex;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly bool _fromUi;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	public override bool IsForcedSynced => true;

	[JsonConstructor]
	private PartyFormationIndexGameCommand()
	{
	}

	[MemoryPackConstructor]
	public PartyFormationIndexGameCommand(int m_formationIndex, bool fromUi)
	{
		m_FormationIndex = m_formationIndex;
		_fromUi = fromUi;
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.Player.FormationManager.SetCurrentFormationIndex(m_FormationIndex, _fromUi);
	}

	static PartyFormationIndexGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "PartyFormationIndexGameCommand",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("m_FormationIndex", typeof(int)),
				new FieldInfo("_fromUi", typeof(bool))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<PartyFormationIndexGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new PartyFormationIndexGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PartyFormationIndexGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<PartyFormationIndexGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref PartyFormationIndexGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(2, in value.m_FormationIndex, in value._fromUi);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref PartyFormationIndexGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		int value2;
		bool value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<int, bool>(out value2, out value3);
			}
			else
			{
				value2 = value.m_FormationIndex;
				value3 = value._fromUi;
				reader.ReadUnmanaged<int>(out value2);
				reader.ReadUnmanaged<bool>(out value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(PartyFormationIndexGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = 0;
				value3 = false;
			}
			else
			{
				value2 = value.m_FormationIndex;
				value3 = value._fromUi;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<int>(out value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<bool>(out value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new PartyFormationIndexGameCommand(value2, value3);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref PartyFormationIndexGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_FormationIndex");
		writer.WriteUnmanaged(value.m_FormationIndex);
		writer.WriteProperty("_fromUi");
		writer.WriteUnmanaged(value._fromUi);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref PartyFormationIndexGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		int v;
		bool v2;
		if (value == null)
		{
			v = 0;
			v2 = false;
		}
		else
		{
			v = value.m_FormationIndex;
			v2 = value._fromUi;
		}
		bool[] array = new bool[2];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (!(text == "m_FormationIndex"))
				{
					if (text == "_fromUi")
					{
						reader.ReadUnmanaged<bool>(out v2);
						array[1] = true;
					}
				}
				else
				{
					reader.ReadUnmanaged<int>(out v);
					array[0] = true;
				}
			}
			else if (!(text == "m_FormationIndex"))
			{
				if (text == "_fromUi")
				{
					reader.ReadUnmanaged<bool>(out v2);
				}
			}
			else
			{
				reader.ReadUnmanaged<int>(out v);
			}
		}
		_ = value;
		value = new PartyFormationIndexGameCommand(v, v2);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartyFormationIndexGameCommand source = new PartyFormationIndexGameCommand();
		result = Unsafe.As<PartyFormationIndexGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartyFormationIndexGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		int value = m_FormationIndex;
		formatter.UnmanagedField(0, "m_FormationIndex", ref value, state);
		bool value2 = _fromUi;
		formatter.UnmanagedField(1, "_fromUi", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartyFormationIndexGameCommand>();
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
				Unsafe.AsRef(in m_FormationIndex) = formatter.ReadUnmanaged<int>(state);
				break;
			case 1:
				Unsafe.AsRef(in _fromUi) = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
