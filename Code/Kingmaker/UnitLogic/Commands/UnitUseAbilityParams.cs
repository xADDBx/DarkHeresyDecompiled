using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Code.Visual.Animation;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using OwlPack.Runtime;

namespace Kingmaker.UnitLogic.Commands;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitUseAbilityParams : UnitUseAbilityParamsAbstract<UnitUseAbility>, IOwlPackable<UnitUseAbilityParams>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitUseAbilityParams",
		OldNames = null,
		Fields = new FieldInfo[19]
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
			new FieldInfo("m_IgnoreCooldown", typeof(bool?)),
			new FieldInfo("DisableLog", typeof(bool)),
			new FieldInfo("HitPolicy", typeof(AttackHitPolicyType)),
			new FieldInfo("DamagePolicy", typeof(DamagePolicyType)),
			new FieldInfo("KillTarget", typeof(bool))
		}
	};

	protected UnitUseAbilityParams()
	{
	}

	[JsonConstructor]
	protected UnitUseAbilityParams(JsonConstructorMark _)
		: base(_)
	{
	}

	public UnitUseAbilityParams([NotNull] AbilityData ability, [NotNull] TargetWrapper target)
		: base(ability, target)
	{
	}

	public UnitUseAbilityParams Copy()
	{
		return new UnitUseAbilityParams(base.Ability, base.Target)
		{
			m_IgnoreCooldown = m_IgnoreCooldown,
			DisableLog = base.DisableLog,
			HitPolicy = base.HitPolicy,
			DamagePolicy = base.DamagePolicy,
			KillTarget = base.KillTarget,
			IgnoreAbilityUsingInThreateningArea = base.IgnoreAbilityUsingInThreateningArea,
			ParentContext = base.ParentContext,
			DisableCameraFollow = base.DisableCameraFollow,
			CustomAnimationOverride = base.CustomAnimationOverride
		};
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitUseAbilityParams source = new UnitUseAbilityParams();
		result = Unsafe.As<UnitUseAbilityParams, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitUseAbilityParams>(OwlPackTypeInfo);
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
		formatter.UnmanagedNullableField(14, "m_IgnoreCooldown", ref m_IgnoreCooldown, state);
		bool value6 = base.DisableLog;
		formatter.UnmanagedField(15, "DisableLog", ref value6, state);
		AttackHitPolicyType value7 = base.HitPolicy;
		formatter.EnumField(16, "HitPolicy", ref value7, state);
		DamagePolicyType value8 = base.DamagePolicy;
		formatter.EnumField(17, "DamagePolicy", ref value8, state);
		bool value9 = base.KillTarget;
		formatter.UnmanagedField(18, "KillTarget", ref value9, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitUseAbilityParams>();
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
				m_IgnoreCooldown = formatter.ReadNullableUnmanaged<bool>(state);
				break;
			case 15:
				base.DisableLog = formatter.ReadUnmanaged<bool>(state);
				break;
			case 16:
				base.HitPolicy = formatter.ReadEnum<AttackHitPolicyType>(state);
				break;
			case 17:
				base.DamagePolicy = formatter.ReadEnum<DamagePolicyType>(state);
				break;
			case 18:
				base.KillTarget = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
