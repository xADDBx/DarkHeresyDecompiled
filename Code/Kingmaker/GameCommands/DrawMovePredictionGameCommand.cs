using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands.Base;
using Newtonsoft.Json;
using OwlPack.Runtime;
using Pathfinding;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class DrawMovePredictionGameCommand : GameCommand, IOwlPackable<DrawMovePredictionGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private readonly EntityRef<BaseUnitEntity> m_Unit;

	[JsonProperty]
	[OwlPackInclude]
	private readonly ForcedPath m_Path;

	[JsonProperty]
	[OwlPackInclude]
	private readonly float[] m_CostPerEveryCell;

	[JsonProperty]
	[OwlPackInclude]
	private readonly UnitCommandParams m_UnitCommandParams;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "DrawMovePredictionGameCommand",
		OldNames = null,
		Fields = new FieldInfo[4]
		{
			new FieldInfo("m_Unit", typeof(EntityRef<BaseUnitEntity>)),
			new FieldInfo("m_Path", typeof(ForcedPath)),
			new FieldInfo("m_CostPerEveryCell", typeof(float[])),
			new FieldInfo("m_UnitCommandParams", typeof(UnitCommandParams))
		}
	};

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private DrawMovePredictionGameCommand()
	{
	}

	private DrawMovePredictionGameCommand(EntityRef<BaseUnitEntity> unit, [NotNull] Path path, [CanBeNull] float[] m_costPerEveryCell, [CanBeNull] UnitCommandParams m_unitCommandParams)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		m_Unit = unit;
		m_Path = ForcedPath.Construct(path);
		m_CostPerEveryCell = m_costPerEveryCell;
		m_UnitCommandParams = m_unitCommandParams;
		m_Path.Claim(this);
	}

	public DrawMovePredictionGameCommand([NotNull] BaseUnitEntity unit, [NotNull] Path path, [CanBeNull] float[] costPerEveryCell, [CanBeNull] UnitCommandParams unitCommandParams)
		: this((EntityRef<BaseUnitEntity>)unit, path, costPerEveryCell, unitCommandParams)
	{
		if (unit == null)
		{
			throw new ArgumentNullException("unit");
		}
	}

	protected override void ExecuteInternal()
	{
		BaseUnitEntity baseUnitEntity = m_Unit;
		if (baseUnitEntity == null)
		{
			PFLog.GameCommands.Error("Unit #" + m_Unit.Id + " not found!");
			return;
		}
		UnitHelper.DrawMovePredictionLocal(baseUnitEntity, m_Path, m_CostPerEveryCell);
		m_Path.Release(this);
		if (m_UnitCommandParams != null)
		{
			UnitCommandsRunner.SetVirtualMoveCommand(baseUnitEntity, m_UnitCommandParams);
		}
	}

	public override void AfterDeserialization()
	{
		base.AfterDeserialization();
		m_UnitCommandParams?.AfterDeserialization();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		DrawMovePredictionGameCommand source = new DrawMovePredictionGameCommand();
		result = Unsafe.As<DrawMovePredictionGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<DrawMovePredictionGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		EntityRef<BaseUnitEntity> value = m_Unit;
		formatter.Field(0, "m_Unit", ref value, state);
		ForcedPath value2 = m_Path;
		formatter.Field(1, "m_Path", ref value2, state);
		float[] value3 = m_CostPerEveryCell;
		formatter.Field(2, "m_CostPerEveryCell", ref value3, state);
		UnitCommandParams value4 = m_UnitCommandParams;
		formatter.Field(3, "m_UnitCommandParams", ref value4, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<DrawMovePredictionGameCommand>();
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
				Unsafe.AsRef(in m_Path) = formatter.ReadPackable<ForcedPath>(state);
				break;
			case 2:
				Unsafe.AsRef(in m_CostPerEveryCell) = formatter.ReadPackable<float[]>(state);
				break;
			case 3:
				Unsafe.AsRef(in m_UnitCommandParams) = formatter.ReadPackable<UnitCommandParams>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
