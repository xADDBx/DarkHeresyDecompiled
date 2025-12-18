using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Code.Visual.Animation;
using JetBrains.Annotations;
using Kingmaker.Code.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.UnitLogic.Commands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class UnitDirectInteractParams : UnitCommandParams<UnitDirectInteract>, IOwlPackable<UnitDirectInteractParams>
{
	[JsonProperty]
	[OwlPackInclude]
	private ViewBasedPartRef<MapObjectEntity, AbstractInteractionPart> m_Interaction;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitDirectInteractParams",
		OldNames = null,
		Fields = new FieldInfo[15]
		{
			new FieldInfo("Type", typeof(CommandType)),
			new FieldInfo("OwnerRef", typeof(EntityRef<BaseUnitEntity>)),
			new FieldInfo("Target", typeof(TargetWrapper)),
			new FieldInfo("FromCutscene", typeof(bool)),
			new FieldInfo("InterruptAsSoonAsPossible", typeof(bool)),
			new FieldInfo("OverrideSpeed", typeof(float?)),
			new FieldInfo("DoNotInterruptAfterFight", typeof(bool)),
			new FieldInfo("m_FreeAction", typeof(bool?)),
			new FieldInfo("m_NeedLoS", typeof(bool?)),
			new FieldInfo("m_ApproachRadius", typeof(int?)),
			new FieldInfo("m_ForcedPath", typeof(ForcedPath)),
			new FieldInfo("m_MovementType", typeof(WalkSpeedType?)),
			new FieldInfo("m_IsOneFrameCommand", typeof(bool?)),
			new FieldInfo("m_SlowMotionRequired", typeof(bool?)),
			new FieldInfo("m_Interaction", typeof(ViewBasedPartRef<MapObjectEntity, AbstractInteractionPart>))
		}
	};

	public AbstractInteractionPart Interaction => m_Interaction.EntityPart;

	[JsonConstructor]
	private UnitDirectInteractParams()
		: base(default(JsonConstructorMark))
	{
	}

	public UnitDirectInteractParams([NotNull] AbstractInteractionPart interaction)
		: base((TargetWrapper)interaction.Owner)
	{
		if (interaction.Type == InteractionType.Approach)
		{
			throw new Exception("DirectInteract supports only Direct or Flashlight interaction");
		}
		m_Interaction = new ViewBasedPartRef<MapObjectEntity, AbstractInteractionPart>(interaction, interaction.GetType());
	}

	public override bool TryMergeInto(AbstractUnitCommand currentCommand)
	{
		return (currentCommand as UnitDirectInteract)?.Interaction == Interaction;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitDirectInteractParams source = new UnitDirectInteractParams();
		result = Unsafe.As<UnitDirectInteractParams, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitDirectInteractParams>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EnumField(0, "Type", ref Type, state);
		formatter.Field(1, "OwnerRef", ref OwnerRef, state);
		TargetWrapper value = base.Target;
		formatter.Field(2, "Target", ref value, state);
		bool value2 = base.FromCutscene;
		formatter.UnmanagedField(3, "FromCutscene", ref value2, state);
		bool value3 = base.InterruptAsSoonAsPossible;
		formatter.UnmanagedField(4, "InterruptAsSoonAsPossible", ref value3, state);
		float? value4 = base.OverrideSpeed;
		formatter.UnmanagedNullableField(5, "OverrideSpeed", ref value4, state);
		bool value5 = base.DoNotInterruptAfterFight;
		formatter.UnmanagedField(6, "DoNotInterruptAfterFight", ref value5, state);
		formatter.UnmanagedNullableField(7, "m_FreeAction", ref m_FreeAction, state);
		formatter.UnmanagedNullableField(8, "m_NeedLoS", ref m_NeedLoS, state);
		formatter.UnmanagedNullableField(9, "m_ApproachRadius", ref m_ApproachRadius, state);
		formatter.Field(10, "m_ForcedPath", ref m_ForcedPath, state);
		formatter.EnumNullableField(11, "m_MovementType", ref m_MovementType, state);
		formatter.UnmanagedNullableField(12, "m_IsOneFrameCommand", ref m_IsOneFrameCommand, state);
		formatter.UnmanagedNullableField(13, "m_SlowMotionRequired", ref m_SlowMotionRequired, state);
		formatter.Field(14, "m_Interaction", ref m_Interaction, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitDirectInteractParams>();
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
				Type = formatter.ReadEnum<CommandType>(state);
				break;
			case 1:
				OwnerRef = formatter.ReadPackable<EntityRef<BaseUnitEntity>>(state);
				break;
			case 2:
				base.Target = formatter.ReadPackable<TargetWrapper>(state);
				break;
			case 3:
				base.FromCutscene = formatter.ReadUnmanaged<bool>(state);
				break;
			case 4:
				base.InterruptAsSoonAsPossible = formatter.ReadUnmanaged<bool>(state);
				break;
			case 5:
				base.OverrideSpeed = formatter.ReadNullableUnmanaged<float>(state);
				break;
			case 6:
				base.DoNotInterruptAfterFight = formatter.ReadUnmanaged<bool>(state);
				break;
			case 7:
				m_FreeAction = formatter.ReadNullableUnmanaged<bool>(state);
				break;
			case 8:
				m_NeedLoS = formatter.ReadNullableUnmanaged<bool>(state);
				break;
			case 9:
				m_ApproachRadius = formatter.ReadNullableUnmanaged<int>(state);
				break;
			case 10:
				m_ForcedPath = formatter.ReadPackable<ForcedPath>(state);
				break;
			case 11:
				m_MovementType = formatter.ReadNullableEnum<WalkSpeedType>(state);
				break;
			case 12:
				m_IsOneFrameCommand = formatter.ReadNullableUnmanaged<bool>(state);
				break;
			case 13:
				m_SlowMotionRequired = formatter.ReadNullableUnmanaged<bool>(state);
				break;
			case 14:
				m_Interaction = formatter.ReadPackable<ViewBasedPartRef<MapObjectEntity, AbstractInteractionPart>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
