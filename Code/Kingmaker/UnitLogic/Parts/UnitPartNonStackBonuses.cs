using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartNonStackBonuses : BaseUnitPart, IActorStatChangedHandler<EntitySubscriber>, IActorStatChangedHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IActorStatChangedHandler, EntitySubscriber>, IEntitySubscriber, IHashable, IOwlPackable<UnitPartNonStackBonuses>
{
	private List<Modifier> m_NonStuckModifiers = new List<Modifier>();

	private Dictionary<ItemSlot, List<Modifier>> m_NonStuckSlots = new Dictionary<ItemSlot, List<Modifier>>();

	private Dictionary<Buff, List<Modifier>> m_NonStuckBuffs = new Dictionary<Buff, List<Modifier>>();

	private Dictionary<StatType, List<Modifier>> m_ConflictsByStat = new Dictionary<StatType, List<Modifier>>();

	private readonly StatQueryOutput _queryBuffer = new StatQueryOutput();

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

	void IActorStatChangedHandler.HandleActorStatChanged(StatChangeSet stats)
	{
		bool flag = false;
		StatType[] allStats = StatTypeHelper.AllStats;
		foreach (StatType statType in allStats)
		{
			if (!stats.Contains(statType))
			{
				continue;
			}
			bool flag2 = m_ConflictsByStat.Remove(statType);
			flag = flag || flag2;
			_queryBuffer.Clear();
			base.Owner.Actor.GetStat(statType, _queryBuffer, default(StatContext), "HandleActorStatChanged");
			ReadonlyList<Modifier> list = _queryBuffer.AllModifiers.List;
			List<Modifier> list2 = null;
			for (int j = 0; j < list.Count; j++)
			{
				Modifier modifier = list[j];
				if (modifier.Stackable || modifier.Value <= 0 || !PassesSourceFilter(modifier))
				{
					continue;
				}
				for (int k = j + 1; k < list.Count; k++)
				{
					Modifier modifier2 = list[k];
					if (!modifier2.Stackable && modifier2.Value > 0 && PassesSourceFilter(modifier2) && modifier.SameStack(modifier2))
					{
						if (list2 == null)
						{
							list2 = new List<Modifier>();
						}
						if (!list2.Contains(modifier))
						{
							list2.Add(modifier);
						}
						if (!list2.Contains(modifier2))
						{
							list2.Add(modifier2);
						}
					}
				}
			}
			if (list2 != null && list2.Count > 0)
			{
				m_ConflictsByStat[statType] = list2;
				flag = true;
			}
		}
		if (flag)
		{
			RebuildAggregateMaps();
		}
	}

	private static bool PassesSourceFilter(Modifier mod)
	{
		if (mod.Item == null)
		{
			return mod.Fact?.MaybeContext?.MaybeCaster?.IsPlayerFaction ?? true;
		}
		return true;
	}

	private void RebuildAggregateMaps()
	{
		m_NonStuckModifiers.Clear();
		m_NonStuckSlots.Clear();
		m_NonStuckBuffs.Clear();
		foreach (KeyValuePair<StatType, List<Modifier>> item in m_ConflictsByStat)
		{
			List<Modifier> value = item.Value;
			for (int i = 0; i < value.Count; i++)
			{
				Modifier modifier = value[i];
				if (!m_NonStuckModifiers.Contains(modifier))
				{
					m_NonStuckModifiers.Add(modifier);
				}
				for (int j = 0; j < value.Count; j++)
				{
					if (i != j)
					{
						Modifier modifier2 = value[j];
						AddToSourceCollection(modifier, modifier2);
					}
				}
			}
		}
	}

	private void AddToSourceCollection(Modifier source, Modifier modifier)
	{
		if (source.Fact is Buff key)
		{
			if (!m_NonStuckBuffs.TryGetValue(key, out var value))
			{
				value = new List<Modifier>();
				m_NonStuckBuffs.Add(key, value);
			}
			if (!value.Contains(modifier))
			{
				value.Add(modifier);
			}
			return;
		}
		ItemSlot itemSlot = source.Item?.HoldingSlot;
		if (itemSlot != null)
		{
			if (!m_NonStuckSlots.TryGetValue(itemSlot, out var value2))
			{
				value2 = new List<Modifier>();
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
