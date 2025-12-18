using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.ActivatableAbilities;

[Obsolete]
[OwlPackable(OwlPackableMode.Generate)]
public sealed class ActivatableAbility : UnitFact<BlueprintActivatableAbility>, IUnitCommandActHandler, ISubscriber<IMechanicEntity>, ISubscriber, IUnitCommandEndHandler, IUnitRunCommandHandler, IInitiatorRulebookHandler<RulePerformAbility>, IRulebookHandler<RulePerformAbility>, IInitiatorRulebookSubscriber, IEntitySubscriber, IUnitCombatHandler<EntitySubscriber>, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, IEventTag<IUnitCombatHandler, EntitySubscriber>, IUnitBuffHandler, IApplyAbilityEffectHandler, IHashable, IOwlPackable<ActivatableAbility>
{
	[JsonProperty]
	[OwlPackInclude]
	private bool m_IsOn;

	[JsonProperty]
	[CanBeNull]
	[OwlPackInclude]
	private Buff m_AppliedBuff;

	[JsonProperty]
	[OwlPackInclude]
	private TargetWrapper m_Target;

	[JsonProperty]
	[OwlPackInclude]
	private TimeSpan m_TurnOnTime;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_WasInCombat;

	private bool m_ShouldBeDeactivatedInNextRound;

	private BlueprintComponentAndRuntime<ActivatableAbilityResourceLogic>[] m_CachedResourceLogic;

	private BlueprintComponentAndRuntime<ActivatableAbilityRestriction>[] m_CachedRestrictions;

