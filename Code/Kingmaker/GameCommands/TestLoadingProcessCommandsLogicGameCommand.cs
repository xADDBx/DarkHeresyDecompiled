using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Networking;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public class TestLoadingProcessCommandsLogicGameCommand : GameCommand, IOwlPackable<TestLoadingProcessCommandsLogicGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private int m_Counter;

	[JsonProperty]
	[OwlPackInclude]
	private NetPlayerSerializable m_Player;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "TestLoadingProcessCommandsLogicGameCommand",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_Counter", typeof(int)),
			new FieldInfo("m_Player", typeof(NetPlayerSerializable))
		}
	};

	public override bool IsSynchronized => true;

	public override bool IsForcedSynced => true;

	[JsonConstructor]
	protected TestLoadingProcessCommandsLogicGameCommand()
	{
	}

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
