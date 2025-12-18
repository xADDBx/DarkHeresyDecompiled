using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Mechadendrites;
using Kingmaker.Visual.Animation;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Items;

[OwlPackable(OwlPackableMode.Generate)]
public class ItemEntityWeapon : ItemEntity<BlueprintItemWeapon>, IHashable, IOwlPackable<ItemEntityWeapon>
{
	[OwlPackInclude]
	private bool _fake;

	[JsonIgnore]
	public RandomShuffleSequence ProjectileLocatorIndexSequence;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ItemEntityWeapon",
		OldNames = null,
		Fields = new FieldInfo[33]
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
			new FieldInfo("VendorBlueprint", typeof(BlueprintMechanicEntityFact)),
			new FieldInfo("IsNonRemovable", typeof(bool)),
			new FieldInfo("_fake", typeof(bool)),
			new FieldInfo("ForceSecondary", typeof(bool)),
			new FieldInfo("Second", typeof(ItemEntityWeapon)),
			new FieldInfo("IsSecondPartOfDoubleWeapon", typeof(bool)),
			new FieldInfo("CurrentUsedBarrel", typeof(int))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public bool ForceSecondary { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public ItemEntityWeapon Second { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool IsSecondPartOfDoubleWeapon { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public int CurrentUsedBarrel { get; set; }

	public ItemEntityShield Shield { get; private set; }

	public override bool IsFake => _fake;

	public bool IsShield => Shield != null;

	public override bool IsLootable
	{
		get
		{
			if (base.IsLootable)
			{
				return !base.Blueprint.IsNatural;
			}
			return false;
		}
	}

	public int AttackRange => this.GetWeaponStats().ResultMaxDistance;

	public int AttackOptimalRange => this.GetWeaponStats().ResultOptimalDistance;

	public int ThreatRange
	{
		get
		{
			if (!base.Blueprint.IsMelee)
			{
				return 1;
			}
			return AttackRange;
		}
	}

	public override bool IsPartOfAnotherItem
	{
		get
		{
			if (Shield == null)
			{
				return IsSecondPartOfDoubleWeapon;
			}
			return true;
		}
	}

	public WeaponType WeaponType
	{
		get
		{
			if (!IsShield)
			{
				return base.Blueprint.VisualParameters.WeaponType;
			}
			return WeaponType.Shield;
		}
	}

	[CanBeNull]
	public BlueprintAbility AttackOfOpportunityAbility
	{
		get
		{
			BlueprintAbility attackOfOpportunityAbility = base.Blueprint.AttackOfOpportunityAbility;
			bool num;
			if (attackOfOpportunityAbility == null)
			{
				if (!base.Blueprint.IsRanged)
				{
					goto IL_0045;
				}
				num = base.Wielder?.Features.AllowAttackOfOpportunityWithRangedWeapon;
			}
			else
			{
				num = attackOfOpportunityAbility;
			}
			if (num)
			{
				return FirstAbility;
			}
			goto IL_0045;
			IL_0045:
			return null;
		}
	}

	public BlueprintAbilityFXSettings AttackOfOpportunityAbilityFXSettings => base.Blueprint.AttackOfOpportunityAbilityFXSettings;

	[CanBeNull]
	private BlueprintAbility FirstAbility
	{
		get
		{
			for (int i = 0; i < base.Blueprint.WeaponAbilities.Count; i++)
			{
				BlueprintAbility ability = base.Blueprint.WeaponAbilities[i].Ability;
				if (ability != null)
				{
					return ability;
				}
			}
			return null;
		}
	}

	public bool HoldInTwoHands
	{
		get
		{
			if (base.Owner != null && base.Owner is UnitEntity unitEntity && unitEntity.GetOptional<UnitPartMechadendrites>() != null)
			{
				return false;
			}
			return base.Blueprint.IsTwoHanded;
		}
	}

	public ItemEntityWeapon(BlueprintItemWeapon blueprint, string uniqueId, bool fake = false)
		: base(blueprint, uniqueId)
	{
		_fake = fake;
	}

	public ItemEntityWeapon([NotNull] BlueprintItemWeapon bpItem, ItemEntityShield shield = null)
		: base(bpItem)
	{
		Shield = shield;
	}

	protected ItemEntityWeapon(JsonConstructorMark _)
		: base(_)
	{
	}

	protected ItemEntityWeapon()
	{
	}

	protected override void OnReapplyFactsForWielder()
	{
		base.OnReapplyFactsForWielder();
		ReapplyAbilities();
	}

	private void ReapplyAbilities()
	{
		base.Abilities.ForEach(delegate(Ability v)
		{
			base.Wielder.Facts.Remove(v);
		});
		base.Abilities.Clear();
		MechanicEntity wielder = base.Wielder;
		if (base.Blueprint == null || wielder == null)
		{
			return;
		}
		base.Abilities.AddRange(base.Blueprint.WeaponAbilities.AllWithIndex.Where(((int Index, WeaponAbility Slot) i) => i.Slot.Ability != null).Select(delegate((int Index, WeaponAbility Slot) i)
		{
			Ability ability = base.Wielder.Facts.Add(new Ability(i.Slot.Ability, i.Index));
			if (ability != null)
			{
				ability.AddSource(this);
				return ability;
			}
			return ability;
		}).NotNull());
	}

	private void EnsureAbilitiesCoherency()
	{
		BlueprintItemWeapon blueprint = base.Blueprint;
		if (blueprint == null)
		{
			return;
		}
		bool flag = false;
		foreach (var item2 in blueprint.WeaponAbilities.AllWithIndex)
		{
			int index = item2.Index;
			WeaponAbility item = item2.Slot;
			Ability ability = base.Abilities.FirstItem((Ability i) => i != null && i.Data.IndexInItemSettings == index);
			flag = (ability == null && !item.IsNone && item.Ability != null) || (ability != null && (item.IsNone || item.Ability == null));
			if (flag)
			{
				break;
			}
		}
		if (flag)
		{
			ReapplyAbilities();
		}
	}

	public override void OnDidEquipped(MechanicEntity wielder)
	{
		base.OnDidEquipped(wielder);
		Second?.OnDidEquipped(wielder);
	}

	public override void OnWillUnequip()
	{
		Second?.OnWillUnequip();
		base.OnWillUnequip();
	}

	protected override void OnPreSave()
	{
		base.OnPreSave();
		Second?.PreSave();
	}

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		Second?.PostLoad();
	}

	protected override void OnDidPostLoad()
	{
		base.OnDidPostLoad();
		EnsureAbilitiesCoherency();
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		Second?.Dispose();
	}

	protected override void OnSubscribe()
	{
		base.OnSubscribe();
		Second?.Subscribe();
	}

	protected override void OnUnsubscribe()
	{
		Second?.Unsubscribe();
		base.OnUnsubscribe();
	}

	public void PostLoad(ItemEntityShield shield)
	{
		Shield = shield;
		PostLoad();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = ForceSecondary;
		result.Append(ref val2);
		Hash128 val3 = ClassHasher<ItemEntityWeapon>.GetHash128(Second);
		result.Append(ref val3);
		bool val4 = IsSecondPartOfDoubleWeapon;
		result.Append(ref val4);
		int val5 = CurrentUsedBarrel;
		result.Append(ref val5);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		ItemEntityWeapon source = new ItemEntityWeapon();
		result = Unsafe.As<ItemEntityWeapon, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ItemEntityWeapon>(OwlPackTypeInfo);
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
		BlueprintMechanicEntityFact value9 = base.VendorBlueprint;
		formatter.Field(26, "VendorBlueprint", ref value9, state);
		bool value10 = base.IsNonRemovable;
		formatter.UnmanagedField(27, "IsNonRemovable", ref value10, state);
		formatter.UnmanagedField(28, "_fake", ref _fake, state);
		bool value11 = ForceSecondary;
		formatter.UnmanagedField(29, "ForceSecondary", ref value11, state);
		ItemEntityWeapon value12 = Second;
		formatter.Field(30, "Second", ref value12, state);
		bool value13 = IsSecondPartOfDoubleWeapon;
		formatter.UnmanagedField(31, "IsSecondPartOfDoubleWeapon", ref value13, state);
		int value14 = CurrentUsedBarrel;
		formatter.UnmanagedField(32, "CurrentUsedBarrel", ref value14, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ItemEntityWeapon>();
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
				base.VendorBlueprint = formatter.ReadPackable<BlueprintMechanicEntityFact>(state);
				break;
			case 27:
				base.IsNonRemovable = formatter.ReadUnmanaged<bool>(state);
				break;
			case 28:
				_fake = formatter.ReadUnmanaged<bool>(state);
				break;
			case 29:
				ForceSecondary = formatter.ReadUnmanaged<bool>(state);
				break;
			case 30:
				Second = formatter.ReadPackable<ItemEntityWeapon>(state);
				break;
			case 31:
				IsSecondPartOfDoubleWeapon = formatter.ReadUnmanaged<bool>(state);
				break;
			case 32:
				CurrentUsedBarrel = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
