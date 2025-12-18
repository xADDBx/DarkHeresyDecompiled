using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem;
using Kingmaker.Gameplay.Components;
using Kingmaker.Gameplay.Features.Items.Utility;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Items.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class PartEmptyHandWeapons : MechanicEntityPart, IHashable, IOwlPackable<PartEmptyHandWeapons>
{
	[OwlPackInclude]
	private readonly List<EmptyHandWeaponEntry> _entries = new List<EmptyHandWeaponEntry>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartEmptyHandWeapons",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("_entries", typeof(List<EmptyHandWeaponEntry>))
		}
	};

	public void Add(EntityFact fact, AddEmptyHandWeapon settings)
	{
		EntityFact fact = fact;
		AddEmptyHandWeapon settings = settings;
		if (_entries.Contains((EmptyHandWeaponEntry i) => i.IsFrom(fact, settings)))
		{
			throw new ArgumentException("Empty hand weapon already added");
		}
		_entries.Add(new EmptyHandWeaponEntry(fact, settings));
		UpdateHands();
	}

	public void Remove(EntityFact fact, AddEmptyHandWeapon settings)
	{
		EntityFact fact = fact;
		AddEmptyHandWeapon settings = settings;
		_entries.Remove<EmptyHandWeaponEntry>((EmptyHandWeaponEntry x) => x.IsFrom(fact, settings));
		UpdateHands();
		if (_entries.Empty())
		{
			RemoveSelf();
		}
	}

	protected override void OnPostLoad()
	{
		_entries.RemoveAll((EmptyHandWeaponEntry i) => !i.IsValid);
		UpdateHands();
	}

	public void UpdateHands()
	{
		foreach (HandsEquipmentSet handsEquipmentSet in base.Owner.GetRequired<PartUnitBody>().HandsEquipmentSets)
		{
			bool hasTwoHandedWeapon = handsEquipmentSet.PrimaryHand.MaybeWeapon != null && handsEquipmentSet.PrimaryHand.MaybeWeapon.HoldInTwoHands;
			foreach (HandSlot handSlot in handsEquipmentSet.Hands)
			{
				EmptyHandWeaponEntry emptyHandWeaponEntry = _entries.FindLast((EmptyHandWeaponEntry i) => !hasTwoHandedWeapon && i.IsSuitableFor(handSlot));
				handSlot.SetEmptyHandWeapon(emptyHandWeaponEntry?.Blueprint);
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
		PartEmptyHandWeapons source = new PartEmptyHandWeapons();
		result = Unsafe.As<PartEmptyHandWeapons, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartEmptyHandWeapons>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		List<EmptyHandWeaponEntry> value = _entries;
		formatter.Field(0, "_entries", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartEmptyHandWeapons>();
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
				Unsafe.AsRef(in _entries) = formatter.ReadPackable<List<EmptyHandWeaponEntry>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
