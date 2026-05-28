using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartArmor : MechanicEntityPart, IDamageablePart, IStatModifier, IHashable, IOwlPackable<PartArmor>
{
	[OwlPackInclude]
	private float _missingDurabilityFraction;

	private bool _hpAsArmor;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartArmor",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("_missingDurabilityFraction", typeof(float))
		}
	};

	public int DurabilityValue => base.Owner.Actor.GetStat(StatType.MaxArmorDurability, null, default(StatContext), "DurabilityValue");

	public int DurabilityLeft
	{
		get
		{
			int durabilityValue = DurabilityValue;
			return durabilityValue - Mathf.FloorToInt(_missingDurabilityFraction * (float)durabilityValue);
		}
	}

	public float DurabilityLeftFraction => Math.Max(0f, 1f - _missingDurabilityFraction);

	public int Damage
	{
		get
		{
			return Mathf.FloorToInt(_missingDurabilityFraction * (float)DurabilityValue);
		}
		private set
		{
			float num = ((DurabilityValue > 0) ? ((float)value / (float)DurabilityValue) : 0f);
			if (!Mathf.Approximately(num, _missingDurabilityFraction))
			{
				base.Owner.Actor.NotifyStatChanged(StatType.CurrentArmorDurability, "Damage");
			}
			_missingDurabilityFraction = num;
		}
	}

	public bool HasDamage => _missingDurabilityFraction > 0f;

	public void DealDamage(int damage)
	{
		SetDamage(Damage + damage);
	}

	public void HealDamage(int heal)
	{
		SetDamage(Math.Max(0, Damage - heal));
	}

	public void HealDamageAll()
	{
		SetDamage(0);
	}

	public void SetDurabilityLeft(int targetDurability)
	{
		SetDamage(Math.Max(0, DurabilityValue - targetDurability));
	}

	protected override void OnAttachOrPrePostLoad()
	{
		RestoreHpAsArmor();
	}

	public void SetDamage(int damage)
	{
		if (damage < 0)
		{
			PFLog.Default.Error("Damage can't be less than 0");
			return;
		}
		Damage = Math.Min(damage, DurabilityValue);
		PartHealth required = base.Owner.GetRequired<PartHealth>();
		if (required != null && required.IsCountHpAsArmor)
		{
			Damage = required.ClampDamage(Damage);
			required.SetDamage(Damage);
		}
	}

	public void ActivateHpAsArmor()
	{
		_hpAsArmor = true;
		PartHealth required = base.Owner.GetRequired<PartHealth>();
		Damage = required.Damage;
		base.Owner.Actor.NotifyStatChanged(StatType.MaxArmorDurability, "ActivateHpAsArmor");
	}

	public void DeactivateHpAsArmor(int combinedDamage)
	{
		_hpAsArmor = false;
		int num2 = (Damage = Math.Min(combinedDamage, DurabilityLeft));
		base.Owner.GetRequired<PartHealth>().SetDamage(combinedDamage - num2);
		base.Owner.Actor.NotifyStatChanged(StatType.MaxArmorDurability, "DeactivateHpAsArmor");
	}

	public void RestoreHpAsArmor()
	{
		_hpAsArmor = base.Owner.GetOptional<PartHealth>()?.IsCountHpAsArmor ?? false;
	}

	void IStatModifier.TryApplyStatModifier(StatModifierCollector collector, StatType stat, StatContext context)
	{
		if (_hpAsArmor && stat == StatType.MaxArmorDurability)
		{
			collector.OverrideFull(StatType.MaxHitPoints, onlyIfHigher: false, null);
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
		PartArmor source = new PartArmor();
		result = Unsafe.As<PartArmor, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartArmor>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.UnmanagedField(0, "_missingDurabilityFraction", ref _missingDurabilityFraction, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartArmor>();
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
				_missingDurabilityFraction = formatter.ReadUnmanaged<float>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
