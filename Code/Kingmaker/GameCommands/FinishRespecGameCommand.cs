using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class FinishRespecGameCommand : GameCommand, IOwlPackable<FinishRespecGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private readonly EntityRef<BaseUnitEntity> m_RespecEntity;

	[JsonProperty]
	[OwlPackInclude]
	private readonly bool m_ForFree;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "FinishRespecGameCommand",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_RespecEntity", typeof(EntityRef<BaseUnitEntity>)),
			new FieldInfo("m_ForFree", typeof(bool))
		}
	};

	public override bool IsSynchronized => true;

	[JsonConstructor]
	public FinishRespecGameCommand()
	{
	}

	private FinishRespecGameCommand(EntityRef<BaseUnitEntity> m_respecEntity, bool m_forfree)
	{
		m_RespecEntity = m_respecEntity;
		m_ForFree = m_forfree;
	}

	public FinishRespecGameCommand(BaseUnitEntity respecEntity, bool forFree)
		: this((EntityRef<BaseUnitEntity>)respecEntity, forFree)
	{
	}

	protected override void ExecuteInternal()
	{
		_ = Game.Instance.Player;
		PartUnitProgression progression = m_RespecEntity.Entity.Progression;
		progression.Respec();
		if (!m_ForFree)
		{
			progression.CountRespecIn();
		}
		Game.Instance.AdvanceGameTime(1.Days());
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		FinishRespecGameCommand source = new FinishRespecGameCommand();
		result = Unsafe.As<FinishRespecGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<FinishRespecGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		EntityRef<BaseUnitEntity> value = m_RespecEntity;
		formatter.Field(0, "m_RespecEntity", ref value, state);
		bool value2 = m_ForFree;
		formatter.UnmanagedField(1, "m_ForFree", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<FinishRespecGameCommand>();
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
				Unsafe.AsRef(in m_RespecEntity) = formatter.ReadPackable<EntityRef<BaseUnitEntity>>(state);
				break;
			case 1:
				Unsafe.AsRef(in m_ForFree) = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
