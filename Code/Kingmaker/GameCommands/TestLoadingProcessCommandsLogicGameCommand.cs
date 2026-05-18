using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Networking;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public class TestLoadingProcessCommandsLogicGameCommand : GameCommand, IMemoryPackable<TestLoadingProcessCommandsLogicGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<TestLoadingProcessCommandsLogicGameCommand>
{
	[Preserve]
	private sealed class TestLoadingProcessCommandsLogicGameCommandFormatter : MemoryPackFormatter<TestLoadingProcessCommandsLogicGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref TestLoadingProcessCommandsLogicGameCommand value)
		{
			TestLoadingProcessCommandsLogicGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref TestLoadingProcessCommandsLogicGameCommand value)
		{
			TestLoadingProcessCommandsLogicGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref TestLoadingProcessCommandsLogicGameCommand value)
		{
			TestLoadingProcessCommandsLogicGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref TestLoadingProcessCommandsLogicGameCommand value)
		{
			TestLoadingProcessCommandsLogicGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private int m_Counter;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private NetPlayerSerializable m_Player;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	public override bool IsForcedSynced => true;

	[JsonConstructor]
	protected TestLoadingProcessCommandsLogicGameCommand()
	{
	}

	[MemoryPackConstructor]
	public TestLoadingProcessCommandsLogicGameCommand(int m_counter, NetPlayerSerializable m_player)
	{
		m_Counter = m_counter;
		m_Player = m_player;
	}

	public TestLoadingProcessCommandsLogicGameCommand(int counter, NetPlayer player)
		: this(counter, (NetPlayerSerializable)player)
	{
		PFLog.Net.Log($"TestLoadingProcessCommandsLogicGameCommand new {m_Counter} {((NetPlayer)m_Player).ToString()}");
	}

	protected override void ExecuteInternal()
	{
		PFLog.Net.Log($"TestLoadingProcessCommandsLogicGameCommand exe {m_Counter} {((NetPlayer)m_Player).ToString()}");
		if (m_Counter != 0 && NetworkingManager.LocalNetPlayer.Equals((NetPlayer)m_Player))
		{
			Game.Instance.GameCommandQueue.TestLoadingProcessCommandsLogicGameCommand(m_Counter - 1, (NetPlayer)m_Player);
		}
	}

	static TestLoadingProcessCommandsLogicGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "TestLoadingProcessCommandsLogicGameCommand",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("m_Counter", typeof(int)),
				new FieldInfo("m_Player", typeof(NetPlayerSerializable))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<TestLoadingProcessCommandsLogicGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new TestLoadingProcessCommandsLogicGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<TestLoadingProcessCommandsLogicGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<TestLoadingProcessCommandsLogicGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref TestLoadingProcessCommandsLogicGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(2, in value.m_Counter, in value.m_Player);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref TestLoadingProcessCommandsLogicGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		int value2;
		NetPlayerSerializable value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<int, NetPlayerSerializable>(out value2, out value3);
			}
			else
			{
				value2 = value.m_Counter;
				value3 = value.m_Player;
				reader.ReadUnmanaged<int>(out value2);
				reader.ReadUnmanaged<NetPlayerSerializable>(out value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(TestLoadingProcessCommandsLogicGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = 0;
				value3 = default(NetPlayerSerializable);
			}
			else
			{
				value2 = value.m_Counter;
				value3 = value.m_Player;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<int>(out value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<NetPlayerSerializable>(out value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new TestLoadingProcessCommandsLogicGameCommand(value2, value3);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref TestLoadingProcessCommandsLogicGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_Counter");
		writer.WriteUnmanaged(value.m_Counter);
		writer.WriteProperty("m_Player");
		writer.WriteUnmanaged(value.m_Player);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref TestLoadingProcessCommandsLogicGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		int v;
		NetPlayerSerializable v2;
		if (value == null)
		{
			v = 0;
			v2 = default(NetPlayerSerializable);
		}
		else
		{
			v = value.m_Counter;
			v2 = value.m_Player;
		}
		bool[] array = new bool[2];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (!(text == "m_Counter"))
				{
					if (text == "m_Player")
					{
						reader.ReadUnmanaged<NetPlayerSerializable>(out v2);
						array[1] = true;
					}
				}
				else
				{
					reader.ReadUnmanaged<int>(out v);
					array[0] = true;
				}
			}
			else if (!(text == "m_Counter"))
			{
				if (text == "m_Player")
				{
					reader.ReadUnmanaged<NetPlayerSerializable>(out v2);
				}
			}
			else
			{
				reader.ReadUnmanaged<int>(out v);
			}
		}
		_ = value;
		value = new TestLoadingProcessCommandsLogicGameCommand(v, v2);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		TestLoadingProcessCommandsLogicGameCommand source = new TestLoadingProcessCommandsLogicGameCommand();
		result = Unsafe.As<TestLoadingProcessCommandsLogicGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<TestLoadingProcessCommandsLogicGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.UnmanagedField(0, "m_Counter", ref m_Counter, state);
		formatter.Field(1, "m_Player", ref m_Player, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<TestLoadingProcessCommandsLogicGameCommand>();
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
				m_Counter = formatter.ReadUnmanaged<int>(state);
				break;
			case 1:
				m_Player = formatter.ReadPackable<NetPlayerSerializable>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
