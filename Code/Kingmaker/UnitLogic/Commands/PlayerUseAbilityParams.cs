using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Code.Visual.Animation;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Code.Framework;
using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Gameplay.Parts;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.UnitLogic.Commands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class PlayerUseAbilityParams : UnitUseAbilityParams, IOwlPackable<PlayerUseAbilityParams>
{
	[JsonProperty]
	[OwlPackInclude]
	private string m_AbilityId;

	[JsonProperty]
	[OwlPackInclude]
	private string m_VariantId;

	public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PlayerUseAbilityParams",
		OldNames = null,
		Fields = new FieldInfo[21]
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
			new FieldInfo("KillTarget", typeof(bool)),
			new FieldInfo("m_AbilityId", typeof(string)),
			new FieldInfo("m_VariantId", typeof(string))
		}
	};

	protected override bool DefaultIsOneFrameCommand
	{
		get
		{
			bool flag = base.Ability?.AbilityGroups != null && base.Ability.AbilityGroups.Any((BlueprintAbilityGroup g) => g?.IsWeaponAttackGroup ?? false);
			if (Game.Instance.Player.UISettings.FastPartyCast && Game.Instance.Player.IsInCombat)
			{
				return !flag;
			}
			return false;
		}
	}

	private PlayerUseAbilityParams()
	{
	}

	[JsonConstructor]
	public PlayerUseAbilityParams(JsonConstructorMark _)
		: base(_)
	{
	}

	public PlayerUseAbilityParams([NotNull] AbilityData ability, [NotNull] TargetWrapper target)
		: base(ability, target)
	{
		if (ability.ConvertedFrom != null)
		{
			m_AbilityId = ability.ConvertedFrom.UniqueId;
			m_VariantId = ability.UniqueId;
		}
		else
		{
			m_AbilityId = ability.UniqueId;
			m_VariantId = null;
		}
	}

	public void Prepare()
	{
		AbilityData abilityData = LinksHelper.GetPartyAbility(m_AbilityId);
		if (abilityData == null)
		{
			throw new Exception("Can't find ability by id " + m_AbilityId);
		}
		if (m_VariantId != null)
		{
			abilityData = abilityData.GetConversions().FirstOrDefault((AbilityData a) => a.UniqueId.Equals(m_VariantId, StringComparison.Ordinal));
			if (abilityData == null)
			{
				throw new Exception("Can't find ability variants by AbilityId=" + m_AbilityId + " VariantId=" + m_VariantId);
			}
		}
		base.Ability = abilityData;
		AbilityUsageMetricsEvent abilityUsageMetricsEvent = Metrics.Ability.Id(abilityData.Blueprint.AssetGuid).Caster(abilityData.Caster.Blueprint.AssetGuid);
		PartAbilityModifiers optional = abilityData.Caster.Parts.GetOptional<PartAbilityModifiers>();
		abilityUsageMetricsEvent.Modifiers((optional != null) ? (from m in optional.AddedModifiers
			where m.IsAddedManually
			select m.Modifier.AssetGuid) : null).Toggles((from t in (abilityData.Caster as BaseUnitEntity)?.ToggleAbilities.Visible
			where t.Enabled
			select t.Blueprint.AssetGuid)).Send();
	}

	protected override AbstractUnitCommand CreateCommandInternal()
	{
		return new PlayerUseAbility(this);
	}

	public new static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PlayerUseAbilityParams source = new PlayerUseAbilityParams();
		result = Unsafe.As<PlayerUseAbilityParams, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PlayerUseAbilityParams>(OwlPackTypeInfo);
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
		formatter.StringField(19, "m_AbilityId", ref m_AbilityId, state);
		formatter.StringField(20, "m_VariantId", ref m_VariantId, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PlayerUseAbilityParams>();
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
			case 19:
				m_AbilityId = formatter.ReadString(state);
				break;
			case 20:
				m_VariantId = formatter.ReadString(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
