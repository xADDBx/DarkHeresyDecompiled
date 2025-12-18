using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public class SwitchPartyCharactersGameCommand : GameCommand, IOwlPackable<SwitchPartyCharactersGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private readonly sbyte m_Index1;

	[JsonProperty]
	[OwlPackInclude]
	private readonly sbyte m_Index2;

	[JsonProperty]
	[OwlPackInclude]
	private readonly EntityRef<BaseUnitEntity> m_Unit1;

	[JsonProperty]
	[OwlPackInclude]
	private readonly EntityRef<BaseUnitEntity> m_Unit2;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "SwitchPartyCharactersGameCommand",
		OldNames = null,
		Fields = new FieldInfo[4]
		{
			new FieldInfo("m_Index1", typeof(sbyte)),
			new FieldInfo("m_Index2", typeof(sbyte)),
			new FieldInfo("m_Unit1", typeof(EntityRef<BaseUnitEntity>)),
			new FieldInfo("m_Unit2", typeof(EntityRef<BaseUnitEntity>))
		}
	};

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private SwitchPartyCharactersGameCommand()
	{
	}

	private SwitchPartyCharactersGameCommand(EntityRef<BaseUnitEntity> m_unit1, EntityRef<BaseUnitEntity> m_unit2, sbyte m_index1, sbyte m_index2)
	{
		m_Unit1 = m_unit1;
		m_Unit2 = m_unit2;
		m_Index1 = m_index1;
		m_Index2 = m_index2;
	}

	public SwitchPartyCharactersGameCommand(BaseUnitEntity unit1, BaseUnitEntity unit2)
	{
		m_Unit1 = unit1;
		m_Unit2 = unit2;
		m_Index1 = (sbyte)Game.Instance.Player.PartyCharacters.FindIndex((UnitReference u) => u == unit1);
		m_Index2 = (sbyte)Game.Instance.Player.PartyCharacters.FindIndex((UnitReference u) => u == unit2);
	}

	protected override void ExecuteInternal()
	{
		BaseUnitEntity unit1 = m_Unit1.Entity;
		BaseUnitEntity unit2 = m_Unit2.Entity;
		if (unit1 != null && unit2 != null)
		{
			int num = Game.Instance.Player.PartyCharacters.FindIndex((UnitReference u) => u == unit1);
			int num2 = Game.Instance.Player.PartyCharacters.FindIndex((UnitReference u) => u == unit2);
			if (num == m_Index1 && num2 == m_Index2)
			{
				Game.Instance.Controllers.SelectionCharacter.SwitchCharacter(unit1, unit2);
			}
		}
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SwitchPartyCharactersGameCommand source = new SwitchPartyCharactersGameCommand();
		result = Unsafe.As<SwitchPartyCharactersGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SwitchPartyCharactersGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		sbyte value = m_Index1;
		formatter.UnmanagedField(0, "m_Index1", ref value, state);
		sbyte value2 = m_Index2;
		formatter.UnmanagedField(1, "m_Index2", ref value2, state);
		EntityRef<BaseUnitEntity> value3 = m_Unit1;
		formatter.Field(2, "m_Unit1", ref value3, state);
		EntityRef<BaseUnitEntity> value4 = m_Unit2;
		formatter.Field(3, "m_Unit2", ref value4, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SwitchPartyCharactersGameCommand>();
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
				Unsafe.AsRef(in m_Index1) = formatter.ReadUnmanaged<sbyte>(state);
				break;
			case 1:
				Unsafe.AsRef(in m_Index2) = formatter.ReadUnmanaged<sbyte>(state);
				break;
			case 2:
				Unsafe.AsRef(in m_Unit1) = formatter.ReadPackable<EntityRef<BaseUnitEntity>>(state);
				break;
			case 3:
				Unsafe.AsRef(in m_Unit2) = formatter.ReadPackable<EntityRef<BaseUnitEntity>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
