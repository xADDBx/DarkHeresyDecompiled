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
public sealed class PartyFormationResetGameCommand : GameCommand, IMemoryPackable<PartyFormationResetGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<PartyFormationResetGameCommand>
{
	[Preserve]
	private sealed class PartyFormationResetGameCommandFormatter : MemoryPackFormatter<PartyFormationResetGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref PartyFormationResetGameCommand value)
		{
			PartyFormationResetGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref PartyFormationResetGameCommand value)
		{
			PartyFormationResetGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref PartyFormationResetGameCommand value)
		{
			PartyFormationResetGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref PartyFormationResetGameCommand value)
		{
			PartyFormationResetGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly int m_FormationIndex;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private PartyFormationResetGameCommand()
	{
	}

	[MemoryPackConstructor]
	public PartyFormationResetGameCommand(int m_formationIndex)
	{
		m_FormationIndex = m_formationIndex;
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.Player.FormationManager.ResetCustomFormation(m_FormationIndex);
	}

	static PartyFormationResetGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "PartyFormationResetGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("m_FormationIndex", typeof(int))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<PartyFormationResetGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new PartyFormationResetGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PartyFormationResetGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<PartyFormationResetGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref PartyFormationResetGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.m_FormationIndex);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref PartyFormationResetGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		int value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<int>(out value2);
			}
			else
			{
				value2 = value.m_FormationIndex;
				reader.ReadUnmanaged<int>(out value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(PartyFormationResetGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_FormationIndex : 0);
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<int>(out value2);
				_ = 1;
			}
			_ = value;
		}
		value = new PartyFormationResetGameCommand(value2);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref PartyFormationResetGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_FormationIndex");
		writer.WriteUnmanaged(value.m_FormationIndex);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref PartyFormationResetGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		int v = ((value != null) ? value.m_FormationIndex : 0);
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "m_FormationIndex")
				{
					reader.ReadUnmanaged<int>(out v);
					array[0] = true;
				}
			}
			else if (text == "m_FormationIndex")
			{
				reader.ReadUnmanaged<int>(out v);
			}
		}
		_ = value;
		value = new PartyFormationResetGameCommand(v);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartyFormationResetGameCommand source = new PartyFormationResetGameCommand();
		result = Unsafe.As<PartyFormationResetGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartyFormationResetGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		int value = m_FormationIndex;
		formatter.UnmanagedField(0, "m_FormationIndex", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartyFormationResetGameCommand>();
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
			}
		}
		formatter.LeaveObject();
	}
}
