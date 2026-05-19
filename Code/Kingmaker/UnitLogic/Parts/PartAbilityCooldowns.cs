using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.SystemMechanics;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartAbilityCooldowns : MechanicEntityPart, IHashable, IOwlPackable<PartAbilityCooldowns>
{
	public interface IOwner : IEntityPartOwner<PartAbilityCooldowns>, IEntityPartOwner
	{
		PartAbilityCooldowns AbilityCooldowns { get; }
	}

	[OwlPackable(OwlPackableMode.Generate)]
	public class CooldownData : IHashable, IOwlPackable, IOwlPackable<CooldownData>
	{
		[JsonProperty]
		[OwlPackInclude]
		public int Cooldown;

		[JsonProperty]
		[OwlPackInclude]
		public bool UntilEndOfCombat;

		[JsonProperty]
		[OwlPackInclude]
		public bool Interrupt;

		[OwlPackInclude]
		public bool UntilEndOfTurn;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "CooldownData",
			OldNames = null,
			Fields = new FieldInfo[4]
			{
				new FieldInfo("Cooldown", typeof(int)),
				new FieldInfo("UntilEndOfCombat", typeof(bool)),
				new FieldInfo("Interrupt", typeof(bool)),
				new FieldInfo("UntilEndOfTurn", typeof(bool))
			}
		};

		[JsonConstructor]
		private CooldownData()
		{
		}

		public CooldownData(int cooldown, bool untilEndOfCombat = false)
		{
			Cooldown = cooldown;
			UntilEndOfCombat = untilEndOfCombat;
		}

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			result.Append(ref Cooldown);
			result.Append(ref UntilEndOfCombat);
			result.Append(ref Interrupt);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			CooldownData source = new CooldownData();
			result = Unsafe.As<CooldownData, TPossiblyBase>(ref source);
		}

		public virtual void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
		{
			(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
			var (objectId, _) = orRegister;
			if (orRegister.isRef)
			{
				formatter.ObjectRef(objectId);
				return;
			}
			ushort type = state.TypeLibrary.RegisterType<CooldownData>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			formatter.UnmanagedField(0, "Cooldown", ref Cooldown, state);
			formatter.UnmanagedField(1, "UntilEndOfCombat", ref UntilEndOfCombat, state);
			formatter.UnmanagedField(2, "Interrupt", ref Interrupt, state);
			formatter.UnmanagedField(3, "UntilEndOfTurn", ref UntilEndOfTurn, state);
			formatter.EndObject();
		}

		public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CooldownData>();
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
					Cooldown = formatter.ReadUnmanaged<int>(state);
					break;
				case 1:
					UntilEndOfCombat = formatter.ReadUnmanaged<bool>(state);
					break;
				case 2:
					Interrupt = formatter.ReadUnmanaged<bool>(state);
					break;
				case 3:
					UntilEndOfTurn = formatter.ReadUnmanaged<bool>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	[Serializable]
	[OwlPackable(OwlPackableMode.Generate)]
	public struct CooldownsStateSave : IHashable, IOwlPackable, IOwlPackable<CooldownsStateSave>
	{
		public Dictionary<BlueprintAbility, CooldownData> AbilityCooldowns;

		public Dictionary<BlueprintAbilityGroup, CooldownData> GroupCooldowns;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "CooldownsStateSave",
			Fields = new FieldInfo[0]
		};

		public Hash128 GetHash128()
		{
			return default(Hash128);
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			CooldownsStateSave source = default(CooldownsStateSave);
			result = Unsafe.As<CooldownsStateSave, TPossiblyBase>(ref source);
		}

		public void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
		{
			(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
			var (objectId, _) = orRegister;
			if (orRegister.isRef)
			{
				formatter.ObjectRef(objectId);
				return;
			}
			ushort type = state.TypeLibrary.RegisterType<CooldownsStateSave>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			formatter.EndObject();
		}

		public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CooldownsStateSave>();
			List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
			formatter.EnterObject();
			for (int i = 0; i < typeInfo.Fields.Length; i++)
			{
				formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
				if (mappingForType[fieldID] == byte.MaxValue)
				{
					formatter.SkipField(size);
				}
			}
			formatter.LeaveObject();
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	private Dictionary<BlueprintAbility, CooldownData> m_AbilityCooldowns = new Dictionary<BlueprintAbility, CooldownData>();

	[JsonProperty]
	[OwlPackInclude]
	private Dictionary<BlueprintAbilityGroup, CooldownData> m_GroupCooldowns = new Dictionary<BlueprintAbilityGroup, CooldownData>();

	[JsonProperty]
	[OwlPackInclude]
	private List<CooldownsStateSave> m_SavedCooldowns = new List<CooldownsStateSave>();

	public RestrictionCalculator InterruptionAbilityRestrictions;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartAbilityCooldowns",
		OldNames = null,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("m_AbilityCooldowns", typeof(Dictionary<BlueprintAbility, CooldownData>)),
			new FieldInfo("m_GroupCooldowns", typeof(Dictionary<BlueprintAbilityGroup, CooldownData>)),
			new FieldInfo("m_SavedCooldowns", typeof(List<CooldownsStateSave>))
		}
	};

	[JsonConstructor]
	public PartAbilityCooldowns()
	{
	}

	public void StartCooldown(AbilityData ability)
	{
		if (!base.Owner.IsInCombat)
		{
			return;
		}
		if (IsOnCooldown(ability))
		{
			PFLog.Default.Error(ability.Blueprint.Name + " is already on cooldown!");
			return;
		}
		RuleCalculateCooldown ruleCalculateCooldown = Rulebook.Trigger(new RuleCalculateCooldown(base.Owner, ability));
		if (ruleCalculateCooldown.Result <= 0)
		{
			Cooldown cooldownComponent = ruleCalculateCooldown.CooldownComponent;
			if (cooldownComponent == null || !cooldownComponent.UntilEndOfCombat)
			{
				goto IL_00d5;
			}
		}
		CooldownData value = new CooldownData(ruleCalculateCooldown.Result, ruleCalculateCooldown.CooldownComponent?.UntilEndOfCombat ?? false)
		{
			Interrupt = (base.Owner.Initiative.InterruptingOrder > 0)
		};
		m_AbilityCooldowns.Add(ability.Blueprint.OriginalBlueprint, value);
		goto IL_00d5;
		IL_00d5:
		if (ability.AbilityGroups.Count > 0)
		{
			foreach (BlueprintAbilityGroup abilityGroup in ability.AbilityGroups)
			{
				if (IsShouldStartCooldown(ability, abilityGroup))
				{
					StartGroupCooldown(abilityGroup, ruleCalculateCooldown);
				}
			}
		}
		base.EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IUnitAbilityCooldownHandler>)delegate(IUnitAbilityCooldownHandler h)
		{
			h.HandleAbilityCooldownStarted(ability);
		}, isCheckRuntime: true);
	}

	private bool IsShouldStartCooldown(AbilityData ability, BlueprintAbilityGroup groupRef)
	{
		PartTwoWeaponFighting optional = base.Owner.Parts.GetOptional<PartTwoWeaponFighting>();
		if (optional != null && !optional.EnableAttackWithPairedWeapon)
		{
			return true;
		}
		BlueprintCombatRoot combatRoot = ConfigRoot.Instance.CombatRoot;
		HandsEquipmentSet handsEquipmentSet = base.Owner.GetBodyOptional()?.CurrentHandsEquipmentSet;
		ItemEntityWeapon sourceWeapon = ability.SourceWeapon;
		if (sourceWeapon != null && !sourceWeapon.HoldInTwoHands)
		{
			if (sourceWeapon.HoldingSlot == handsEquipmentSet?.PrimaryHand && groupRef == combatRoot.SecondaryHandAbilityGroup.Blueprint)
			{
				return false;
			}
			if (sourceWeapon.HoldingSlot == handsEquipmentSet?.SecondaryHand && groupRef == combatRoot.PrimaryHandAbilityGroup.Blueprint)
			{
				return false;
			}
		}
		return true;
	}

	public void StartAutonomousCooldown(BlueprintAbility ability, int rounds)
	{
		if (m_AbilityCooldowns.ContainsKey(ability))
		{
			PFLog.Default.Error(ability.name + " is already on cooldown!");
			return;
		}
		if (rounds < 1)
		{
			PFLog.Default.Error($"Wrong cooldown value for {ability.name} : {rounds}!");
			return;
		}
		CooldownData value = new CooldownData(rounds)
		{
			Interrupt = (base.Owner.Initiative.InterruptingOrder > 0)
		};
		m_AbilityCooldowns.Add(ability, value);
	}

	public int? GetAutonomousCooldown(BlueprintAbility ability)
	{
		if (m_AbilityCooldowns.TryGetValue(ability, out var value))
		{
			if (value.UntilEndOfCombat)
			{
				return 99;
			}
			return value.Cooldown;
		}
		return null;
	}

	public void RemoveAutonomousCooldown(BlueprintAbility ability)
	{
		if (!m_AbilityCooldowns.ContainsKey(ability))
		{
			PFLog.Default.Error(ability.name + " is not on cooldown!");
		}
		else
		{
			m_AbilityCooldowns.Remove(ability);
		}
	}

	public void StartGroupCooldown(BlueprintAbilityGroup abilityGroup, RuleCalculateCooldown cooldownRule = null)
	{
		if (abilityGroup == null)
		{
			PFLog.Default.Error("AbilityGroup is null!");
			return;
		}
		if (m_GroupCooldowns.ContainsKey(abilityGroup))
		{
			PFLog.Default.Error(abilityGroup.name + " is already on cooldown!");
			return;
		}
		int num = 0;
		num = ((cooldownRule != null) ? (cooldownRule.GroupCooldownsData.FirstOrDefault((GroupCooldownData p) => p.Group == abilityGroup)?.Cooldown ?? 0) : Rulebook.Trigger(new RuleCalculateGroupCooldown(base.Owner, abilityGroup)).Result);
		if (num > 0)
		{
			CooldownData value = new CooldownData(num)
			{
				Interrupt = (base.Owner.Initiative.InterruptingOrder > 0),
				UntilEndOfTurn = abilityGroup.CooldownForCurrentTurnOnly
			};
			m_GroupCooldowns.Add(abilityGroup, value);
		}
	}

	public bool IsOnCooldown(AbilityData ability)
	{
		if (!base.Owner.IsInCombat)
		{
			return false;
		}
		foreach (KeyValuePair<BlueprintAbility, CooldownData> abilityCooldown in m_AbilityCooldowns)
		{
			if (ability.Blueprint.SameAbility(abilityCooldown.Key))
			{
				return true;
			}
		}
		if (ability.AbilityGroups.Count > 0)
		{
			foreach (BlueprintAbilityGroup abilityGroup in ability.AbilityGroups)
			{
				if (abilityGroup != null && IsShouldStartCooldown(ability, abilityGroup) && m_GroupCooldowns.ContainsKey(abilityGroup) && !IsIgnoredByComponent(abilityGroup, ability))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsOnCooldownUntilEndOfCombat(AbilityData ability)
	{
		if (!base.Owner.IsInCombat)
		{
			return false;
		}
		foreach (KeyValuePair<BlueprintAbility, CooldownData> abilityCooldown in m_AbilityCooldowns)
		{
			if (ability.Blueprint.SameAbility(abilityCooldown.Key) && abilityCooldown.Value.UntilEndOfCombat)
			{
				return true;
			}
		}
		if (ability.AbilityGroups.Count > 0)
		{
			foreach (BlueprintAbilityGroup abilityGroup in ability.AbilityGroups)
			{
				if (abilityGroup != null && m_GroupCooldowns.TryGetValue(abilityGroup, out var value) && value.UntilEndOfCombat && !IsIgnoredByComponent(abilityGroup, ability))
				{
					return true;
				}
			}
		}
		return false;
	}

	public int GetCooldown(BlueprintAbility ability)
	{
		if (!base.Owner.IsInCombat)
		{
			return 0;
		}
		int num = 0;
		if (m_AbilityCooldowns.TryGetValue(ability, out var value))
		{
			num = (value.UntilEndOfCombat ? int.MaxValue : value.Cooldown);
		}
		AbilityData data = ((Ability)base.Owner.Facts.Get(ability)).Data;
		if (data.AbilityGroups.Count > 0)
		{
			foreach (BlueprintAbilityGroup abilityGroup in ability.AbilityGroups)
			{
				if (abilityGroup != null && m_GroupCooldowns.TryGetValue(abilityGroup, out var value2) && !IsIgnoredByComponent(abilityGroup, data))
				{
					num = Math.Max(value2.UntilEndOfCombat ? int.MaxValue : value2.Cooldown, num);
				}
			}
		}
		return num;
	}

	public bool IsIgnoredByComponent(BlueprintAbilityGroup group, AbilityData ability)
	{
		return ability.Blueprint.GetComponents<IgnoreGroupCooldownByBuff>().Any((IgnoreGroupCooldownByBuff component) => component.IgnoreGroup == group && ability.Caster?.Buffs.GetBuff(component.IgnoreBuff) != null);
	}

	public void TickCooldowns(bool interrupt)
	{
		KeyValuePair<BlueprintAbility, CooldownData>[] array = m_AbilityCooldowns.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<BlueprintAbility, CooldownData> keyValuePair = array[i];
			if (interrupt && !keyValuePair.Value.Interrupt)
			{
				continue;
			}
			keyValuePair.Value.Interrupt = false;
			if (keyValuePair.Value.UntilEndOfCombat)
			{
				if (!base.Owner.IsInCombat)
				{
					m_AbilityCooldowns.Remove(keyValuePair.Key);
				}
				continue;
			}
			keyValuePair.Value.Cooldown--;
			if (keyValuePair.Value.Cooldown <= 0)
			{
				m_AbilityCooldowns.Remove(keyValuePair.Key);
			}
		}
		KeyValuePair<BlueprintAbilityGroup, CooldownData>[] array2 = m_GroupCooldowns.ToArray();
		for (int i = 0; i < array2.Length; i++)
		{
			KeyValuePair<BlueprintAbilityGroup, CooldownData> keyValuePair2 = array2[i];
			if (interrupt && !keyValuePair2.Value.Interrupt)
			{
				continue;
			}
			keyValuePair2.Value.Interrupt = false;
			if (keyValuePair2.Value.UntilEndOfTurn)
			{
				m_GroupCooldowns.Remove(keyValuePair2.Key);
				continue;
			}
			if (keyValuePair2.Value.UntilEndOfCombat)
			{
				if (!base.Owner.IsInCombat)
				{
					m_GroupCooldowns.Remove(keyValuePair2.Key);
				}
				continue;
			}
			keyValuePair2.Value.Cooldown--;
			if (keyValuePair2.Value.Cooldown <= 0)
			{
				m_GroupCooldowns.Remove(keyValuePair2.Key);
			}
		}
	}

	public void RemoveGroupCooldown(BlueprintAbilityGroup abilityGroup)
	{
		if (abilityGroup == null)
		{
			PFLog.Default.Error("Ability group is null!");
			return;
		}
		foreach (BlueprintAbilityGroup group in abilityGroup.GetAllAbilityGroups())
		{
			m_GroupCooldowns.Remove(group);
			base.EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IUnitAbilityCooldownHandler>)delegate(IUnitAbilityCooldownHandler h)
			{
				h.HandleGroupCooldownRemoved(group);
			}, isCheckRuntime: true);
		}
	}

	public void RemoveHandAbilityGroupsCooldown()
	{
		BlueprintCombatRoot combatRoot = ConfigRoot.Instance.CombatRoot;
		RemoveGroupCooldown(combatRoot.PrimaryHandAbilityGroup);
		RemoveGroupCooldown(combatRoot.SecondaryHandAbilityGroup);
	}

	public void ResetCooldowns(bool ignoreOncePerCombatRestriction = false)
	{
		if (ignoreOncePerCombatRestriction)
		{
			m_AbilityCooldowns.Clear();
			m_GroupCooldowns.Clear();
		}
		else
		{
			m_AbilityCooldowns = GetUntilEndOfCombatCooldowns();
			m_GroupCooldowns = GetUntilEndOfCombatGroupCooldowns();
		}
		base.EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IUnitAbilityCooldownHandler>)delegate(IUnitAbilityCooldownHandler h)
		{
			h.HandleCooldownReset();
		}, isCheckRuntime: true);
	}

	public int GroupCooldown(BlueprintAbilityGroup abilityGroup)
	{
		if (!base.Owner.IsInCombat)
		{
			return 0;
		}
		if (abilityGroup == null)
		{
			PFLog.Default.Error("Ability group is null!");
			return 0;
		}
		int num = 0;
		foreach (BlueprintAbilityGroup allAbilityGroup in abilityGroup.GetAllAbilityGroups())
		{
			num = Math.Max(m_GroupCooldowns.Get(allAbilityGroup)?.Cooldown ?? 0, num);
		}
		return num;
	}

	public bool GroupIsOnCooldown(BlueprintAbilityGroup abilityGroup)
	{
		if (!base.Owner.IsInCombat)
		{
			return false;
		}
		if (abilityGroup == null)
		{
			PFLog.Default.Error("Ability group is null!");
			return false;
		}
		foreach (BlueprintAbilityGroup allAbilityGroup in abilityGroup.GetAllAbilityGroups())
		{
			CooldownData cooldownData = m_GroupCooldowns.Get(allAbilityGroup);
			if (cooldownData != null && cooldownData.Cooldown > 0)
			{
				return true;
			}
		}
		return false;
	}

	public void RemoveAbilityCooldown(BlueprintAbility ability)
	{
		if (ability == null)
		{
			PFLog.Default.Error("Ability group is null!");
		}
		else
		{
			m_AbilityCooldowns.Remove(ability);
		}
	}

	public void Clear()
	{
		m_AbilityCooldowns.Clear();
		m_GroupCooldowns.Clear();
	}

	public void SaveCooldownData()
	{
		CooldownsStateSave cooldownsStateSave = default(CooldownsStateSave);
		cooldownsStateSave.AbilityCooldowns = m_AbilityCooldowns.ToDictionary((KeyValuePair<BlueprintAbility, CooldownData> entry) => entry.Key, (KeyValuePair<BlueprintAbility, CooldownData> entry) => entry.Value);
		cooldownsStateSave.GroupCooldowns = m_GroupCooldowns.Where((KeyValuePair<BlueprintAbilityGroup, CooldownData> entry) => !entry.Value.UntilEndOfTurn).ToDictionary((KeyValuePair<BlueprintAbilityGroup, CooldownData> entry) => entry.Key, (KeyValuePair<BlueprintAbilityGroup, CooldownData> entry) => entry.Value);
		CooldownsStateSave item = cooldownsStateSave;
		m_SavedCooldowns.Add(item);
	}

	public void RestoreCooldownData(bool ignoreOncePerCombatRestriction = false)
	{
		if (m_SavedCooldowns.Count == 0)
		{
			PFLog.AI.Error("trying to restore cooldowns for " + base.Owner.Name + " but there are none saved");
			return;
		}
		Dictionary<BlueprintAbility, CooldownData> abilityCooldowns = m_SavedCooldowns.Last().AbilityCooldowns;
		Dictionary<BlueprintAbilityGroup, CooldownData> groupCooldowns = m_SavedCooldowns.Last().GroupCooldowns;
		if (ignoreOncePerCombatRestriction)
		{
			if (abilityCooldowns != null)
			{
				m_AbilityCooldowns = abilityCooldowns;
			}
			else
			{
				m_AbilityCooldowns.Clear();
			}
			if (groupCooldowns != null)
			{
				m_GroupCooldowns = groupCooldowns;
			}
			else
			{
				m_GroupCooldowns.Clear();
			}
		}
		else
		{
			Dictionary<BlueprintAbility, CooldownData> untilEndOfCombatCooldowns = GetUntilEndOfCombatCooldowns();
			if (abilityCooldowns != null)
			{
				m_AbilityCooldowns = abilityCooldowns;
			}
			else
			{
				m_AbilityCooldowns.Clear();
			}
			foreach (KeyValuePair<BlueprintAbility, CooldownData> item in untilEndOfCombatCooldowns.Where((KeyValuePair<BlueprintAbility, CooldownData> cooldown) => !m_AbilityCooldowns.ContainsKey(cooldown.Key)))
			{
				m_AbilityCooldowns.Add(item.Key, item.Value);
			}
			Dictionary<BlueprintAbilityGroup, CooldownData> untilEndOfCombatGroupCooldowns = GetUntilEndOfCombatGroupCooldowns();
			if (groupCooldowns != null)
			{
				m_GroupCooldowns = groupCooldowns;
			}
			else
			{
				m_GroupCooldowns.Clear();
			}
			foreach (KeyValuePair<BlueprintAbilityGroup, CooldownData> item2 in untilEndOfCombatGroupCooldowns.Where((KeyValuePair<BlueprintAbilityGroup, CooldownData> cooldown) => !m_GroupCooldowns.ContainsKey(cooldown.Key)))
			{
				m_GroupCooldowns.Add(item2.Key, item2.Value);
			}
		}
		m_SavedCooldowns.RemoveLast();
	}

	private Dictionary<BlueprintAbility, CooldownData> GetUntilEndOfCombatCooldowns()
	{
		return m_AbilityCooldowns.Where((KeyValuePair<BlueprintAbility, CooldownData> abilityCooldown) => abilityCooldown.Value.UntilEndOfCombat).ToDictionary((KeyValuePair<BlueprintAbility, CooldownData> abilityCooldown) => abilityCooldown.Key, (KeyValuePair<BlueprintAbility, CooldownData> abilityCooldown) => abilityCooldown.Value);
	}

	private Dictionary<BlueprintAbilityGroup, CooldownData> GetUntilEndOfCombatGroupCooldowns()
	{
		return m_GroupCooldowns.Where((KeyValuePair<BlueprintAbilityGroup, CooldownData> groupCooldown) => groupCooldown.Value.UntilEndOfCombat).ToDictionary((KeyValuePair<BlueprintAbilityGroup, CooldownData> groupCooldown) => groupCooldown.Key, (KeyValuePair<BlueprintAbilityGroup, CooldownData> groupCooldown) => groupCooldown.Value);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Dictionary<BlueprintAbility, CooldownData> abilityCooldowns = m_AbilityCooldowns;
		if (abilityCooldowns != null)
		{
			int val2 = 0;
			foreach (KeyValuePair<BlueprintAbility, CooldownData> item in abilityCooldowns)
			{
				Hash128 hash = default(Hash128);
				Hash128 val3 = SimpleBlueprintHasher.GetHash128(item.Key);
				hash.Append(ref val3);
				Hash128 val4 = ClassHasher<CooldownData>.GetHash128(item.Value);
				hash.Append(ref val4);
				val2 ^= hash.GetHashCode();
			}
			result.Append(ref val2);
		}
		Dictionary<BlueprintAbilityGroup, CooldownData> groupCooldowns = m_GroupCooldowns;
		if (groupCooldowns != null)
		{
			int val5 = 0;
			foreach (KeyValuePair<BlueprintAbilityGroup, CooldownData> item2 in groupCooldowns)
			{
				Hash128 hash2 = default(Hash128);
				Hash128 val6 = SimpleBlueprintHasher.GetHash128(item2.Key);
				hash2.Append(ref val6);
				Hash128 val7 = ClassHasher<CooldownData>.GetHash128(item2.Value);
				hash2.Append(ref val7);
				val5 ^= hash2.GetHashCode();
			}
			result.Append(ref val5);
		}
		List<CooldownsStateSave> savedCooldowns = m_SavedCooldowns;
		if (savedCooldowns != null)
		{
			for (int i = 0; i < savedCooldowns.Count; i++)
			{
				CooldownsStateSave obj = savedCooldowns[i];
				Hash128 val8 = StructHasher<CooldownsStateSave>.GetHash128(ref obj);
				result.Append(ref val8);
			}
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartAbilityCooldowns source = new PartAbilityCooldowns();
		result = Unsafe.As<PartAbilityCooldowns, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartAbilityCooldowns>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_AbilityCooldowns", ref m_AbilityCooldowns, state);
		formatter.Field(1, "m_GroupCooldowns", ref m_GroupCooldowns, state);
		formatter.Field(2, "m_SavedCooldowns", ref m_SavedCooldowns, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartAbilityCooldowns>();
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
				m_AbilityCooldowns = formatter.ReadPackable<Dictionary<BlueprintAbility, CooldownData>>(state);
				break;
			case 1:
				m_GroupCooldowns = formatter.ReadPackable<Dictionary<BlueprintAbilityGroup, CooldownData>>(state);
				break;
			case 2:
				m_SavedCooldowns = formatter.ReadPackable<List<CooldownsStateSave>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
