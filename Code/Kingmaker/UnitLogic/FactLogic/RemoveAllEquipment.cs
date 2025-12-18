using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("0408ed03c2844bf89bb4a6569af75bf9")]
public class RemoveAllEquipment : UnitFactComponentDelegate
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class HandEquipmentSetItems : IHashable, IOwlPackable, IOwlPackable<HandEquipmentSetItems>
	{
		[JsonProperty]
		[OwlPackInclude]
		public BlueprintItem PrimaryHand;

		[JsonProperty]
		[OwlPackInclude]
		public BlueprintItem SecondaryHand;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "HandEquipmentSetItems",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("PrimaryHand", typeof(BlueprintItem)),
				new FieldInfo("SecondaryHand", typeof(BlueprintItem))
			}
		};

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = SimpleBlueprintHasher.GetHash128(PrimaryHand);
			result.Append(ref val);
			Hash128 val2 = SimpleBlueprintHasher.GetHash128(SecondaryHand);
			result.Append(ref val2);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			HandEquipmentSetItems source = new HandEquipmentSetItems();
			result = Unsafe.As<HandEquipmentSetItems, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<HandEquipmentSetItems>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			formatter.Field(0, "PrimaryHand", ref PrimaryHand, state);
			formatter.Field(1, "SecondaryHand", ref SecondaryHand, state);
			formatter.EndObject();
		}

		public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<HandEquipmentSetItems>();
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
					PrimaryHand = formatter.ReadPackable<BlueprintItem>(state);
					break;
				case 1:
					SecondaryHand = formatter.ReadPackable<BlueprintItem>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	[OwlPackable(OwlPackableMode.Generate)]
	public class Data : IEntityFactComponentSavableData, IHashable, IOwlPackable<Data>
	{
		[JsonProperty]
		[OwlPackInclude]
		public List<HandEquipmentSetItems> HandSets = new List<HandEquipmentSetItems>();

		[JsonProperty]
		[CanBeNull]
		[OwlPackInclude]
		public HandEquipmentSetItems PolymorphHandsEquipmentSet;

		[JsonProperty]
		[OwlPackInclude]
		public List<BlueprintItemEquipmentUsable> QuickSlots = new List<BlueprintItemEquipmentUsable>();

		[JsonProperty]
		[CanBeNull]
		[OwlPackInclude]
		public BlueprintItemArmor Armor;

		[JsonProperty]
		[CanBeNull]
		[OwlPackInclude]
		public BlueprintItemEquipmentShirt Shirt;

		[JsonProperty]
		[CanBeNull]
		[OwlPackInclude]
		public BlueprintItemEquipmentBelt Belt;

		[JsonProperty]
		[CanBeNull]
		[OwlPackInclude]
		public BlueprintItemEquipmentHead Head;

		[JsonProperty]
		[CanBeNull]
		[OwlPackInclude]
		public BlueprintItemEquipmentGlasses Glasses;

		[JsonProperty]
		[CanBeNull]
		[OwlPackInclude]
		public BlueprintItemEquipmentFeet Feet;

		[JsonProperty]
		[CanBeNull]
		[OwlPackInclude]
		public BlueprintItemEquipmentGloves Gloves;

		[JsonProperty]
		[CanBeNull]
		[OwlPackInclude]
		public BlueprintItemEquipmentNeck Neck;

		[JsonProperty]
		[CanBeNull]
		[OwlPackInclude]
		public BlueprintItemEquipmentRing Ring1;

		[JsonProperty]
		[CanBeNull]
		[OwlPackInclude]
		public BlueprintItemEquipmentRing Ring2;

		[JsonProperty]
		[CanBeNull]
		[OwlPackInclude]
		public BlueprintItemEquipmentWrist Wrist;

		[JsonProperty]
		[CanBeNull]
		[OwlPackInclude]
		public BlueprintItemEquipmentShoulders Shoulders;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "Data",
			OldNames = null,
			Fields = new FieldInfo[15]
			{
				new FieldInfo("HandSets", typeof(List<HandEquipmentSetItems>)),
				new FieldInfo("PolymorphHandsEquipmentSet", typeof(HandEquipmentSetItems)),
				new FieldInfo("QuickSlots", typeof(List<BlueprintItemEquipmentUsable>)),
				new FieldInfo("Armor", typeof(BlueprintItemArmor)),
				new FieldInfo("Shirt", typeof(BlueprintItemEquipmentShirt)),
				new FieldInfo("Belt", typeof(BlueprintItemEquipmentBelt)),
				new FieldInfo("Head", typeof(BlueprintItemEquipmentHead)),
				new FieldInfo("Glasses", typeof(BlueprintItemEquipmentGlasses)),
				new FieldInfo("Feet", typeof(BlueprintItemEquipmentFeet)),
				new FieldInfo("Gloves", typeof(BlueprintItemEquipmentGloves)),
				new FieldInfo("Neck", typeof(BlueprintItemEquipmentNeck)),
				new FieldInfo("Ring1", typeof(BlueprintItemEquipmentRing)),
				new FieldInfo("Ring2", typeof(BlueprintItemEquipmentRing)),
				new FieldInfo("Wrist", typeof(BlueprintItemEquipmentWrist)),
				new FieldInfo("Shoulders", typeof(BlueprintItemEquipmentShoulders))
			}
		};

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			List<HandEquipmentSetItems> handSets = HandSets;
			if (handSets != null)
			{
				for (int i = 0; i < handSets.Count; i++)
				{
					Hash128 val2 = ClassHasher<HandEquipmentSetItems>.GetHash128(handSets[i]);
					result.Append(ref val2);
				}
			}
			Hash128 val3 = ClassHasher<HandEquipmentSetItems>.GetHash128(PolymorphHandsEquipmentSet);
			result.Append(ref val3);
			List<BlueprintItemEquipmentUsable> quickSlots = QuickSlots;
			if (quickSlots != null)
			{
				for (int j = 0; j < quickSlots.Count; j++)
				{
					Hash128 val4 = SimpleBlueprintHasher.GetHash128(quickSlots[j]);
					result.Append(ref val4);
				}
			}
			Hash128 val5 = SimpleBlueprintHasher.GetHash128(Armor);
			result.Append(ref val5);
			Hash128 val6 = SimpleBlueprintHasher.GetHash128(Shirt);
			result.Append(ref val6);
			Hash128 val7 = SimpleBlueprintHasher.GetHash128(Belt);
			result.Append(ref val7);
			Hash128 val8 = SimpleBlueprintHasher.GetHash128(Head);
			result.Append(ref val8);
			Hash128 val9 = SimpleBlueprintHasher.GetHash128(Glasses);
			result.Append(ref val9);
			Hash128 val10 = SimpleBlueprintHasher.GetHash128(Feet);
			result.Append(ref val10);
			Hash128 val11 = SimpleBlueprintHasher.GetHash128(Gloves);
			result.Append(ref val11);
			Hash128 val12 = SimpleBlueprintHasher.GetHash128(Neck);
			result.Append(ref val12);
			Hash128 val13 = SimpleBlueprintHasher.GetHash128(Ring1);
			result.Append(ref val13);
			Hash128 val14 = SimpleBlueprintHasher.GetHash128(Ring2);
			result.Append(ref val14);
			Hash128 val15 = SimpleBlueprintHasher.GetHash128(Wrist);
			result.Append(ref val15);
			Hash128 val16 = SimpleBlueprintHasher.GetHash128(Shoulders);
			result.Append(ref val16);
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
			formatter.Field(0, "HandSets", ref HandSets, state);
			formatter.Field(1, "PolymorphHandsEquipmentSet", ref PolymorphHandsEquipmentSet, state);
			formatter.Field(2, "QuickSlots", ref QuickSlots, state);
			formatter.Field(3, "Armor", ref Armor, state);
			formatter.Field(4, "Shirt", ref Shirt, state);
			formatter.Field(5, "Belt", ref Belt, state);
			formatter.Field(6, "Head", ref Head, state);
			formatter.Field(7, "Glasses", ref Glasses, state);
			formatter.Field(8, "Feet", ref Feet, state);
			formatter.Field(9, "Gloves", ref Gloves, state);
			formatter.Field(10, "Neck", ref Neck, state);
			formatter.Field(11, "Ring1", ref Ring1, state);
			formatter.Field(12, "Ring2", ref Ring2, state);
			formatter.Field(13, "Wrist", ref Wrist, state);
			formatter.Field(14, "Shoulders", ref Shoulders, state);
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
					HandSets = formatter.ReadPackable<List<HandEquipmentSetItems>>(state);
					break;
				case 1:
					PolymorphHandsEquipmentSet = formatter.ReadPackable<HandEquipmentSetItems>(state);
					break;
				case 2:
					QuickSlots = formatter.ReadPackable<List<BlueprintItemEquipmentUsable>>(state);
					break;
				case 3:
					Armor = formatter.ReadPackable<BlueprintItemArmor>(state);
					break;
				case 4:
					Shirt = formatter.ReadPackable<BlueprintItemEquipmentShirt>(state);
					break;
				case 5:
					Belt = formatter.ReadPackable<BlueprintItemEquipmentBelt>(state);
					break;
				case 6:
					Head = formatter.ReadPackable<BlueprintItemEquipmentHead>(state);
					break;
				case 7:
					Glasses = formatter.ReadPackable<BlueprintItemEquipmentGlasses>(state);
					break;
				case 8:
					Feet = formatter.ReadPackable<BlueprintItemEquipmentFeet>(state);
					break;
				case 9:
					Gloves = formatter.ReadPackable<BlueprintItemEquipmentGloves>(state);
					break;
				case 10:
					Neck = formatter.ReadPackable<BlueprintItemEquipmentNeck>(state);
					break;
				case 11:
					Ring1 = formatter.ReadPackable<BlueprintItemEquipmentRing>(state);
					break;
				case 12:
					Ring2 = formatter.ReadPackable<BlueprintItemEquipmentRing>(state);
					break;
				case 13:
					Wrist = formatter.ReadPackable<BlueprintItemEquipmentWrist>(state);
					break;
				case 14:
					Shoulders = formatter.ReadPackable<BlueprintItemEquipmentShoulders>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	protected override void OnFactAttached()
	{
		base.OnFactAttached();
		using (ContextData<ItemsCollection.SuppressEvents>.Request())
		{
			Data data = base.Fact.RequestSavableData<Data>(this);
			PartUnitBody bodyOptional = base.Owner.GetBodyOptional();
			if (bodyOptional == null)
			{
				return;
			}
			data.Armor = bodyOptional.Armor.MaybeItem?.Blueprint as BlueprintItemArmor;
			UnequipItem(bodyOptional.Armor);
			data.Shirt = bodyOptional.Shirt.MaybeItem?.Blueprint as BlueprintItemEquipmentShirt;
			UnequipItem(bodyOptional.Shirt);
			data.Belt = bodyOptional.Belt.MaybeItem?.Blueprint as BlueprintItemEquipmentBelt;
			UnequipItem(bodyOptional.Belt);
			data.Head = bodyOptional.Head.MaybeItem?.Blueprint as BlueprintItemEquipmentHead;
			UnequipItem(bodyOptional.Head);
			data.Glasses = bodyOptional.Glasses.MaybeItem?.Blueprint as BlueprintItemEquipmentGlasses;
			UnequipItem(bodyOptional.Glasses);
			data.Feet = bodyOptional.Feet.MaybeItem?.Blueprint as BlueprintItemEquipmentFeet;
			UnequipItem(bodyOptional.Feet);
			data.Gloves = bodyOptional.Gloves.MaybeItem?.Blueprint as BlueprintItemEquipmentGloves;
			UnequipItem(bodyOptional.Gloves);
			data.Neck = bodyOptional.Neck.MaybeItem?.Blueprint as BlueprintItemEquipmentNeck;
			UnequipItem(bodyOptional.Neck);
			data.Ring1 = bodyOptional.Ring1.MaybeItem?.Blueprint as BlueprintItemEquipmentRing;
			UnequipItem(bodyOptional.Ring1);
			data.Ring2 = bodyOptional.Ring2.MaybeItem?.Blueprint as BlueprintItemEquipmentRing;
			UnequipItem(bodyOptional.Ring2);
			data.Wrist = bodyOptional.Wrist.MaybeItem?.Blueprint as BlueprintItemEquipmentWrist;
			UnequipItem(bodyOptional.Wrist);
			data.Shoulders = bodyOptional.Shoulders.MaybeItem?.Blueprint as BlueprintItemEquipmentShoulders;
			UnequipItem(bodyOptional.Shoulders);
			if (bodyOptional.QuickSlots != null)
			{
				UsableSlot[] quickSlots = bodyOptional.QuickSlots;
				foreach (UsableSlot usableSlot in quickSlots)
				{
					data.QuickSlots.Add(usableSlot.MaybeItem?.Blueprint as BlueprintItemEquipmentUsable);
					UnequipItem(usableSlot);
				}
			}
			foreach (HandsEquipmentSet handsEquipmentSet in bodyOptional.HandsEquipmentSets)
			{
				HandEquipmentSetItems item = new HandEquipmentSetItems
				{
					PrimaryHand = handsEquipmentSet.PrimaryHand.MaybeItem?.Blueprint,
					SecondaryHand = handsEquipmentSet.SecondaryHand.MaybeItem?.Blueprint
				};
				data.HandSets.Add(item);
				UnequipItem(handsEquipmentSet.PrimaryHand);
				UnequipItem(handsEquipmentSet.SecondaryHand);
			}
			HandsEquipmentSet polymorphHandsEquipmentSet = bodyOptional.PolymorphHandsEquipmentSet;
			if (polymorphHandsEquipmentSet != null)
			{
				data.PolymorphHandsEquipmentSet = new HandEquipmentSetItems
				{
					PrimaryHand = polymorphHandsEquipmentSet.PrimaryHand.MaybeItem?.Blueprint,
					SecondaryHand = polymorphHandsEquipmentSet.SecondaryHand.MaybeItem?.Blueprint
				};
				UnequipItem(polymorphHandsEquipmentSet.PrimaryHand);
				UnequipItem(polymorphHandsEquipmentSet.SecondaryHand);
			}
		}
	}

	protected override void OnFactDetached()
	{
		base.OnFactDetached();
		Data data = base.Fact.RequestSavableData<Data>(this);
		PartUnitBody bodyOptional = base.Owner.GetBodyOptional();
		if (bodyOptional != null)
		{
			EquipItem(data.Armor, bodyOptional.Armor);
			EquipItem(data.Shirt, bodyOptional.Shirt);
			EquipItem(data.Belt, bodyOptional.Belt);
			EquipItem(data.Head, bodyOptional.Head);
			EquipItem(data.Glasses, bodyOptional.Glasses);
			EquipItem(data.Feet, bodyOptional.Feet);
			EquipItem(data.Gloves, bodyOptional.Gloves);
			EquipItem(data.Neck, bodyOptional.Neck);
			EquipItem(data.Ring1, bodyOptional.Ring1);
			EquipItem(data.Ring2, bodyOptional.Ring2);
			EquipItem(data.Wrist, bodyOptional.Wrist);
			EquipItem(data.Shoulders, bodyOptional.Shoulders);
			for (int i = 0; i < data.QuickSlots.Count; i++)
			{
				EquipItem(data.QuickSlots[i], bodyOptional.QuickSlots[i]);
			}
			for (int j = 0; j < data.HandSets.Count; j++)
			{
				HandsEquipmentSet handsEquipmentSet = bodyOptional.HandsEquipmentSets[j];
				EquipItem(data.HandSets[j].PrimaryHand, handsEquipmentSet.PrimaryHand);
				EquipItem(data.HandSets[j].SecondaryHand, handsEquipmentSet.SecondaryHand);
			}
			HandEquipmentSetItems polymorphHandsEquipmentSet = data.PolymorphHandsEquipmentSet;
			EquipItem(polymorphHandsEquipmentSet?.PrimaryHand, bodyOptional.PolymorphHandsEquipmentSet?.PrimaryHand);
			EquipItem(polymorphHandsEquipmentSet?.SecondaryHand, bodyOptional.PolymorphHandsEquipmentSet?.SecondaryHand);
		}
	}

	private void UnequipItem(ItemSlot slot)
	{
		ItemEntity maybeItem = slot.MaybeItem;
		slot.MaybeItem?.OnWillUnequip();
		slot.MaybeItem?.Dispose();
		maybeItem?.Collection?.Extract(maybeItem);
	}

	private void EquipItem(BlueprintItem itemBp, ItemSlot slot)
	{
		if (slot != null)
		{
			slot.RemoveItem(autoMerge: true, force: true);
			if (itemBp != null)
			{
				ItemEntity item = itemBp.CreateEntity();
				slot.InsertItem(item, force: true);
			}
		}
	}
}
