using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Newtonsoft.Json;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class PartyFormationOffsetGameCommand : GameCommand, IOwlPackable<PartyFormationOffsetGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private readonly int m_FormationIndex;

	[JsonProperty]
	[OwlPackInclude]
	private readonly int m_Index;

	[JsonProperty]
	[OwlPackInclude]
	private readonly UnitReference m_Unit;

	[JsonProperty]
	[OwlPackInclude]
	private readonly Vector2 m_Vector;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartyFormationOffsetGameCommand",
		OldNames = null,
		Fields = new FieldInfo[4]
		{
			new FieldInfo("m_FormationIndex", typeof(int)),
			new FieldInfo("m_Index", typeof(int)),
			new FieldInfo("m_Unit", typeof(UnitReference)),
			new FieldInfo("m_Vector", typeof(Vector2))
		}
	};

	public override bool IsSynchronized => true;

	public override bool IsForcedSynced => true;

	[JsonConstructor]
	private PartyFormationOffsetGameCommand()
	{
	}

	public PartyFormationOffsetGameCommand(int m_formationIndex, int m_index, BaseUnitEntity m_unit, Vector2 m_vector)
	{
		m_FormationIndex = m_formationIndex;
		m_Index = m_index;
		m_Unit = m_unit.FromBaseUnitEntity();
		m_Vector = m_vector;
	}

	public PartyFormationOffsetGameCommand(int m_formationIndex, int m_index, UnitReference m_unit, Vector2 m_vector)
	{
		m_FormationIndex = m_formationIndex;
		m_Index = m_index;
		m_Unit = m_unit;
		m_Vector = m_vector;
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.Player.FormationManager.SetOffset(m_FormationIndex, m_Index, m_Unit.ToBaseUnitEntity(), m_Vector);
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartyFormationOffsetGameCommand source = new PartyFormationOffsetGameCommand();
		result = Unsafe.As<PartyFormationOffsetGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartyFormationOffsetGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		int value = m_FormationIndex;
		formatter.UnmanagedField(0, "m_FormationIndex", ref value, state);
		int value2 = m_Index;
		formatter.UnmanagedField(1, "m_Index", ref value2, state);
		UnitReference value3 = m_Unit;
		formatter.Field(2, "m_Unit", ref value3, state);
		Vector2 value4 = m_Vector;
		formatter.Field(3, "m_Vector", ref value4, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartyFormationOffsetGameCommand>();
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
				Unsafe.AsRef(in m_Index) = formatter.ReadUnmanaged<int>(state);
				break;
			case 2:
				Unsafe.AsRef(in m_Unit) = formatter.ReadPackable<UnitReference>(state);
				break;
			case 3:
				Unsafe.AsRef(in m_Vector) = formatter.ReadPackable<Vector2>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
