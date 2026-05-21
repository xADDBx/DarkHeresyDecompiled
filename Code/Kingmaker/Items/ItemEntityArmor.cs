using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.Gameplay.Features.Items.Utility;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Items;

[OwlPackable(OwlPackableMode.Generate)]
public class ItemEntityArmor : ItemEntity<BlueprintItemArmor>, IStatModifier, IHashable, IOwlPackable<ItemEntityArmor>
{
	public static readonly StatType[] PenaltyDependentSkills = new StatType[0];

	[OwlPackInclude]
	public ItemPowerLevel PowerLevelOverride;

	[OwlPackInclude]
	public ItemFaction FactionOverride;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ItemEntityArmor",
		OldNames = null,
		Fields = new FieldInfo[31]
		{
			new FieldInfo("UniqueId", typeof(string)),
			new FieldInfo("m_IsInGame", typeof(bool)),
			new FieldInfo("m_Position", typeof(Vector3)),
			new FieldInfo("m_Orientation", typeof(float)),
			new FieldInfo("m_InitialPosition", typeof(Vector3?)),
			new FieldInfo("m_InitialOrientation", typeof(float?)),
			new FieldInfo("Facts", typeof(EntityFactsManager)),
			new FieldInfo("Parts", typeof(EntityPartsManager)),
			new FieldInfo("m_IsRevealed", typeof(bool)),
			new FieldInfo("m_ViewHandlingOnDisposePolicyOverride", typeof(ViewHandlingOnDisposePolicyType?)),
			new FieldInfo("m_Initiative", typeof(Initiative)),
			new FieldInfo("m_OriginalBlueprint", typeof(BlueprintMechanicEntityFact)),
			new FieldInfo("m_Blueprint", typeof(BlueprintMechanicEntityFact)),
			new FieldInfo("MainFact", typeof(MechanicEntityFact)),
			new FieldInfo("m_Count", typeof(int)),
			new FieldInfo("m_InventorySlotIndex", typeof(int)),
			new FieldInfo("m_FactsAppliedToWielder", typeof(EntityFact[])),
			new FieldInfo("m_SkinningSuccessful", typeof(bool)),
			new FieldInfo("m_WielderRef", typeof(EntityRef<MechanicEntity>)),
			new FieldInfo("m_IdentifyRolls", typeof(List<IdentifyRollData>)),
			new FieldInfo("m_NotLootable", typeof(bool)),
			new FieldInfo("Time", typeof(TimeSpan)),
			new FieldInfo("Charges", typeof(int)),
			new FieldInfo("IsIdentified", typeof(bool)),
			new FieldInfo("SellTime", typeof(TimeSpan?)),
			new FieldInfo("OriginArea", typeof(BlueprintArea)),
			new FieldInfo("SourceContainer", typeof(BlueprintItem)),
			new FieldInfo("VendorBlueprint", typeof(BlueprintMechanicEntityFact)),
			new FieldInfo("IsNonRemovable", typeof(bool)),
			new FieldInfo("PowerLevelOverride", typeof(ItemPowerLevel)),
			new FieldInfo("FactionOverride", typeof(ItemFaction))
		}
	};

	public ItemPowerLevel PowerLevel => base.Blueprint.ResolvePowerLevel(PowerLevelOverride);

	public ItemFaction EffectiveFaction
	{
		get
		{
			if (FactionOverride == ItemFaction.None)
			{
				return base.Blueprint.Faction;
			}
			return FactionOverride;
		}
	}

	public ItemEntityShield Shield { get; private set; }

	public override bool IsPartOfAnotherItem => Shield != null;

	public int ArmorDamageReduction => base.Blueprint.GetArmorDamageReduction(PowerLevelOverride);

	public int ArmorDurability => base.Blueprint.GetArmorDurability(PowerLevelOverride);

	public ItemEntityArmor([NotNull] BlueprintItemArmor bpItem, ItemEntityShield shield = null)
		: base(bpItem)
	{
		Shield = shield;
	}

	protected ItemEntityArmor(OwlPackConstructorParameter _)
		: base(_)
	{
	}

	protected override bool CanBeEquippedInternal(MechanicEntity owner)
	{
		if (base.CanBeEquippedInternal(owner))
		{
			return owner.GetProficienciesOptional()?.Contains(base.Blueprint.ProficiencyGroup) ?? true;
		}
		return false;
	}

	public void PostLoad(ItemEntityShield shield)
	{
		Shield = shield;
		PostLoad();
	}

	protected override void OnPostLoad()
	{
		AddArmorModifiers(base.Owner);
	}

	public override void OnDidEquipped(MechanicEntity wielder)
	{
		base.OnDidEquipped(wielder);
		AddArmorModifiers(wielder);
	}

	public override void OnWillUnequip()
	{
		RemoveArmorModifiers();
		base.OnWillUnequip();
	}

	public override bool CanBeMerged(ItemEntity other)
	{
		if (base.CanBeMerged(other) && other is ItemEntityArmor itemEntityArmor && PowerLevelOverride == itemEntityArmor.PowerLevelOverride)
		{
			return FactionOverride == itemEntityArmor.FactionOverride;
		}
		return false;
	}

	public override void CopyRuntimeStateTo(ItemEntity other)
	{
		base.CopyRuntimeStateTo(other);
		ItemEntityArmor obj = (ItemEntityArmor)other;
		obj.PowerLevelOverride = PowerLevelOverride;
		obj.FactionOverride = FactionOverride;
	}

	public StatFactionModifierConfig[] GetFractionModifiers()
	{
		ItemFaction effectiveFaction = EffectiveFaction;
		if (effectiveFaction != 0)
		{
			return ConfigRoot.Instance.ItemFactionRoot.GetArmorModifiers(effectiveFaction);
		}
		return Array.Empty<StatFactionModifierConfig>();
	}

	public override StatBaseValue GetStatBaseValue(StatType type)
	{
		return type switch
		{
			StatType.ItemArmorDamageReduction => ArmorDamageReduction, 
			StatType.ItemArmorAmount => ArmorDurability, 
			_ => base.GetStatBaseValue(type), 
		};
	}

	void IStatModifier.TryApplyStatModifier(StatModifierCollector collector, StatType stat, StatContext context)
	{
		StatFactionModifierConfig[] fractionModifiers = GetFractionModifiers();
		foreach (StatFactionModifierConfig statFactionModifierConfig in fractionModifiers)
		{
			if (statFactionModifierConfig.Stat == stat)
			{
				collector.Modifiers.Add(statFactionModifierConfig.ModifierType, statFactionModifierConfig.Value, null, null, BonusType.None, StatType.Unknown, statFactionModifierConfig.Descriptor);
			}
		}
	}

	void IStatModifier.CollectAffectedStats(ICollection<AffectedStatEntry> entries)
	{
		StatFactionModifierConfig[] fractionModifiers = GetFractionModifiers();
		foreach (StatFactionModifierConfig statFactionModifierConfig in fractionModifiers)
		{
			entries.Add(new AffectedStatEntry(statFactionModifierConfig.Stat));
		}
	}

	private void AddArmorModifiers([CanBeNull] MechanicEntity wielder)
	{
		if (wielder != null && wielder.UseArmorOfEquipment)
		{
			PartEquipmentStats orCreate = wielder.GetOrCreate<PartEquipmentStats>();
			orCreate.Register(this, StatType.ItemArmorDamageReduction, ArmorDamageReduction);
			orCreate.Register(this, StatType.ItemArmorAmount, ArmorDurability);
			StatFactionModifierConfig[] fractionModifiers = GetFractionModifiers();
			foreach (StatFactionModifierConfig statFactionModifierConfig in fractionModifiers)
			{
				StatType stat = ((statFactionModifierConfig.Stat == StatType.ItemArmorDefence) ? StatType.Defence : statFactionModifierConfig.Stat);
				orCreate.Register(this, stat, statFactionModifierConfig.Value, statFactionModifierConfig.ModifierType, statFactionModifierConfig.Descriptor);
			}
		}
	}

	private void RemoveArmorModifiers()
	{
		MechanicEntity owner = base.Owner;
		if (owner != null && owner.UseArmorOfEquipment)
		{
			base.Owner.GetOptional<PartEquipmentStats>()?.Unregister(this);
		}
	}

	public void RefreshArmorModifiers()
	{
		if (base.Owner != null)
		{
			RemoveArmorModifiers();
			AddArmorModifiers(base.Owner);
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
		ItemEntityArmor source = new ItemEntityArmor(default(OwlPackConstructorParameter));
		result = Unsafe.As<ItemEntityArmor, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ItemEntityArmor>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.UniqueId;
		formatter.StringField(0, "UniqueId", ref value, state);
		formatter.UnmanagedField(1, "m_IsInGame", ref m_IsInGame, state);
		formatter.Field(2, "m_Position", ref m_Position, state);
		formatter.UnmanagedField(3, "m_Orientation", ref m_Orientation, state);
		formatter.NullableField(4, "m_InitialPosition", ref m_InitialPosition, state);
		formatter.UnmanagedNullableField(5, "m_InitialOrientation", ref m_InitialOrientation, state);
		formatter.Field(6, "Facts", ref Facts, state);
		formatter.Field(7, "Parts", ref Parts, state);
		formatter.UnmanagedField(8, "m_IsRevealed", ref m_IsRevealed, state);
		formatter.EnumNullableField(9, "m_ViewHandlingOnDisposePolicyOverride", ref m_ViewHandlingOnDisposePolicyOverride, state);
		formatter.Field(10, "m_Initiative", ref m_Initiative, state);
		formatter.Field(11, "m_OriginalBlueprint", ref m_OriginalBlueprint, state);
		formatter.Field(12, "m_Blueprint", ref m_Blueprint, state);
		MechanicEntityFact value2 = base.MainFact;
		formatter.Field(13, "MainFact", ref value2, state);
		formatter.UnmanagedField(14, "m_Count", ref m_Count, state);
		formatter.UnmanagedField(15, "m_InventorySlotIndex", ref m_InventorySlotIndex, state);
		formatter.Field(16, "m_FactsAppliedToWielder", ref m_FactsAppliedToWielder, state);
		formatter.UnmanagedField(17, "m_SkinningSuccessful", ref m_SkinningSuccessful, state);
		formatter.Field(18, "m_WielderRef", ref m_WielderRef, state);
		List<IdentifyRollData> value3 = base.m_IdentifyRolls;
		formatter.Field(19, "m_IdentifyRolls", ref value3, state);
		formatter.UnmanagedField(20, "m_NotLootable", ref m_NotLootable, state);
		TimeSpan value4 = base.Time;
		formatter.Field(21, "Time", ref value4, state);
		int value5 = base.Charges;
		formatter.UnmanagedField(22, "Charges", ref value5, state);
		bool value6 = base.IsIdentified;
		formatter.UnmanagedField(23, "IsIdentified", ref value6, state);
		TimeSpan? value7 = base.SellTime;
		formatter.NullableField(24, "SellTime", ref value7, state);
		BlueprintArea value8 = base.OriginArea;
		formatter.Field(25, "OriginArea", ref value8, state);
		formatter.Field(26, "SourceContainer", ref SourceContainer, state);
		BlueprintMechanicEntityFact value9 = base.VendorBlueprint;
		formatter.Field(27, "VendorBlueprint", ref value9, state);
		bool value10 = base.IsNonRemovable;
		formatter.UnmanagedField(28, "IsNonRemovable", ref value10, state);
		formatter.EnumField(29, "PowerLevelOverride", ref PowerLevelOverride, state);
		formatter.EnumField(30, "FactionOverride", ref FactionOverride, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ItemEntityArmor>();
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
				base.UniqueId = formatter.ReadString(state);
				break;
			case 1:
				m_IsInGame = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				m_Position = formatter.ReadPackable<Vector3>(state);
				break;
			case 3:
				m_Orientation = formatter.ReadUnmanaged<float>(state);
				break;
			case 4:
				m_InitialPosition = formatter.ReadNullablePackable<Vector3>(state);
				break;
			case 5:
				m_InitialOrientation = formatter.ReadNullableUnmanaged<float>(state);
				break;
			case 6:
				Facts = formatter.ReadPackable<EntityFactsManager>(state);
				break;
			case 7:
				Parts = formatter.ReadPackable<EntityPartsManager>(state);
				break;
			case 8:
				m_IsRevealed = formatter.ReadUnmanaged<bool>(state);
				break;
			case 9:
				m_ViewHandlingOnDisposePolicyOverride = formatter.ReadNullableEnum<ViewHandlingOnDisposePolicyType>(state);
				break;
			case 10:
				m_Initiative = formatter.ReadPackable<Initiative>(state);
				break;
			case 11:
				m_OriginalBlueprint = formatter.ReadPackable<BlueprintMechanicEntityFact>(state);
				break;
			case 12:
				m_Blueprint = formatter.ReadPackable<BlueprintMechanicEntityFact>(state);
				break;
			case 13:
				base.MainFact = formatter.ReadPackable<MechanicEntityFact>(state);
				break;
			case 14:
				m_Count = formatter.ReadUnmanaged<int>(state);
				break;
			case 15:
				m_InventorySlotIndex = formatter.ReadUnmanaged<int>(state);
				break;
			case 16:
				m_FactsAppliedToWielder = formatter.ReadPackable<EntityFact[]>(state);
				break;
			case 17:
				m_SkinningSuccessful = formatter.ReadUnmanaged<bool>(state);
				break;
			case 18:
				m_WielderRef = formatter.ReadPackable<EntityRef<MechanicEntity>>(state);
				break;
			case 19:
				base.m_IdentifyRolls = formatter.ReadPackable<List<IdentifyRollData>>(state);
				break;
			case 20:
				m_NotLootable = formatter.ReadUnmanaged<bool>(state);
				break;
			case 21:
				base.Time = formatter.ReadPackable<TimeSpan>(state);
				break;
			case 22:
				base.Charges = formatter.ReadUnmanaged<int>(state);
				break;
			case 23:
				base.IsIdentified = formatter.ReadUnmanaged<bool>(state);
				break;
			case 24:
				base.SellTime = formatter.ReadNullablePackable<TimeSpan>(state);
				break;
			case 25:
				base.OriginArea = formatter.ReadPackable<BlueprintArea>(state);
				break;
			case 26:
				SourceContainer = formatter.ReadPackable<BlueprintItem>(state);
				break;
			case 27:
				base.VendorBlueprint = formatter.ReadPackable<BlueprintMechanicEntityFact>(state);
				break;
			case 28:
				base.IsNonRemovable = formatter.ReadUnmanaged<bool>(state);
				break;
			case 29:
				PowerLevelOverride = formatter.ReadEnum<ItemPowerLevel>(state);
				break;
			case 30:
				FactionOverride = formatter.ReadEnum<ItemFaction>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
