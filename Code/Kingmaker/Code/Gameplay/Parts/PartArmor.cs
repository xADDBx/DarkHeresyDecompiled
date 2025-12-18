using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartArmor : MechanicEntityPart, IDamageablePart, IHashable, IOwlPackable<PartArmor>
{
	[JsonProperty]
	[OwlPackInclude]
	private float _missingDurabilityFraction;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartArmor",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("_missingDurabilityFraction", typeof(float))
		}
	};

	private StatsContainer StatsContainer => base.Owner.GetRequired<PartStatsContainer>().Container;

	public ModifiableValue DamageReduction => StatsContainer.GetStat(StatType.ArmorDamageReduction);

	public ModifiableValue Durability => StatsContainer.GetStat(StatType.ArmorDurability);

	public int DurabilityLeft => (int)Durability - Damage;

	public float DurabilityLeftFraction => Math.Max(0f, 1f - _missingDurabilityFraction);

	public int Damage
	{
		get
		{
			return Mathf.FloorToInt(_missingDurabilityFraction * (float)(int)Durability);
		}
		private set
		{
			_missingDurabilityFraction = (((int)Durability > 0) ? ((float)value / (float)(int)Durability) : 0f);
		}
	}

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
		SetDamage(Math.Max(0, (int)Durability - targetDurability));
	}

	protected override void OnAttachOrPrePostLoad()
	{
		StatsContainer.Register(StatType.ArmorDamageReduction);
		ModifiableValue modifiableValue = StatsContainer.Register(StatType.ArmorDurability);
		if (base.Owner.IsMechanism)
		{
			modifiableValue.AddOverride(StatType.HitPoints, this, onlyIfHigher: false);
		}
	}

	public void SetDamage(int damage)
	{
		if (damage < 0)
		{
			PFLog.Default.Error("Damage can't be less than 0");
			return;
		}
		Damage = Math.Min(damage, Durability);
		if (base.Owner.IsMechanism)
		{
			PartHealth required = base.Owner.GetRequired<PartHealth>();
			if (required != null)
			{
				Damage = required.ClampDamage(Damage);
				required.SetDamage(Damage);
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref _missingDurabilityFraction);
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
