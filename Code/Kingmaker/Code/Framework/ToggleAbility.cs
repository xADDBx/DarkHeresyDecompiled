using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework;
using Kingmaker.Gameplay.Parts;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Code.Framework;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class ToggleAbility : MechanicEntityFact, IEntityGainFactHandler<EntitySubscriber>, IEntityGainFactHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IEntityGainFactHandler, EntitySubscriber>, IEntityLostFactHandler<EntitySubscriber>, IEntityLostFactHandler, IEventTag<IEntityLostFactHandler, EntitySubscriber>, IUnitEquipmentHandler<EntitySubscriber>, IUnitEquipmentHandler, IEventTag<IUnitEquipmentHandler, EntitySubscriber>, IEntityDamageChanged<EntitySubscriber>, IEntityDamageChanged, IEventTag<IEntityDamageChanged, EntitySubscriber>, ITurnStartHandler<EntitySubscriber>, ITurnStartHandler, IEntitySubscriber, IEventTag<ITurnStartHandler, EntitySubscriber>, IHashable, IOwlPackable<ToggleAbility>
{
	[JsonProperty]
	[OwlPackInclude]
	private bool m_Enabled;

	[JsonProperty]
	[OwlPackInclude]
	private EntityFactRef<MechanicEntityFact> m_Fact;

	[JsonIgnore]
	private IAbilityCasterRestriction[]? m_Restrictions;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ToggleAbility",
		OldNames = null,
		Fields = new FieldInfo[12]
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
			new FieldInfo("m_Enabled", typeof(bool)),
			new FieldInfo("m_Fact", typeof(EntityFactRef<MechanicEntityFact>))
		}
	};

	private ReadonlyList<IAbilityCasterRestriction> Restrictions => m_Restrictions ?? (m_Restrictions = Blueprint.GetComponents<IAbilityCasterRestriction>().ToArray());

	public bool Enabled
	{
		get
		{
			return m_Enabled;
		}
		set
		{
			SetEnabled(value);
		}
	}

	public bool IsRestrictionsPassed
	{
		get
		{
			IAbilityCasterRestriction failedRestriction;
			return CheckRestrictionsPassed(out failedRestriction);
		}
	}

	public new BlueprintToggleAbility Blueprint => (BlueprintToggleAbility)base.Blueprint;

	public ToggleAbility(BlueprintToggleAbility blueprint)
		: base(blueprint, null)
	{
	}

	private ToggleAbility(OwlPackConstructorParameter _)
	{
	}

	protected override void OnApplyPostLoadFixes()
	{
		if (m_Enabled && m_Fact.Fact == null)
		{
			SetEnabled(value: true, force: true);
		}
	}

	public void SetEnabled(bool value)
	{
		SetEnabled(value, force: false);
	}

	private void SetEnabled(bool value, bool force)
	{
		if (m_Enabled != value || force)
		{
			m_Enabled = value;
			MechanicEntityFact fact = m_Fact.Fact;
			fact?.Owner.Facts.Remove(fact);
			m_Fact = default(EntityFactRef<MechanicEntityFact>);
			if (!m_Enabled || force)
			{
				base.Owner.GetOptional<PartAbilityModifiers>()?.RemoveModifiers(this);
			}
			if (m_Enabled)
			{
				DisableAbilitiesFromSameGroup();
				MechanicEntityFact mechanicEntityFact = (MechanicEntityFact)base.Owner.AddFact(Blueprint.Fact, base.Context);
				mechanicEntityFact?.AddSource(this);
				m_Fact = mechanicEntityFact;
				base.Owner.GetOptional<PartAbilityModifiers>()?.AddModifiers(this);
			}
		}
	}

	private void DisableAbilitiesFromSameGroup()
	{
		BlueprintToggleAbilityGroup maybeBlueprint = Blueprint.Group.MaybeBlueprint;
		if (maybeBlueprint == null)
		{
			return;
		}
		foreach (ToggleAbility item in base.Owner.Facts.GetAll<ToggleAbility>())
		{
			if (item != this && item.Blueprint.Group == maybeBlueprint)
			{
				item.SetEnabled(value: false);
			}
		}
	}

	public bool CheckRestrictionsPassed(out IAbilityCasterRestriction? failedRestriction)
	{
		foreach (IAbilityCasterRestriction restriction in Restrictions)
		{
			if (!restriction.IsCasterRestrictionPassed(base.Owner))
			{
				failedRestriction = restriction;
				return false;
			}
		}
		failedRestriction = null;
		return true;
	}

	private void UpdateIsEnabled()
	{
		if (Enabled && !IsRestrictionsPassed)
		{
			SetEnabled(value: false);
		}
	}

	void IEntityGainFactHandler.HandleEntityGainFact(EntityFact fact)
	{
		UpdateIsEnabled();
	}

	void IEntityLostFactHandler.HandleEntityLostFact(EntityFact fact)
	{
		UpdateIsEnabled();
	}

	void IUnitEquipmentHandler.HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
	{
		UpdateIsEnabled();
	}

	void IEntityDamageChanged.HandleDamageChanged(PartHealth health)
	{
		UpdateIsEnabled();
	}

	void ITurnStartHandler.HandleUnitStartTurn(bool isTurnBased)
	{
		if (!isTurnBased || !Enabled || !Blueprint.HasBuffModifierTag)
		{
			return;
		}
		PartAbilityModifiers optional = base.Owner.GetOptional<PartAbilityModifiers>();
		if (optional == null)
		{
			return;
		}
		List<BlueprintAbilityModifier> list;
		using (optional.GetBoundModifiers(Blueprint).ToPooledList(out list))
		{
			if (list.Count >= 1)
			{
				AbilityData abilityData = new AbilityData(ConfigRoot.Instance.AbilityRoot.ToggleAbilityStartTurnModifiers, base.Owner, 0, list);
				Game.Instance.Controllers.AbilityExecutor.Execute(abilityData.ClaimExecutionContext(base.Owner, base.Context));
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_Enabled);
		EntityFactRef<MechanicEntityFact> obj = m_Fact;
		Hash128 val2 = StructHasher<EntityFactRef<MechanicEntityFact>>.GetHash128(ref obj);
		result.Append(ref val2);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		ToggleAbility source = new ToggleAbility(default(OwlPackConstructorParameter));
		result = Unsafe.As<ToggleAbility, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ToggleAbility>(OwlPackTypeInfo);
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
		formatter.UnmanagedField(10, "m_Enabled", ref m_Enabled, state);
		formatter.Field(11, "m_Fact", ref m_Fact, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ToggleAbility>();
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
				m_Enabled = formatter.ReadUnmanaged<bool>(state);
				break;
			case 11:
				m_Fact = formatter.ReadPackable<EntityFactRef<MechanicEntityFact>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
