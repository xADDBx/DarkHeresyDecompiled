using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Mechanics;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Framework.Mechanics.Actor;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class PartEquipmentStats : MechanicEntityPart, IStatModifier, IHashable, IOwlPackable<PartEquipmentStats>
{
	private readonly struct Entry
	{
		public readonly ItemEntity Source;

		public readonly StatType Stat;

		public readonly int Value;

		public readonly ModifierType ModifierType;

		public readonly ModifierDescriptor Descriptor;

		public Entry(ItemEntity source, StatType stat, int value, ModifierType modifierType, ModifierDescriptor descriptor)
		{
			Source = source;
			Stat = stat;
			Value = value;
			ModifierType = modifierType;
			Descriptor = descriptor;
		}
	}

	[OwlPackIgnore]
	private readonly List<Entry> _entries = new List<Entry>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartEquipmentStats",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public void Register(ItemEntity source, StatType stat, int value, ModifierType modifierType = ModifierType.ValAdd, ModifierDescriptor descriptor = ModifierDescriptor.None)
	{
		_entries.Add(new Entry(source, stat, value, modifierType, descriptor));
		base.Owner.Actor.NotifyStatsChanged((ulong)(1L << (int)stat), "Register");
	}

	public void Unregister(ItemEntity source)
	{
		ulong num = 0uL;
		for (int num2 = _entries.Count - 1; num2 >= 0; num2--)
		{
			if (_entries[num2].Source == source)
			{
				num |= (ulong)(1L << (int)_entries[num2].Stat);
				_entries.RemoveAt(num2);
			}
		}
		if (num != 0L)
		{
			base.Owner.Actor.NotifyStatsChanged(num, "Unregister");
		}
	}

	void IStatModifier.TryApplyStatModifier(StatModifierCollector collector, StatType stat, StatContext context)
	{
		for (int i = 0; i < _entries.Count; i++)
		{
			Entry entry = _entries[i];
			if (entry.Stat == stat)
			{
				collector.Modifiers.Add(entry.ModifierType, entry.Value, null, entry.Source, BonusType.None, StatType.Unknown, entry.Descriptor);
			}
		}
	}

	void IStatModifier.CollectAffectedStats(ICollection<AffectedStatEntry> entries)
	{
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
		PartEquipmentStats source = new PartEquipmentStats();
		result = Unsafe.As<PartEquipmentStats, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartEquipmentStats>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartEquipmentStats>();
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
