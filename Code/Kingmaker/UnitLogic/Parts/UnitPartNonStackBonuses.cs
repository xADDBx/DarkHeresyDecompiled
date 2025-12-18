using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartNonStackBonuses : BaseUnitPart, IHashable, IOwlPackable<UnitPartNonStackBonuses>
{
	private List<Modifier> m_NonStuckModifiers = new List<Modifier>();

	private Dictionary<ItemSlot, List<Modifier>> m_NonStuckSlots = new Dictionary<ItemSlot, List<Modifier>>();

	private Dictionary<Buff, List<Modifier>> m_NonStuckBuffs = new Dictionary<Buff, List<Modifier>>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartNonStackBonuses",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public bool ShouldShowWarning(ItemSlot slot)
	{
		List<Modifier> nonStuckModifiers = GetNonStuckModifiers(slot);
		if (nonStuckModifiers != null)
		{
			return nonStuckModifiers.Count > 0;
		}
		return false;
	}

	public bool ShouldShowWarning(Buff buff)
	{
		List<Modifier> nonStuckModifiers = GetNonStuckModifiers(buff);
		if (nonStuckModifiers != null)
		{
			return nonStuckModifiers.Count > 0;
		}
		return false;
	}

	public List<Modifier> GetNonStuckModifiers(ItemSlot slot)
	{
		return m_NonStuckSlots.Get(slot);
	}

	public List<Modifier> GetNonStuckModifiers(Buff buff)
	{
		return m_NonStuckBuffs.Get(buff);
	}

	public List<ItemSlot> GetItemsList(ItemSlot excludeItem = null)
	{
		List<ItemSlot> list = new List<ItemSlot>();
		foreach (KeyValuePair<ItemSlot, List<Modifier>> nonStuckSlot in m_NonStuckSlots)
		{
			if (excludeItem != nonStuckSlot.Key)
			{
				list.Add(nonStuckSlot.Key);
			}
		}
		return list;
	}

	public List<Buff> GetBuffList(Buff excludeBuff = null)
	{
		List<Buff> list = new List<Buff>();
		foreach (KeyValuePair<Buff, List<Modifier>> nonStuckBuff in m_NonStuckBuffs)
		{
			if (excludeBuff != nonStuckBuff.Key)
			{
				list.Add(nonStuckBuff.Key);
			}
		}
		return list;
	}

	public void HandleModifierAdded(ModifiableValue modifiable, Modifier newMod)
	{
		if (!(modifiable.Owner is BaseUnitEntity baseUnitEntity) || !baseUnitEntity.Faction.IsPlayer || !baseUnitEntity.IsInCompanionRoster() || newMod.Stackable || newMod.Value <= 0 || (newMod.Item == null && ((!(newMod.Fact?.MaybeContext?.MaybeCaster?.IsPlayerFaction)) ?? false)))
		{
			return;
		}
		bool flag = false;
		foreach (Modifier modifier in modifiable.GetModifiers(newMod.Descriptor))
		{
			if (modifier != newMod && !modifier.Stackable)
			{
				flag = true;
				AddNewModifier(newMod, modifier);
			}
		}
		if (flag)
		{
			m_NonStuckModifiers.Add(newMod);
			PFLog.EntityFact.Log("Add non-stack " + newMod);
			EventBus.RaiseEvent(delegate(INonStackModifierHandler h)
			{
				h.HandleNonStackModifierAdded(this, modifiable, newMod);
			});
		}
	}

	public void HandleModifierRemoving(ModifiableValue modifiable, Modifier mod)
	{
		if (!m_NonStuckModifiers.Remove(mod))
		{
			return;
		}
		PFLog.EntityFact.Log("Remove non-stack " + mod);
		using PooledList<Modifier> pooledList = PooledList<Modifier>.Get();
		if (mod.Fact is Buff key && m_NonStuckBuffs.TryGetValue(key, out var value))
		{
			pooledList.AddRange(value);
			ListPool<Modifier>.Release(value);
			m_NonStuckBuffs.Remove(key);
		}
		ItemSlot itemSlot = mod.Item?.HoldingSlot;
		if (itemSlot != null && m_NonStuckSlots.TryGetValue(itemSlot, out var value2))
		{
			pooledList.AddRange(value2);
			value2.Clear();
		}
		foreach (Modifier item in pooledList)
		{
			if (item.Fact is Buff key2)
			{
				List<Modifier> list = m_NonStuckBuffs.Get(key2);
				if (list != null)
				{
					list.Remove(mod);
					if (list.Count == 0)
					{
						m_NonStuckBuffs.Remove(key2);
						m_NonStuckModifiers.Remove(item);
					}
				}
			}
			ItemSlot itemSlot2 = item.Item?.HoldingSlot;
			if (itemSlot2 == null)
			{
				continue;
			}
			List<Modifier> list2 = m_NonStuckSlots.Get(itemSlot2);
			if (list2 != null)
			{
				list2.Remove(mod);
				if (list2.Count == 0)
				{
					m_NonStuckModifiers.Remove(item);
				}
			}
		}
	}

	private void AddNewModifier(Modifier newModifier, Modifier modifier)
	{
		if (!m_NonStuckModifiers.Contains(modifier))
		{
			m_NonStuckModifiers.Add(modifier);
		}
		AddModifierToSourceCollection(newModifier, modifier);
		AddModifierToSourceCollection(modifier, newModifier);
	}

	private void AddModifierToSourceCollection(Modifier collectionProvider, Modifier modifier)
	{
		if (collectionProvider.Fact is Buff key)
		{
			if (!m_NonStuckBuffs.TryGetValue(key, out var value))
			{
				value = ListPool<Modifier>.Claim();
				m_NonStuckBuffs.Add(key, value);
			}
			if (!value.Contains(modifier))
			{
				value.Add(modifier);
			}
			return;
		}
		ItemSlot itemSlot = collectionProvider.Item?.HoldingSlot;
		if (itemSlot != null)
		{
			if (!m_NonStuckSlots.TryGetValue(itemSlot, out var value2))
			{
				value2 = ListPool<Modifier>.Claim();
				m_NonStuckSlots.Add(itemSlot, value2);
			}
			if (!value2.Contains(modifier))
			{
				value2.Add(modifier);
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartNonStackBonuses source = new UnitPartNonStackBonuses();
		result = Unsafe.As<UnitPartNonStackBonuses, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartNonStackBonuses>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartNonStackBonuses>();
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
