using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.Attributes;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[TypeId("d9a11879e7bd4893adfce6e4b8261428")]
public class ReplaceCurrentHandsEquipmentSet : UnitFactComponentDelegate
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class Data : IEntityFactComponentSavableData, IHashable, IOwlPackable<Data>
	{
		[JsonProperty]
		[OwlPackInclude]
		public BlueprintItem ReplacedWeaponPrimaryHand;

		[JsonProperty]
		[OwlPackInclude]
		public BlueprintItem ReplacedWeaponSecondaryHand;

		[JsonProperty]
		[OwlPackInclude]
		public int HandsEquipmentSetIndex;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "Data",
			OldNames = null,
			Fields = new FieldInfo[3]
			{
				new FieldInfo("ReplacedWeaponPrimaryHand", typeof(BlueprintItem)),
				new FieldInfo("ReplacedWeaponSecondaryHand", typeof(BlueprintItem)),
				new FieldInfo("HandsEquipmentSetIndex", typeof(int))
			}
		};

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			Hash128 val2 = SimpleBlueprintHasher.GetHash128(ReplacedWeaponPrimaryHand);
			result.Append(ref val2);
			Hash128 val3 = SimpleBlueprintHasher.GetHash128(ReplacedWeaponSecondaryHand);
			result.Append(ref val3);
			result.Append(ref HandsEquipmentSetIndex);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			Data source = new Data();
			result = Unsafe.As<Data, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<Data>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			formatter.Field(0, "ReplacedWeaponPrimaryHand", ref ReplacedWeaponPrimaryHand, state);
			formatter.Field(1, "ReplacedWeaponSecondaryHand", ref ReplacedWeaponSecondaryHand, state);
			formatter.UnmanagedField(2, "HandsEquipmentSetIndex", ref HandsEquipmentSetIndex, state);
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<Data>();
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
					ReplacedWeaponPrimaryHand = formatter.ReadPackable<BlueprintItem>(state);
					break;
				case 1:
					ReplacedWeaponSecondaryHand = formatter.ReadPackable<BlueprintItem>(state);
					break;
				case 2:
					HandsEquipmentSetIndex = formatter.ReadUnmanaged<int>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	[SerializeField]
	private BlueprintItemWeaponReference m_WeaponPrimaryHand;

	[SerializeField]
	[HideIf("IsTwoHanded")]
	private BlueprintItemWeaponReference m_WeaponSecondaryHand;

	public BlueprintItemWeapon WeaponPrimaryHand => m_WeaponPrimaryHand?.Get();

	private bool IsTwoHanded => WeaponPrimaryHand?.IsTwoHanded ?? false;

	public BlueprintItemWeapon WeaponSecondaryHand => m_WeaponSecondaryHand?.Get();

	protected override void OnActivate()
	{
		BaseUnitEntity owner = base.Fact.Owner;
		HandsEquipmentSet currentHandsEquipmentSet = owner.Body.CurrentHandsEquipmentSet;
		HandSlot primaryHand = currentHandsEquipmentSet.PrimaryHand;
		HandSlot secondaryHand = currentHandsEquipmentSet.SecondaryHand;
		Data data = RequestSavableData<Data>();
		data.ReplacedWeaponPrimaryHand = primaryHand.MaybeItem?.Blueprint;
		data.ReplacedWeaponSecondaryHand = secondaryHand.MaybeItem?.Blueprint;
		data.HandsEquipmentSetIndex = owner.Body.CurrentHandEquipmentSetIndex;
		ItemEntity maybeItem = primaryHand.MaybeItem;
		ItemEntity maybeItem2 = secondaryHand.MaybeItem;
		InsertWeapon(WeaponPrimaryHand, primaryHand);
		InsertWeapon(WeaponSecondaryHand, secondaryHand);
		maybeItem?.Collection?.Extract(maybeItem);
		maybeItem2?.Collection?.Extract(maybeItem2);
	}

	protected override void OnDeactivate()
	{
		BaseUnitEntity owner = base.Fact.Owner;
		Data data = RequestSavableData<Data>();
		HandsEquipmentSet handsEquipmentSet = owner.Body.HandsEquipmentSets[data.HandsEquipmentSetIndex];
		HandSlot primaryHand = handsEquipmentSet.PrimaryHand;
		HandSlot secondaryHand = handsEquipmentSet.SecondaryHand;
		ItemEntity maybeItem = primaryHand.MaybeItem;
		ItemEntity maybeItem2 = secondaryHand.MaybeItem;
		InsertWeapon(data.ReplacedWeaponPrimaryHand, primaryHand);
		InsertWeapon(data.ReplacedWeaponSecondaryHand, secondaryHand);
		maybeItem?.Collection?.Extract(maybeItem);
		maybeItem2?.Collection?.Extract(maybeItem2);
	}

	private void InsertWeapon(BlueprintItem weapon, HandSlot slot)
	{
		if (weapon == null)
		{
			slot.MaybeItem?.OnWillUnequip();
			slot.MaybeItem?.Dispose();
		}
		else
		{
			ItemEntity item = weapon.CreateEntity();
			slot.InsertItem(item, force: true);
		}
	}
}
