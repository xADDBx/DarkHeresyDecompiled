using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Code.Visual.Animation;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Newtonsoft.Json;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.UnitLogic.Commands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class UnitMoveToParams : UnitCommandParams<UnitMoveTo>, IOwlPackable<UnitMoveToParams>
{
	public const float DefaultApproachRadiusForAgent = 0.3f;

	[JsonProperty(PropertyName = "ara", DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public float ApproachRadiusForAgentASP;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitMoveToParams",
		OldNames = null,
		Fields = new FieldInfo[18]
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
			new FieldInfo("ApproachRadiusForAgentASP", typeof(float)),
			new FieldInfo("Orientation", typeof(float?)),
			new FieldInfo("RunAway", typeof(bool)),
			new FieldInfo("Roaming", typeof(bool))
		}
	};

	[JsonProperty(PropertyName = "ori")]
	[OwlPackInclude]
	public float? Orientation { get; set; }

	[JsonProperty(PropertyName = "raw")]
	[OwlPackInclude]
	public bool RunAway { get; set; }

	[JsonProperty(PropertyName = "roa")]
	[OwlPackInclude]
	public bool Roaming { get; set; }

	public float MaxApproachForAgentASP => Mathf.Max(Mathf.Sqrt(2.Cells().Meters), ApproachRadiusForAgentASP);

	public override int DefaultApproachRadius => 10000;

	public override bool TryMergeInto(AbstractUnitCommand currentCommand)
	{
		if (base.FromCutscene)
		{
			return false;
		}
		if (!(currentCommand is UnitMoveTo unitMoveTo))
		{
			return false;
		}
		if (currentCommand.Executor.MovementAgent.NodeLinkTraverser.IsTraverseNow)
		{
			return false;
		}
		unitMoveTo.Params.ForcedPath = base.ForcedPath;
		unitMoveTo.Params.Target = base.Target;
		unitMoveTo.Params.ApproachRadiusForAgentASP = ApproachRadiusForAgentASP;
		unitMoveTo.Params.Orientation = Orientation;
		unitMoveTo.Params.RunAway = RunAway;
		unitMoveTo.Params.Roaming = Roaming;
		unitMoveTo.Params.OverrideSpeed = base.OverrideSpeed;
		unitMoveTo.Params.MovementType = base.MovementType;
		unitMoveTo.Executor.View.MoveTo(base.ForcedPath, base.Target.Point, ApproachRadiusForAgentASP);
		return true;
	}

	[JsonConstructor]
	private UnitMoveToParams()
		: base(default(JsonConstructorMark))
	{
	}

	public UnitMoveToParams([NotNull] ForcedPath path, [CanBeNull] TargetWrapper target, float approachRadiusForAgent = 0.3f)
		: base(target)
	{
		base.ForcedPath = path;
		ApproachRadiusForAgentASP = approachRadiusForAgent;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitMoveToParams source = new UnitMoveToParams();
		result = Unsafe.As<UnitMoveToParams, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitMoveToParams>(OwlPackTypeInfo);
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
		formatter.UnmanagedField(14, "ApproachRadiusForAgentASP", ref ApproachRadiusForAgentASP, state);
		float? value6 = Orientation;
		formatter.UnmanagedNullableField(15, "Orientation", ref value6, state);
		bool value7 = RunAway;
		formatter.UnmanagedField(16, "RunAway", ref value7, state);
		bool value8 = Roaming;
		formatter.UnmanagedField(17, "Roaming", ref value8, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitMoveToParams>();
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
				ApproachRadiusForAgentASP = formatter.ReadUnmanaged<float>(state);
				break;
			case 15:
				Orientation = formatter.ReadNullableUnmanaged<float>(state);
				break;
			case 16:
				RunAway = formatter.ReadUnmanaged<bool>(state);
				break;
			case 17:
				Roaming = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
