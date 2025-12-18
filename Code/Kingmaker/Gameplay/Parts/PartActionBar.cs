using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Code.Framework;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework;
using Kingmaker.Items;
using Kingmaker.Networking.Serialization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Parts;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.Gameplay.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class PartActionBar : BaseUnitPart, IEntityLostFactHandler<EntitySubscriber>, IEntityLostFactHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IEntityLostFactHandler, EntitySubscriber>, IAbilityReplacementsUpdatedHandler<EntitySubscriber>, IAbilityReplacementsUpdatedHandler, IEventTag<IAbilityReplacementsUpdatedHandler, EntitySubscriber>, IHiddenFactsUpdatedHandler<EntitySubscriber>, IHiddenFactsUpdatedHandler, IEventTag<IHiddenFactsUpdatedHandler, EntitySubscriber>, IHashable, IOwlPackable<PartActionBar>
{
	public interface IOwner : IEntityPartOwner<PartUnitProgression>, IEntityPartOwner
	{
		PartActionBar ActionBar { get; }
	}

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	public List<MechanicActionBarSlot> Slots = new List<MechanicActionBarSlot>();

	[JsonProperty]
	[OwlPackInclude]
	private int m_SlotRowIndexConsole;

	private List<MechanicActionBarSlot> m_HeroicBrokenSlots = new List<MechanicActionBarSlot>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartActionBar",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("Slots", typeof(List<MechanicActionBarSlot>)),
			new FieldInfo("m_SlotRowIndexConsole", typeof(int))
		}
	};

	public int SlotRowIndexConsole
	{
		get
		{
			return m_SlotRowIndexConsole;
		}
		set
		{
			m_SlotRowIndexConsole = value;
		}
	}

	public List<MechanicActionBarSlot> HeroicBrokenSlots => m_HeroicBrokenSlots;

	public void TryToInitialize()
	{
		TryInitilizeAbilitySlots();
		TryInitializeHeroicBrokenSlots();
	}

	private void TryInitilizeAbilitySlots()
	{
		List<AbilityWrapper> value;
		using (CollectionPool<List<AbilityWrapper>, AbilityWrapper>.Get(out value))
		{
			CollectNewAbilities(base.Owner, value);
			foreach (AbilityWrapper item in value)
			{
				SetToFirstFreeSlot(item);
			}
			if (Slots.Count > 100)
			{
				Slots.RemoveRange(100, Slots.Count - 100);
			}
		}
	}

	private void TryInitializeHeroicBrokenSlots()
	{
		List<AbilityWrapper> value;
		using (CollectionPool<List<AbilityWrapper>, AbilityWrapper>.Get(out value))
		{
			CollectHeroicAbilities(base.Owner, value);
			HashSet<BlueprintScriptableObject> currentHeroicBlueprints = new HashSet<BlueprintScriptableObject>(value.Select((AbilityWrapper a) => a.Ability.Blueprint));
			HeroicBrokenSlots.RemoveAll(delegate(MechanicActionBarSlot slot)
			{
				BlueprintMechanicEntityFact blueprintMechanicEntityFact = slot.AbilityFact?.Blueprint;
				return blueprintMechanicEntityFact == null || !currentHeroicBlueprints.Contains(blueprintMechanicEntityFact);
			});
			foreach (AbilityWrapper item in value)
			{
				SetToHeroicBrokenPool(item);
			}
		}
	}

	private void SetToFirstFreeSlot(AbilityWrapper ability)
	{
		MechanicActionBarSlot mechanicActionBarSlot = ability.CreateSlot(base.Owner);
		for (int i = 0; i < 100; i++)
		{
			if (GetSlot(i) is MechanicActionBarSlotEmpty)
			{
				Slots[i] = mechanicActionBarSlot;
				return;
			}
		}
		Slots.Add(mechanicActionBarSlot);
	}

	private void SetToHeroicBrokenPool(AbilityWrapper ability)
	{
		if (!m_HeroicBrokenSlots.Exists((MechanicActionBarSlot i) => i.AbilityFact.Blueprint.Equals(ability.Ability.Blueprint)))
		{
			HeroicBrokenSlots.Add(ability.CreateSlot(base.Owner));
		}
	}

	private void FillSlots(int index)
	{
		for (int i = Slots.Count; i < index + 1; i++)
		{
			Slots.Add(new MechanicActionBarSlotEmpty
			{
				Unit = base.Owner
			});
		}
	}

	public MechanicActionBarSlot GetSlot(int index)
	{
		FillSlots(index);
		MechanicActionBarSlot mechanicActionBarSlot = Slots[index];
		if (mechanicActionBarSlot == null || mechanicActionBarSlot.IsBad())
		{
			List<MechanicActionBarSlot> slots = Slots;
			MechanicActionBarSlotEmpty obj = new MechanicActionBarSlotEmpty
			{
				Unit = base.Owner
			};
			mechanicActionBarSlot = obj;
			slots[index] = obj;
		}
		return mechanicActionBarSlot;
	}

	public bool ContainsSlot(MechanicEntityFact ability)
	{
		return Slots.Any((MechanicActionBarSlot slot) => slot?.AbilityFact == ability);
	}

	private int IndexOf(BlueprintFact fact)
	{
		return Slots.FindIndex((MechanicActionBarSlot i) => i.AbilityFact?.Blueprint == fact);
	}

	public void SetSlot(MechanicActionBarSlot slot, int index)
	{
		if (index != -1)
		{
			FillSlots(index);
			Slots[index] = slot;
			EventBus.RaiseEvent(delegate(IActionBarSlotsUpdatedHandler h)
			{
				h.HandleActionBarSlotsUpdated();
			});
		}
	}

	public void SetSlot(BaseUnitEntity abilityOwner, Ability ability, int index)
	{
		SetSlot(new AbilityWrapper(ability).CreateSlot(abilityOwner), index);
	}

	public void SetSlot(BaseUnitEntity abilityOwner, ToggleAbility ability, int index)
	{
		SetSlot(new AbilityWrapper(ability).CreateSlot(abilityOwner), index);
	}

	public void RemoveSlot(int index)
	{
		if (index < Slots.Count)
		{
			Slots[index] = new MechanicActionBarSlotEmpty
			{
				Unit = base.Owner
			};
			EventBus.RaiseEvent(delegate(IActionBarSlotsUpdatedHandler h)
			{
				h.HandleActionBarSlotsUpdated();
			});
		}
	}

	public void RemoveFromIndexToEnd(int index)
	{
		Slots.RemoveRange(index, Slots.Count - index);
	}

	private void CollectNewAbilities(BaseUnitEntity unit, List<AbilityWrapper> results)
	{
		results.AddRange(from i in unit.Abilities.Visible
			where i.FirstSource == null || !i.FirstSource.IsMissing
			where !ContainsSlot(i) && !(i.SourceItem is ItemEntityWeapon) && !i.Data.Blueprint.IsHeroic && !i.Data.Blueprint.IsBroken
			select new AbilityWrapper(i));
		results.AddRange(from i in unit.ToggleAbilities.Visible
			where i.FirstSource == null || !i.FirstSource.IsMissing
			where !ContainsSlot(i) && !(i.SourceItem is ItemEntityWeapon)
			select new AbilityWrapper(i));
		results.Sort((AbilityWrapper f1, AbilityWrapper f2) => CompareFactsPriority(f1.Blueprint, f2.Blueprint));
	}

	private void CollectHeroicAbilities(BaseUnitEntity unit, List<AbilityWrapper> results)
	{
		results.AddRange(from i in unit.Abilities.Visible
			where i.Data.Blueprint.IsHeroic || i.Data.Blueprint.IsBroken
			select new AbilityWrapper(i));
	}

	private void RemoveSlot(MechanicEntityFact ability)
	{
		if (Slots == null)
		{
			return;
		}
		for (int i = 0; i < Slots.Count; i++)
		{
			if (Slots[i]?.AbilityFact == ability)
			{
				Slots[i] = new MechanicActionBarSlotEmpty
				{
					Unit = base.Owner
				};
				EventBus.RaiseEvent(delegate(IActionBarSlotsUpdatedHandler h)
				{
					h.HandleActionBarSlotsUpdated();
				});
				break;
			}
		}
	}

	private static int CompareFactsPriority(BlueprintMechanicEntityFact f1, BlueprintMechanicEntityFact f2)
	{
		int num = f1.GetComponent<ActionPanelLogic>()?.Priority ?? 0;
		int value = f2.GetComponent<ActionPanelLogic>()?.Priority ?? 0;
		return -num.CompareTo(value);
	}

	void IEntityLostFactHandler.HandleEntityLostFact(EntityFact fact)
	{
		MechanicEntityFact mechanicEntityFact = fact as MechanicEntityFact;
		if ((mechanicEntityFact != null && (mechanicEntityFact is Ability || mechanicEntityFact is ToggleAbility)) ? true : false)
		{
			RemoveSlot(mechanicEntityFact);
		}
	}

	void IAbilityReplacementsUpdatedHandler.HandleAbilityReplacementsUpdated(BlueprintAbility target)
	{
		Ability replacement = base.Owner.GetOptional<PartAbilityReplacements>()?.Get(target);
		for (int i = 0; i < Slots.Count; i++)
		{
			if (Slots[i] is MechanicActionBarSlotAbility mechanicActionBarSlotAbility && mechanicActionBarSlotAbility.AbilityFact?.Blueprint == target)
			{
				SetSlot(new MechanicActionBarSlotAbility
				{
					Ability = mechanicActionBarSlotAbility.OriginalAbility,
					Replacement = replacement,
					Unit = base.Owner
				}, i);
			}
		}
	}

	void IHiddenFactsUpdatedHandler.HandleHiddenFactsUpdated(IEnumerable<BlueprintFact> updatedFacts)
	{
		foreach (BlueprintFact updatedFact in updatedFacts)
		{
			if (updatedFact is BlueprintAbility || updatedFact is BlueprintToggleAbility)
			{
				int num = IndexOf(updatedFact);
				if (num != -1)
				{
					RemoveSlot(num);
				}
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_SlotRowIndexConsole);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartActionBar source = new PartActionBar();
		result = Unsafe.As<PartActionBar, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartActionBar>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "Slots", ref Slots, state);
		formatter.UnmanagedField(1, "m_SlotRowIndexConsole", ref m_SlotRowIndexConsole, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartActionBar>();
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
				Slots = formatter.ReadPackable<List<MechanicActionBarSlot>>(state);
				break;
			case 1:
				m_SlotRowIndexConsole = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
