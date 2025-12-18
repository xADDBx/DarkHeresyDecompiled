using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class SwitchHandEquipmentGameCommand : GameCommand, IOwlPackable<SwitchHandEquipmentGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private readonly EntityRef<BaseUnitEntity> m_Unit;

	[JsonProperty]
	[OwlPackInclude]
	private readonly sbyte m_HandEquipmentSetIndex;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "SwitchHandEquipmentGameCommand",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_Unit", typeof(EntityRef<BaseUnitEntity>)),
			new FieldInfo("m_HandEquipmentSetIndex", typeof(sbyte))
		}
	};

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private SwitchHandEquipmentGameCommand()
	{
	}

	private SwitchHandEquipmentGameCommand(EntityRef<BaseUnitEntity> m_unit, sbyte m_handEquipmentSetIndex)
	{
		m_Unit = m_unit;
		m_HandEquipmentSetIndex = m_handEquipmentSetIndex;
	}

	public SwitchHandEquipmentGameCommand([NotNull] BaseUnitEntity unit, int handEquipmentSetIndex)
		: this((EntityRef<BaseUnitEntity>)unit, (sbyte)handEquipmentSetIndex)
	{
		if (unit == null)
		{
			throw new NullReferenceException("unit");
		}
		if (handEquipmentSetIndex < -128 || 127 < handEquipmentSetIndex)
		{
			throw new ArgumentOutOfRangeException($"handEquipmentSetIndex={handEquipmentSetIndex}");
		}
	}

	protected override void ExecuteInternal()
	{
		BaseUnitEntity entity = m_Unit.Entity;
		if (entity == null)
		{
			PFLog.GameCommands.Error("Unit #" + m_Unit.Id + " not found!");
		}
		else
		{
			entity.Body.CurrentHandEquipmentSetIndex = m_HandEquipmentSetIndex;
		}
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SwitchHandEquipmentGameCommand source = new SwitchHandEquipmentGameCommand();
		result = Unsafe.As<SwitchHandEquipmentGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SwitchHandEquipmentGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		EntityRef<BaseUnitEntity> value = m_Unit;
		formatter.Field(0, "m_Unit", ref value, state);
		sbyte value2 = m_HandEquipmentSetIndex;
		formatter.UnmanagedField(1, "m_HandEquipmentSetIndex", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SwitchHandEquipmentGameCommand>();
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
				Unsafe.AsRef(in m_Unit) = formatter.ReadPackable<EntityRef<BaseUnitEntity>>(state);
				break;
			case 1:
				Unsafe.AsRef(in m_HandEquipmentSetIndex) = formatter.ReadUnmanaged<sbyte>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