	public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ActivatableAbility",
		OldNames = null,
		Fields = new FieldInfo[19]
		{
			new FieldInfo("m_ComponentsData", typeof(Dictionary<string, List<IEntityFactComponentSavableData>>)),
			new FieldInfo("m_Components", typeof(List<EntityFactComponent>)),
			new FieldInfo("m_Sources", typeof(List<EntityFactSource>)),
			new FieldInfo("m_ChildrenFacts", typeof(List<EntityFactRef>)),
			new FieldInfo("UniqueId", typeof(string)),
			new FieldInfo("m_Blueprint", typeof(BlueprintFact)),
			new FieldInfo("IsActive", typeof(bool)),
			new FieldInfo("ChildOf", typeof(EntityFactRef)),
			new FieldInfo("m_ParentContext", typeof(MechanicsContext)),
			new FieldInfo("m_Context", typeof(MechanicsContext)),
			new FieldInfo("TimeToNextRound", typeof(float)),
			new FieldInfo("m_IsOn", typeof(bool)),
			new FieldInfo("m_AppliedBuff", typeof(Buff)),
			new FieldInfo("IsStarted", typeof(bool)),
			new FieldInfo("ReadyToStart", typeof(bool)),
			new FieldInfo("m_Target", typeof(TargetWrapper)),
			new FieldInfo("m_TurnOnTime", typeof(TimeSpan)),
			new FieldInfo("m_WasInCombat", typeof(bool)),
			new FieldInfo("SelectTargetAbility", typeof(Ability))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public float TimeToNextRound { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool IsStarted { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool ReadyToStart { get; private set; }

	[JsonProperty]
	[CanBeNull]
	[OwlPackInclude]
	public Ability SelectTargetAbility { get; private set; }

	public bool IsOn
	{
		get
		{
			return m_IsOn;
		}
		set
		{
			SetIsOn(value, null);
		}
	}

	[CanBeNull]
	public Buff AppliedBuff => m_AppliedBuff;

	public bool IsAvailableByResources
	{
		get
		{
			m_CachedResourceLogic = m_CachedResourceLogic ?? SelectComponentsWithRuntime<ActivatableAbilityResourceLogic>().ToArray();
			return !m_CachedResourceLogic.HasItem((BlueprintComponentAndRuntime<ActivatableAbilityResourceLogic> i) => !i.Component.IsAvailable(i.Runtime));
		}
	}

	public bool IsAvailableByRestrictions
	{
		get
		{
			if (base.Blueprint.OnlyInCombat && !base.Owner.IsInCombat)
			{
				return false;
			}
			m_CachedRestrictions = m_CachedRestrictions ?? SelectComponentsWithRuntime<ActivatableAbilityRestriction>().ToArray();
			return !m_CachedRestrictions.HasItem((BlueprintComponentAndRuntime<ActivatableAbilityRestriction> i) => !i.Component.IsAvailable(i.Runtime));
		}
	}

	public int? ResourceCount
	{
		get
		{
			m_CachedResourceLogic = m_CachedResourceLogic ?? SelectComponentsWithRuntime<ActivatableAbilityResourceLogic>().ToArray();
			ActivatableAbilityResourceLogic component = m_CachedResourceLogic.FirstItem().Component;
			BlueprintAbilityResource blueprintAbilityResource = ((component != null && component.SpendType != 0) ? component.RequiredResource : null);
			int num = (blueprintAbilityResource ? base.Owner.AbilityResources.GetResourceAmount(blueprintAbilityResource) : ((base.SourceItem == null) ? (-1) : (base.SourceItem.IsSpendCharges ? base.SourceItem.Charges : (-1))));
			if (num >= 0)
			{
				return num;
			}
			return null;
		}
	}

	public bool IsAvailable
	{
		get
		{
			if (IsAvailableByResources)
			{
				return IsAvailableByRestrictions;
			}
			return false;
		}
	}

	public bool IsWaitingForTarget
	{
		get
		{
			if (base.Blueprint.IsTargeted && IsOn)
			{
				return m_Target == null;
			}
			return false;
		}
	}

	public ActivatableAbility(BlueprintActivatableAbility blueprint)
		: base(blueprint, (MechanicsContext)null)
	{
	}

	[JsonConstructor]
	private ActivatableAbility(JsonConstructorMark _)
	{
	}

	protected ActivatableAbility()
	{
	}

	private void SetIsOn(bool value, [CanBeNull] BaseUnitEntity target)
	{
		if (m_IsOn != value)
		{
			m_IsOn = value;
			m_TurnOnTime = Game.Instance.Controllers.TimeController.RealTime;
			if (IsWaitingForTarget)
			{
				m_Target = target;
			}
			else if (target != null)
			{
				PFLog.Default.Error("ActivatableAbility.SetIsOn: !IsWaitingForTarget && target != null");
			}
			if (m_IsOn)
			{
				OnDidTurnOn();
			}
			else
			{
				OnDidTurnOff();
			}
			if (IsWaitingForTarget && SelectTargetAbility != null && base.Owner.IsDirectlyControllable)
			{
				Game.Instance.Controllers.SelectedAbilityHandler.SetAbility(SelectTargetAbility.Data);
			}
		}
	}

	public void TurnOffImmediately()
	{
		SetIsOn(value: false, null);
	}

	public void SetIsOnWithTarget(bool value, [NotNull] BaseUnitEntity target)
	{
		SetIsOn(value, target);
	}

	protected override void OnActivate()
	{
		base.OnActivate();
		if (base.Blueprint.IsTargeted)
		{
			SelectTargetAbility = base.Owner.Facts.Add(new Ability(base.Blueprint.SelectTargetAbility));
		}
	}

	protected override void OnDeactivate()
	{
		if (SelectTargetAbility != null)
		{
			base.Owner.Facts.Remove(SelectTargetAbility);
			SelectTargetAbility = null;
		}
		Stop();
		base.OnDeactivate();
	}

	public void OnNewRound()
	{
		m_WasInCombat |= base.Owner.IsInCombat;
		if (m_ShouldBeDeactivatedInNextRound || !IsOn || !IsAvailable || (base.Blueprint.DeactivateIfCombatEnded && !base.Owner.IsInCombat && (base.Blueprint.ActivateOnCombatStarts || m_WasInCombat)))
		{
			Stop();
		}
		else
		{
			CallComponents(delegate(IActivatableAbilitySpendResourceLogic l)
			{
				l.OnNewRound();
			});
		}
		m_ShouldBeDeactivatedInNextRound = base.Blueprint.DeactivateAfterFirstRound;
	}

	public void HandleUnitJoinCombat()
	{
		if (base.Blueprint.ActivateOnCombatStarts && m_IsOn && !IsStarted && base.Blueprint.ActivateImmediately)
		{
			TryStart();
		}
	}

	public void HandleUnitLeaveCombat()
	{
		if (base.Blueprint.DeactivateIfCombatEnded && IsStarted && base.Blueprint.DeactivateImmediately)
		{
			Stop();
		}
	}

	public void HandleUnitRunCommand(AbstractUnitCommand command)
	{
	}

	public void HandleUnitCommandDidAct(AbstractUnitCommand command)
	{
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
	}

	public void TryStart()
	{
		if (IsStarted || IsWaitingForTarget || !IsAvailable)
		{
			return;
		}
		ReadyToStart = true;
		if (!base.Blueprint.ActivateOnCombatStarts || base.Owner.IsInCombat)
		{
			_ = base.Blueprint.Group;
			m_WasInCombat = base.Owner.IsInCombat;
			IsStarted = true;
			ReapplyBuff();
			CallComponents(delegate(IActivatableAbilitySpendResourceLogic l)
			{
				l.OnStart();
			});
			TimeToNextRound = 0f;
			ReadyToStart = false;
		}
	}

	public void Stop(bool forceRemovedBuff = false)
	{
		if (IsStarted)
		{
			EventBus.RaiseEvent(delegate(IActivatableAbilityWillStopHandler h)
			{
				h.HandleActivatableAbilityWillStop(this);
			});
			m_WasInCombat = false;
			IsStarted = false;
			TimeToNextRound = 0f;
			m_AppliedBuff?.Remove();
			m_AppliedBuff = null;
			if (m_IsOn && !IsAvailableByRestrictions)
			{
				m_IsOn = false;
				m_TurnOnTime = Game.Instance.Controllers.TimeController.RealTime;
			}
		}
	}

	public void ReapplyBuff()
	{
	}

	public void OnEventAboutToTrigger(RulePerformAbility evt)
	{
		if (m_IsOn && base.Blueprint.ActivateOnUnitAction && base.Blueprint.ActivateOnUnitActionType == AbilityActivateOnUnitActionType.CastSpell)
		{
			TryStart();
		}
	}

	public void OnEventDidTrigger(RulePerformAbility evt)
	{
	}

	private void OnAttack(BlueprintItemWeapon weapon)
	{
		if (m_IsOn && IsStarted && IsAvailable)
		{
			CallComponents(delegate(IActivatableAbilitySpendResourceLogic l)
			{
				l.OnAttack(weapon);
			});
			if (!IsAvailable && base.Blueprint.DeactivateImmediately)
			{
				Stop();
			}
		}
	}

	private void OnHit()
	{
		if (m_IsOn && IsStarted && IsAvailable)
		{
			CallComponents(delegate(IActivatableAbilitySpendResourceLogic l)
			{
				l.OnHit();
			});
			if (!IsAvailable && base.Blueprint.DeactivateImmediately)
			{
				Stop();
			}
		}
	}

	private void OnCrit()
	{
		if (m_IsOn && IsStarted && IsAvailable)
		{
			CallComponents(delegate(IActivatableAbilitySpendResourceLogic l)
			{
				l.OnCrit();
			});
			if (!IsAvailable && base.Blueprint.DeactivateImmediately)
			{
				Stop();
			}
		}
	}

	public void HandleBuffDidAdded(Buff buff, MechanicEntity caster)
	{
	}

	public void HandleBuffDidRemoved(Buff buff, MechanicEntity caster)
	{
		if (m_AppliedBuff == buff && IsStarted)
		{
			IsOn = false;
			if (IsStarted)
			{
				Stop();
			}
			m_ShouldBeDeactivatedInNextRound = false;
		}
	}

	public void HandleBuffRankIncreased(Buff buff, int delta, MechanicEntity caster)
	{
	}

	public void HandleBuffRankDecreased(Buff buff, int delta, MechanicEntity caster)
	{
	}

	private void OnDidTurnOn()
	{
		if (IsWaitingForTarget)
		{
			return;
		}
		_ = base.Blueprint.Group;
		if (!IsStarted)
		{
			CallComponents(delegate(IActivatableAbilitySpendResourceLogic l)
			{
				l.OnAbilityTurnOn();
			});
			if (base.Blueprint.ActivateImmediately)
			{
				TryStart();
			}
		}
	}

	public bool IsActivateWithSameCommand(ActivatableAbility other)
	{
		if (base.Blueprint.Group == ActivatableAbilityGroup.Judgment)
		{
			return base.Blueprint.Group == other.Blueprint.Group;
		}
		return false;
	}

	private void OnDidTurnOff()
	{
		m_Target = null;
		ReadyToStart = false;
		if (IsStarted && base.Blueprint.DeactivateImmediately)
		{
			Stop();
		}
	}

	public void TurnOnWithTarget([NotNull] MechanicEntity target)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		if (!IsWaitingForTarget)
		{
			throw new InvalidOperationException("Activatable ability is not waiting for target");
		}
		m_Target = target;
		OnDidTurnOn();
	}

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		if (m_IsOn && IsStarted && ((m_AppliedBuff != null && !m_AppliedBuff.Active && !m_AppliedBuff.IsSuppressed) || m_AppliedBuff?.Blueprint != base.Blueprint.Buff))
		{
			ReapplyBuff();
		}
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		m_AppliedBuff = null;
		m_Target = null;
		m_CachedResourceLogic = null;
		m_CachedRestrictions = null;
	}

	public void OnAbilityEffectApplied(AbilityExecutionContext context)
	{
	}

	public void OnTryToApplyAbilityEffect(AbilityExecutionContext context, AbilityDeliveryTarget target)
	{
	}

	public void OnAbilityEffectAppliedToTarget(AbilityExecutionContext context, AbilityDeliveryTarget target)
	{
		if (IsWaitingForTarget && context.Ability.Fact == SelectTargetAbility)
		{
			TurnOnWithTarget(target.Target.Entity);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		float val2 = TimeToNextRound;
		result.Append(ref val2);
		result.Append(ref m_IsOn);
		Hash128 val3 = ClassHasher<Buff>.GetHash128(m_AppliedBuff);
		result.Append(ref val3);
		bool val4 = IsStarted;
		result.Append(ref val4);
		bool val5 = ReadyToStart;
		result.Append(ref val5);
		Hash128 val6 = ClassHasher<TargetWrapper>.GetHash128(m_Target);
		result.Append(ref val6);
		result.Append(ref m_TurnOnTime);
		result.Append(ref m_WasInCombat);
		Hash128 val7 = ClassHasher<Ability>.GetHash128(SelectTargetAbility);
		result.Append(ref val7);
		return result;
	}

	public new static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		ActivatableAbility source = new ActivatableAbility();
		result = Unsafe.As<ActivatableAbility, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ActivatableAbility>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_ComponentsData", ref m_ComponentsData, state);
		List<EntityFactComponent> value = base.m_Components;
		formatter.Field(1, "m_Components", ref value, state);
		formatter.Field(2, "m_Sources", ref m_Sources, state);
		formatter.Field(3, "m_ChildrenFacts", ref m_ChildrenFacts, state);
		string value2 = base.UniqueId;
		formatter.StringField(4, "UniqueId", ref value2, state);
		formatter.Field(5, "m_Blueprint", ref m_Blueprint, state);
		bool value3 = base.IsActive;
		formatter.UnmanagedField(6, "IsActive", ref value3, state);
		EntityFactRef value4 = base.ChildOf;
		formatter.Field(7, "ChildOf", ref value4, state);
		formatter.Field(8, "m_ParentContext", ref m_ParentContext, state);
		formatter.Field(9, "m_Context", ref m_Context, state);
		float value5 = TimeToNextRound;
		formatter.UnmanagedField(10, "TimeToNextRound", ref value5, state);
		formatter.UnmanagedField(11, "m_IsOn", ref m_IsOn, state);
		formatter.Field(12, "m_AppliedBuff", ref m_AppliedBuff, state);
		bool value6 = IsStarted;
		formatter.UnmanagedField(13, "IsStarted", ref value6, state);
		bool value7 = ReadyToStart;
		formatter.UnmanagedField(14, "ReadyToStart", ref value7, state);
		formatter.Field(15, "m_Target", ref m_Target, state);
		formatter.Field(16, "m_TurnOnTime", ref m_TurnOnTime, state);
		formatter.UnmanagedField(17, "m_WasInCombat", ref m_WasInCombat, state);
		Ability value8 = SelectTargetAbility;
		formatter.Field(18, "SelectTargetAbility", ref value8, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ActivatableAbility>();
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
				m_ComponentsData = formatter.ReadPackable<Dictionary<string, List<IEntityFactComponentSavableData>>>(state);
				break;
			case 1:
				base.m_Components = formatter.ReadPackable<List<EntityFactComponent>>(state);
				break;
			case 2:
				m_Sources = formatter.ReadPackable<List<EntityFactSource>>(state);
				break;
			case 3:
				m_ChildrenFacts = formatter.ReadPackable<List<EntityFactRef>>(state);
				break;
			case 4:
				base.UniqueId = formatter.ReadString(state);
				break;
			case 5:
				m_Blueprint = formatter.ReadPackable<BlueprintFact>(state);
				break;
			case 6:
				base.IsActive = formatter.ReadUnmanaged<bool>(state);
				break;
			case 7:
				base.ChildOf = formatter.ReadPackable<EntityFactRef>(state);
				break;
			case 8:
				m_ParentContext = formatter.ReadPackable<MechanicsContext>(state);
				break;
			case 9:
				m_Context = formatter.ReadPackable<MechanicsContext>(state);
				break;
			case 10:
				TimeToNextRound = formatter.ReadUnmanaged<float>(state);
				break;
			case 11:
				m_IsOn = formatter.ReadUnmanaged<bool>(state);
				break;
			case 12:
				m_AppliedBuff = formatter.ReadPackable<Buff>(state);
				break;
			case 13:
				IsStarted = formatter.ReadUnmanaged<bool>(state);
				break;
			case 14:
				ReadyToStart = formatter.ReadUnmanaged<bool>(state);
				break;
			case 15:
				m_Target = formatter.ReadPackable<TargetWrapper>(state);
				break;
			case 16:
				m_TurnOnTime = formatter.ReadPackable<TimeSpan>(state);
				break;
			case 17:
				m_WasInCombat = formatter.ReadUnmanaged<bool>(state);
				break;
			case 18:
				SelectTargetAbility = formatter.ReadPackable<Ability>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
