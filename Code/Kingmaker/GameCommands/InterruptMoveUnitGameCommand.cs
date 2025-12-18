using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class InterruptMoveUnitGameCommand : GameCommand, IOwlPackable<InterruptMoveUnitGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private readonly UnitReference m_UnitRef;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "InterruptMoveUnitGameCommand",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_UnitRef", typeof(UnitReference))
		}
	};

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private InterruptMoveUnitGameCommand(UnitReference m_unitRef)
	{
		m_UnitRef = m_unitRef;
	}

	private InterruptMoveUnitGameCommand(OwlPackConstructorParameter _)
	{
	}

	public InterruptMoveUnitGameCommand([NotNull] AbstractUnitEntity unit)
		: this(unit.FromAbstractUnitEntity())
	{
	}

	protected override void ExecuteInternal()
	{
		AbstractUnitEntity abstractUnitEntity = m_UnitRef.ToAbstractUnitEntity();
		if (abstractUnitEntity == null)
		{
			PFLog.GameCommands.Error("Unit '{0}' not found!", m_UnitRef.Id);
		}
		else
		{
			abstractUnitEntity.GetOptional<PartUnitCommands>()?.ForceInterruptMove();
		}
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		InterruptMoveUnitGameCommand source = new InterruptMoveUnitGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<InterruptMoveUnitGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<InterruptMoveUnitGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		UnitReference value = m_UnitRef;
		formatter.Field(0, "m_UnitRef", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<InterruptMoveUnitGameCommand>();
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
				Unsafe.AsRef(in m_UnitRef) = formatter.ReadPackable<UnitReference>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
