using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Commands.Base;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public class StopUnitsGameCommand : GameCommand, IOwlPackable<StopUnitsGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private readonly EntityRef<BaseUnitEntity>[] m_Units;

	private static readonly Func<AbstractUnitCommand, bool> NotStarted = (AbstractUnitCommand unitCommand) => !unitCommand.IsStarted;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "StopUnitsGameCommand",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_Units", typeof(EntityRef<BaseUnitEntity>[]))
		}
	};

	public override bool IsSynchronized => true;

	public override bool IsForcedSynced => true;

	private StopUnitsGameCommand(EntityRef<BaseUnitEntity>[] m_units)
	{
		m_Units = m_units;
	}

	private StopUnitsGameCommand(OwlPackConstructorParameter _)
	{
	}

	public StopUnitsGameCommand(IList<BaseUnitEntity> units)
	{
		int count = units.Count;
		m_Units = new EntityRef<BaseUnitEntity>[count];
		for (int i = 0; i < count; i++)
		{
			m_Units[i] = units[i];
		}
	}

	protected override void ExecuteInternal()
	{
		EntityRef<BaseUnitEntity>[] units = m_Units;
		foreach (BaseUnitEntity baseUnitEntity in units)
		{
			if (baseUnitEntity != null)
			{
				baseUnitEntity.HoldState = false;
				baseUnitEntity.Commands.InterruptAll(NotStarted);
				baseUnitEntity.CombatState.LastTarget = null;
				baseUnitEntity.CombatState.ManualTarget = null;
			}
		}
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		StopUnitsGameCommand source = new StopUnitsGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<StopUnitsGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<StopUnitsGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		EntityRef<BaseUnitEntity>[] value = m_Units;
		formatter.Field(0, "m_Units", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<StopUnitsGameCommand>();
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
				Unsafe.AsRef(in m_Units) = formatter.ReadPackable<EntityRef<BaseUnitEntity>[]>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
